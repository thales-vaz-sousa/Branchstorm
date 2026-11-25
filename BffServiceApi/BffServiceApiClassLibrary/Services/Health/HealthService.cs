using BffApiClassLibrary.Clients.Feedback;
using BffApiClassLibrary.Clients.Hypothesis;
using BffApiClassLibrary.Clients.Idea;
using BffApiClassLibrary.Clients.Team;
using BffApiClassLibrary.Clients.User;
using BffApiClassLibrary.Views;
using Microsoft.Extensions.DependencyInjection;

namespace BffApiClassLibrary.Services.Health;

public interface IHealthService
{
    Task<HealthView> CheckAsync(CancellationToken ct = default);
}

/// <summary>Consulta todos os microsserviços em paralelo; usado pela tela de status e pelo health check do Render.</summary>
public class HealthService(
    IUserApiClient users,
    ITeamApiClient teams,
    IIdeaApiClient ideas,
    IHypothesisApiClient hypotheses,
    IFeedbackApiClient feedbacks) : IHealthService
{
    public async Task<HealthView> CheckAsync(CancellationToken ct = default)
    {
        (string Name, Task<(bool Up, long LatencyMs, string? Error)> Probe)[] probes =
        [
            (users.ServiceName, users.PingAsync(ct)),
            (teams.ServiceName, teams.PingAsync(ct)),
            (ideas.ServiceName, ideas.PingAsync(ct)),
            (hypotheses.ServiceName, hypotheses.PingAsync(ct)),
            (feedbacks.ServiceName, feedbacks.PingAsync(ct))
        ];

        await Task.WhenAll(probes.Select(p => p.Probe));

        List<ServiceHealth> services = probes
            .Select(p =>
            {
                (bool up, long latency, string? error) = p.Probe.Result;
                return new ServiceHealth(p.Name, up ? "up" : "down", latency, error);
            })
            .ToList();

        string status = services.All(s => s.Status == "up") ? "healthy"
            : services.Any(s => s.Status == "up") ? "degraded"
            : "unhealthy";

        return new HealthView(status, DateTime.UtcNow, services);
    }
}

public static class HealthServiceUtil
{
    public static IServiceCollection UseHealthService(this IServiceCollection services)
    {
        services.AddTransient<IHealthService, HealthService>();
        return services;
    }
}
