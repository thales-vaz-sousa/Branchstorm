using BranchstormDomain.Infrastructure.Database;
using BranchstormDomain.Infrastructure.Database.SqLite;
using TeamApiClassLibrary.Team.Repositories;
using TeamApiClassLibrary.Team.Services;

namespace TeamApi.Containers;

public static class DependencyInjectionContainers
{
    public static IServiceCollection AddDependencyInjectionContainers(
        this IServiceCollection services)
    {
        services.AddSingleton<IDatabaseConnection, SqliteConnectionFactory>();
        services.AddScoped<DatabaseInitializer>();

        services.UseTeamService();
        services.UseTeamRepository();

        return services;
    }
}
