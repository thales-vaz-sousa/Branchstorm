using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace BffApiClassLibrary.Common;

/// <summary>
/// Base dos clientes HTTP dos microsserviços. Centraliza o tratamento de erro:
/// 404 vira null nas consultas, demais falhas viram <see cref="UpstreamException"/>.
/// </summary>
public abstract class UpstreamClient(HttpClient httpClient, string serviceName)
{
    protected HttpClient Http { get; } = httpClient;
    public string ServiceName { get; } = serviceName;

    /// <summary>GET que devolve null quando o recurso não existe (404).</summary>
    protected async Task<T?> GetOrDefaultAsync<T>(string url, CancellationToken ct) where T : class
    {
        using HttpResponseMessage response = await SendRawAsync(HttpMethod.Get, url, null, ct);
        if (response.StatusCode is HttpStatusCode.NotFound) return null;

        await EnsureSuccessAsync(response, ct);
        return await ReadAsync<T>(response, ct);
    }

    /// <summary>GET de lista: 404 vira lista vazia.</summary>
    protected async Task<IReadOnlyList<T>> GetListAsync<T>(string url, CancellationToken ct)
    {
        using HttpResponseMessage response = await SendRawAsync(HttpMethod.Get, url, null, ct);
        if (response.StatusCode is HttpStatusCode.NotFound) return [];

        await EnsureSuccessAsync(response, ct);
        return await ReadAsync<List<T>>(response, ct) ?? [];
    }

    /// <summary>Comando (POST/PUT/PATCH). 404 vira null; outras falhas lancam excecao.</summary>
    protected async Task<T?> SendOrDefaultAsync<T>(HttpMethod method, string url, object? body, CancellationToken ct) where T : class
    {
        using HttpResponseMessage response = await SendRawAsync(method, url, body, ct);
        if (response.StatusCode is HttpStatusCode.NotFound) return null;

        await EnsureSuccessAsync(response, ct);
        return await ReadAsync<T>(response, ct);
    }

    /// <summary>Comando que precisa obrigatoriamente de resposta.</summary>
    protected async Task<T> SendAsync<T>(HttpMethod method, string url, object? body, CancellationToken ct) where T : class
    {
        using HttpResponseMessage response = await SendRawAsync(method, url, body, ct);
        await EnsureSuccessAsync(response, ct);

        return await ReadAsync<T>(response, ct)
            ?? throw new UpstreamException(ServiceName, 502, "Resposta inválida do serviço",
                $"O serviço {ServiceName} respondeu sem corpo.");
    }

    /// <summary>Verifica se o serviço responde (qualquer status HTTP conta como "no ar").</summary>
    public async Task<(bool Up, long LatencyMs, string? Error)> PingAsync(CancellationToken ct)
    {
        Stopwatch watch = Stopwatch.StartNew();
        try
        {
            using HttpResponseMessage _ = await Http.GetAsync("", ct);
            return (true, watch.ElapsedMilliseconds, null);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or InvalidOperationException)
        {
            return (false, watch.ElapsedMilliseconds, ex.Message);
        }
    }

    private async Task<HttpResponseMessage> SendRawAsync(HttpMethod method, string url, object? body, CancellationToken ct)
    {
        using HttpRequestMessage request = new(method, url);
        if (body is not null)
        {
            // Corpo com Content-Length fixo (sem chunked), mais compativel com proxies/load balancers.
            string json = JsonSerializer.Serialize(body, body.GetType(), JsonSerializerOptions.Web);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        try
        {
            return await Http.SendAsync(request, ct);
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        {
            throw new UpstreamException(ServiceName, 504, "Serviço demorou para responder",
                $"Tempo esgotado ao chamar o serviço {ServiceName}.");
        }
        catch (HttpRequestException ex)
        {
            throw new UpstreamException(ServiceName, 503, "Serviço indisponível",
                $"Não foi possível conectar ao serviço {ServiceName}: {ex.Message}");
        }
        catch (InvalidOperationException ex)
        {
            // BaseUrl não configurada
            throw new UpstreamException(ServiceName, 503, "Serviço não configurado",
                $"Serviço {ServiceName} sem BaseUrl válida: {ex.Message}");
        }
    }

    private async Task<T?> ReadAsync<T>(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.Content.Headers.ContentLength is 0) return default;

        try
        {
            return await response.Content.ReadFromJsonAsync<T>(cancellationToken: ct);
        }
        catch (JsonException ex)
        {
            throw new UpstreamException(ServiceName, 502, "Resposta inválida do serviço",
                $"O serviço {ServiceName} devolveu um JSON inesperado: {ex.Message}");
        }
    }

    private async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode) return;

        string body = await response.Content.ReadAsStringAsync(ct);
        int upstream = (int)response.StatusCode;
        string message = UpstreamErrorParser.ExtractMessage(body);

        // Os microsserviços lancam ArgumentException para erro de validação, o que chega como 500.
        // Para o front isso e um erro de entrada, entao devolvemos 400 com a mensagem original.
        if (UpstreamErrorParser.IsValidationError(body) || upstream is 400 or 422)
        {
            throw new UpstreamException(ServiceName, 400, "Dados inválidos", message, upstream);
        }

        if (upstream is 404 or 409)
        {
            throw new UpstreamException(ServiceName, upstream,
                upstream == 404 ? "Recurso não encontrado" : "Conflito", message, upstream);
        }

        throw new UpstreamException(ServiceName, 502, "Falha no serviço",
            $"O serviço {ServiceName} respondeu {upstream}. {message}".Trim(), upstream);
    }
}

internal static class UpstreamErrorParser
{
    public static bool IsValidationError(string body) =>
        body.Contains("ArgumentException", StringComparison.Ordinal)
        || body.Contains("Erro de validação", StringComparison.Ordinal)
        || body.Contains("\"errors\"", StringComparison.Ordinal);

    /// <summary>
    /// Pega uma mensagem legivel do corpo do erro (ProblemDetails, texto da excecao ou corpo cru).
    /// </summary>
    public static string ExtractMessage(string body)
    {
        if (string.IsNullOrWhiteSpace(body)) return string.Empty;

        string trimmed = body.TrimStart();
        if (trimmed.StartsWith('{'))
        {
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(trimmed);
                var root = doc.RootElement;
                if (root.TryGetProperty("errors", out var errors) && errors.ValueKind == System.Text.Json.JsonValueKind.Object)
                {
                    var messages = errors.EnumerateObject()
                        .SelectMany(p => p.Value.ValueKind == System.Text.Json.JsonValueKind.Array
                            ? p.Value.EnumerateArray().Select(v => $"{p.Name}: {v.GetString()}")
                            : [$"{p.Name}: {p.Value}"]);
                    return Limit(string.Join("; ", messages));
                }
                if (root.TryGetProperty("detail", out var detail) && detail.ValueKind == System.Text.Json.JsonValueKind.String)
                    return Limit(detail.GetString() ?? string.Empty);
                if (root.TryGetProperty("title", out var title) && title.ValueKind == System.Text.Json.JsonValueKind.String)
                    return Limit(title.GetString() ?? string.Empty);
            }
            catch (System.Text.Json.JsonException)
            {
                // segue para o tratamento como texto
            }
        }

        // Pagina de excecao do ASP.NET em texto: "System.ArgumentException: mensagem (Parameter 'x')\n   at ..."
        string firstLine = trimmed.Split('\n', 2)[0].Trim();
        int colon = firstLine.IndexOf("Exception: ", StringComparison.Ordinal);
        if (colon >= 0)
        {
            firstLine = firstLine[(colon + "Exception: ".Length)..];
        }

        return Limit(firstLine);
    }

    private static string Limit(string value) => value.Length <= 300 ? value : value[..300] + "...";
}
