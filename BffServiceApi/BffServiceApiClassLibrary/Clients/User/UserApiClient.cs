using BffApiClassLibrary.Common;
using Microsoft.Extensions.DependencyInjection;

namespace BffApiClassLibrary.Clients.User;

// Contratos do UserServiceApi (/api/user)
public record UserResult
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
}

public record UserCreateRequest(string Name, string Email);

public interface IUserApiClient
{
    string ServiceName { get; }
    Task<UserResult> CreateAsync(UserCreateRequest request, CancellationToken ct = default);
    Task<UserResult?> FindOneAsync(string code, CancellationToken ct = default);
    Task<IReadOnlyList<UserResult>> SearchAsync(string? name, string? email, CancellationToken ct = default);
    Task<IReadOnlyList<UserResult>> FindByCodesAsync(IEnumerable<string> codes, CancellationToken ct = default);
    Task<(bool Up, long LatencyMs, string? Error)> PingAsync(CancellationToken ct);
}

public class UserApiClient(HttpClient httpClient) : UpstreamClient(httpClient, "UserApi"), IUserApiClient
{
    public Task<UserResult> CreateAsync(UserCreateRequest request, CancellationToken ct = default) =>
        SendAsync<UserResult>(HttpMethod.Post, "api/user", request, ct);

    public Task<UserResult?> FindOneAsync(string code, CancellationToken ct = default) =>
        GetOrDefaultAsync<UserResult>($"api/user/{Uri.EscapeDataString(code)}", ct);

    public Task<IReadOnlyList<UserResult>> SearchAsync(string? name, string? email, CancellationToken ct = default)
    {
        List<string> parameters = [];
        if (!string.IsNullOrWhiteSpace(name)) parameters.Add($"name={Uri.EscapeDataString(name)}");
        if (!string.IsNullOrWhiteSpace(email)) parameters.Add($"email={Uri.EscapeDataString(email)}");

        string url = parameters.Count == 0 ? "api/user" : $"api/user?{string.Join("&", parameters)}";
        return GetListAsync<UserResult>(url, ct);
    }

    /// <summary>Busca em lote (GET /api/user?codes=...), em blocos de 100.</summary>
    public Task<IReadOnlyList<UserResult>> FindByCodesAsync(IEnumerable<string> codes, CancellationToken ct = default) =>
        Batch.FetchInChunksAsync(codes, chunk =>
            GetListAsync<UserResult>($"api/user?{Batch.Query("codes", chunk)}", ct));
}

public static class UserApiClientUtil
{
    public static IServiceCollection UseUserApiClient(this IServiceCollection services, string baseUrl, TimeSpan timeout)
    {
        services.AddHttpClient<IUserApiClient, UserApiClient>(client =>
        {
            client.BaseAddress = UpstreamClientUtil.ToBaseUri(baseUrl);
            client.Timeout = timeout;
        }).AddHttpMessageHandler<CorrelationIdHandler>();

        return services;
    }
}
