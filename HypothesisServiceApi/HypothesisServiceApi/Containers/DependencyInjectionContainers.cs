using HypothesisServiceApi.Hypothesis;

namespace HypothesisServiceApi.Containers;

public static class DependencyInjectionContainers
{
    public static IServiceCollection AddHypothesisServices(
        this IServiceCollection services)
    {
        services.AddSingleton<IHypothesisRepository, InMemoryHypothesisRepository>();
        services.AddScoped<IHypothesisService, HypothesisService>();

        return services;
    }
}