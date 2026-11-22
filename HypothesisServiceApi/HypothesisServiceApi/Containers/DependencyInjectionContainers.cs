using BranchstormDomain.Infrastructure.Database;
using BranchstormDomain.Infrastructure.Database.SqLite;
using HypothesisServiceApi.Hypothesis.Repositories;
using HypothesisServiceApi.Hypothesis.Services;

namespace HypothesisServiceApi.Containers;

public static class DependencyInjectionContainers
{
    public static IServiceCollection AddDependencyInjectionContainers(
        this IServiceCollection services)
    {
        services.AddSingleton<IDatabaseConnection, SqliteConnectionFactory>();
        services.AddScoped<DatabaseInitializer>();

        services.UseHypothesisService();
        services.UseHypothesisRepository();

        return services;
    }
}
