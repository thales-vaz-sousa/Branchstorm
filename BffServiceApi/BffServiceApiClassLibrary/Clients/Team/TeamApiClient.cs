using BffApiClassLibrary.Common;
using Microsoft.Extensions.DependencyInjection;

namespace BffApiClassLibrary.Clients.Team;

// Contratos do TeamServiceApi (/api/team)
public record TeamMemberResult
{
    public string? TeamCode { get; init; }
    public string UserCode { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
}

public record TeamResult
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public IReadOnlyList<TeamMemberResult> Members { get; init; } = [];
}

public record TeamMemberRequest(string UserCode, string Role);

/// <summary>O PATCH do serviço substitui a lista inteira de membros e exige Name/Description.</summary>
public record TeamUpsertRequest(string Name, string Description, IReadOnlyList<TeamMemberRequest> Members);

public interface ITeamApiClient
{
    string ServiceName { get; }
    Task<TeamResult> CreateAsync(TeamUpsertRequest request, CancellationToken ct = default);
    Task<TeamResult?> FindOneAsync(string code, CancellationToken ct = default);
    Task<TeamResult?> ReplaceMembersAsync(string code, TeamUpsertRequest request, CancellationToken ct = default);
    Task<(bool Up, long LatencyMs, string? Error)> PingAsync(CancellationToken ct);
}

public class TeamApiClient(HttpClient httpClient) : UpstreamClient(httpClient, "TeamApi"), ITeamApiClient
{
    public Task<TeamResult> CreateAsync(TeamUpsertRequest request, CancellationToken ct = default) =>
        SendAsync<TeamResult>(HttpMethod.Post, "api/team", request, ct);

    public Task<TeamResult?> FindOneAsync(string code, CancellationToken ct = default) =>
        GetOrDefaultAsync<TeamResult>($"api/team/{Uri.EscapeDataString(code)}", ct);

    public Task<TeamResult?> ReplaceMembersAsync(string code, TeamUpsertRequest request, CancellationToken ct = default) =>
        SendOrDefaultAsync<TeamResult>(HttpMethod.Patch, $"api/team/{Uri.EscapeDataString(code)}", request, ct);
}

public static class TeamApiClientUtil
{
    public static IServiceCollection UseTeamApiClient(this IServiceCollection services, string baseUrl, TimeSpan timeout)
    {
        services.AddHttpClient<ITeamApiClient, TeamApiClient>(client =>
        {
            client.BaseAddress = UpstreamClientUtil.ToBaseUri(baseUrl);
            client.Timeout = timeout;
        }).AddHttpMessageHandler<CorrelationIdHandler>();

        return services;
    }
}
