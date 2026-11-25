using BffApiClassLibrary.Common;
using Microsoft.Extensions.DependencyInjection;

namespace BffApiClassLibrary.Clients.Hypothesis;

// Contratos do HypothesisServiceApi (/api/hypothesis)
public record HypothesisResult
{
    public string Code { get; init; } = string.Empty;
    public string LinkCode { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string SuccessCriteria { get; init; } = string.Empty;
    public string? Result { get; init; }
    public DateTime CreationDate { get; init; }
}

public record HypothesisCreateRequest(string LinkCode, string Description, string SuccessCriteria);

public record HypothesisResultRequest(string Result);

public interface IHypothesisApiClient
{
    string ServiceName { get; }
    Task<HypothesisResult> CreateAsync(HypothesisCreateRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<HypothesisResult>> FindByLinkAsync(string linkCode, CancellationToken ct = default);
    Task<IReadOnlyList<HypothesisResult>> FindByLinksAsync(IEnumerable<string> linkCodes, CancellationToken ct = default);
    /// <summary>Registra o resultado; o proprio serviço de Hipóteses atualiza a validação do nó em Ideias.</summary>
    Task<HypothesisResult> RegisterResultAsync(string code, string result, CancellationToken ct = default);
    Task<(bool Up, long LatencyMs, string? Error)> PingAsync(CancellationToken ct);
}

public class HypothesisApiClient(HttpClient httpClient) : UpstreamClient(httpClient, "HypothesisApi"), IHypothesisApiClient
{
    public Task<HypothesisResult> CreateAsync(HypothesisCreateRequest request, CancellationToken ct = default) =>
        SendAsync<HypothesisResult>(HttpMethod.Post, "api/hypothesis", request, ct);

    public Task<IReadOnlyList<HypothesisResult>> FindByLinkAsync(string linkCode, CancellationToken ct = default) =>
        GetListAsync<HypothesisResult>($"api/hypothesis?linkCode={Uri.EscapeDataString(linkCode)}", ct);

    public Task<IReadOnlyList<HypothesisResult>> FindByLinksAsync(IEnumerable<string> linkCodes, CancellationToken ct = default) =>
        Batch.FetchInChunksAsync(linkCodes, chunk =>
            GetListAsync<HypothesisResult>($"api/hypothesis?{Batch.Query("linkCodes", chunk)}", ct));

    public Task<HypothesisResult> RegisterResultAsync(string code, string result, CancellationToken ct = default) =>
        SendAsync<HypothesisResult>(HttpMethod.Put, $"api/hypothesis/result/{Uri.EscapeDataString(code)}",
            new HypothesisResultRequest(result), ct);
}

public static class HypothesisApiClientUtil
{
    public static IServiceCollection UseHypothesisApiClient(this IServiceCollection services, string baseUrl, TimeSpan timeout)
    {
        services.AddHttpClient<IHypothesisApiClient, HypothesisApiClient>(client =>
        {
            client.BaseAddress = UpstreamClientUtil.ToBaseUri(baseUrl);
            client.Timeout = timeout;
        }).AddHttpMessageHandler<CorrelationIdHandler>();

        return services;
    }
}
