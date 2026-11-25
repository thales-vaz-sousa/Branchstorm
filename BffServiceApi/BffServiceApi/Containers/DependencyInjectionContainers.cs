using BffApiClassLibrary.Clients.Feedback;
using BffApiClassLibrary.Clients.Hypothesis;
using BffApiClassLibrary.Clients.Idea;
using BffApiClassLibrary.Clients.Team;
using BffApiClassLibrary.Clients.User;
using BffApiClassLibrary.Common;
using BffApiClassLibrary.Services.Export;
using BffApiClassLibrary.Services.Health;
using BffApiClassLibrary.Services.Ideas;
using BffApiClassLibrary.Services.Teams;

namespace BffServiceApi.Containers;

public static class DependencyInjectionContainers
{
    public static IServiceCollection AddDependencyInjectionContainers(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddTransient<CorrelationIdHandler>();

        TimeSpan timeout = TimeSpan.FromSeconds(configuration.GetValue("Upstream:TimeoutSeconds", 15));

        // Clientes HTTP dos microsserviços (mesmas chaves de configuracao do gateway)
        services.UseUserApiClient(configuration["UserApi:BaseUrl"] ?? string.Empty, timeout);
        services.UseTeamApiClient(configuration["TeamApi:BaseUrl"] ?? string.Empty, timeout);
        services.UseIdeaApiClient(configuration["IdeaApi:BaseUrl"] ?? string.Empty, timeout);
        services.UseHypothesisApiClient(configuration["HypothesisApi:BaseUrl"] ?? string.Empty, timeout);
        services.UseFeedbackApiClient(configuration["FeedbackApi:BaseUrl"] ?? string.Empty, timeout);

        // Serviços de agregacao por tela
        services.UseIdeaTreeService();
        services.UseLinkDetailService();
        services.UseIdeaCommandService();
        services.UseTeamViewService();
        services.UseExportService();
        services.UseHealthService();

        return services;
    }
}
