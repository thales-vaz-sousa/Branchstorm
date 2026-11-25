using BffApiClassLibrary.Common;
using Microsoft.Extensions.DependencyInjection;

namespace BffApiClassLibrary.Clients.Feedback;

// Contratos do FeedbackApi (/api/feedbacks)
public record FeedbackResult
{
    public string Code { get; init; } = string.Empty;
    public string LinkCode { get; init; } = string.Empty;
    public string AutorCode { get; init; } = string.Empty;
    public string Text { get; init; } = string.Empty;
    public decimal Rating { get; init; }
    public DateTime CreationDate { get; init; }
}

public record FeedbackCreateRequest(string LinkCode, string AutorCode, string Text, decimal Rating);

public interface IFeedbackApiClient
{
    string ServiceName { get; }
    Task<FeedbackResult> CreateAsync(FeedbackCreateRequest request, CancellationToken ct = default);
    /// <summary>
    /// Sempre usa a consulta em lote (GET /api/feedbacks?linkCodes=...), inclusive para um no so:
    /// o GET /api/feedbacks/{linkCode} do serviço devolve apenas um feedback.
    /// </summary>
    Task<IReadOnlyList<FeedbackResult>> FindByLinksAsync(IEnumerable<string> linkCodes, CancellationToken ct = default);
    Task<(bool Up, long LatencyMs, string? Error)> PingAsync(CancellationToken ct);
}

public class FeedbackApiClient(HttpClient httpClient) : UpstreamClient(httpClient, "FeedbackApi"), IFeedbackApiClient
{
    public Task<FeedbackResult> CreateAsync(FeedbackCreateRequest request, CancellationToken ct = default) =>
        SendAsync<FeedbackResult>(HttpMethod.Post, "api/feedbacks", request, ct);

    public Task<IReadOnlyList<FeedbackResult>> FindByLinksAsync(IEnumerable<string> linkCodes, CancellationToken ct = default) =>
        Batch.FetchInChunksAsync(linkCodes, chunk =>
            GetListAsync<FeedbackResult>($"api/feedbacks?{Batch.Query("linkCodes", chunk)}", ct));
}

public static class FeedbackApiClientUtil
{
    public static IServiceCollection UseFeedbackApiClient(this IServiceCollection services, string baseUrl, TimeSpan timeout)
    {
        services.AddHttpClient<IFeedbackApiClient, FeedbackApiClient>(client =>
        {
            client.BaseAddress = UpstreamClientUtil.ToBaseUri(baseUrl);
            client.Timeout = timeout;
        }).AddHttpMessageHandler<CorrelationIdHandler>();

        return services;
    }
}
