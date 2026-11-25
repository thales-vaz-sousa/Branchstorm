using BffApiClassLibrary.Common;
using Microsoft.Extensions.DependencyInjection;

namespace BffApiClassLibrary.Clients.Idea;

// Contratos do IdeaServiceApi (/api/idea)
public record LinkHistoryResult
{
    public string Code { get; init; } = string.Empty;
    public string LinkCode { get; init; } = string.Empty;
    public string Event { get; init; } = string.Empty;
    public DateTime CreationDate { get; init; }
}

public record LinkResult
{
    public string Code { get; init; } = string.Empty;
    public string? FatherCode { get; init; }
    public string Type { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Status { get; init; } = string.Empty;
    public string Validation { get; init; } = string.Empty;
    public string? AutorCode { get; init; }
    public string? TeamCode { get; init; }
    public string? Reason { get; init; }
    public DateTime CreationDate { get; init; }
    public IReadOnlyList<LinkHistoryResult> History { get; init; } = [];
}

/// <summary>Type em snake_case ("problem" | "solution" | "version"), como o serviço espera.</summary>
public record LinkCreateRequest(
    string? FatherCode,
    string Type,
    string Title,
    string? Description,
    string? AutorCode,
    string? TeamCode,
    string? Reason);

public record LinkValidationRequest(string Validation, string? Reason);

public record LinkSelectRequest(string? Reason);

public interface IIdeaApiClient
{
    string ServiceName { get; }
    Task<LinkResult> CreateAsync(LinkCreateRequest request, CancellationToken ct = default);
    Task<LinkResult?> FindOneAsync(string code, CancellationToken ct = default);
    /// <summary>O nó informado e todos os descendentes (CTE recursiva), cada um com seu histórico.</summary>
    Task<IReadOnlyList<LinkResult>> FindTreeAsync(string code, CancellationToken ct = default);
    Task<LinkResult?> SelectAsync(string code, string? reason, CancellationToken ct = default);
    Task<LinkResult?> UpdateValidationAsync(string code, string validation, string? reason, CancellationToken ct = default);
    Task<(bool Up, long LatencyMs, string? Error)> PingAsync(CancellationToken ct);
}

public class IdeaApiClient(HttpClient httpClient) : UpstreamClient(httpClient, "IdeaApi"), IIdeaApiClient
{
    public Task<LinkResult> CreateAsync(LinkCreateRequest request, CancellationToken ct = default) =>
        SendAsync<LinkResult>(HttpMethod.Post, "api/idea", request, ct);

    public Task<LinkResult?> FindOneAsync(string code, CancellationToken ct = default) =>
        GetOrDefaultAsync<LinkResult>($"api/idea/{Uri.EscapeDataString(code)}", ct);

    public Task<IReadOnlyList<LinkResult>> FindTreeAsync(string code, CancellationToken ct = default) =>
        GetListAsync<LinkResult>($"api/idea/tree/{Uri.EscapeDataString(code)}", ct);

    public Task<LinkResult?> SelectAsync(string code, string? reason, CancellationToken ct = default) =>
        SendOrDefaultAsync<LinkResult>(HttpMethod.Put, $"api/idea/select/{Uri.EscapeDataString(code)}",
            new LinkSelectRequest(reason), ct);

    public Task<LinkResult?> UpdateValidationAsync(string code, string validation, string? reason, CancellationToken ct = default) =>
        SendOrDefaultAsync<LinkResult>(HttpMethod.Put, $"api/idea/validation/{Uri.EscapeDataString(code)}",
            new LinkValidationRequest(validation, reason), ct);
}

public static class IdeaApiClientUtil
{
    public static IServiceCollection UseIdeaApiClient(this IServiceCollection services, string baseUrl, TimeSpan timeout)
    {
        services.AddHttpClient<IIdeaApiClient, IdeaApiClient>(client =>
        {
            client.BaseAddress = UpstreamClientUtil.ToBaseUri(baseUrl);
            client.Timeout = timeout;
        }).AddHttpMessageHandler<CorrelationIdHandler>();

        return services;
    }
}
