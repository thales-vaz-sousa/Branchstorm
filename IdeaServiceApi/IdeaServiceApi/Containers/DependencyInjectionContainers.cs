using BranchstormDomain.Infrastructure.Database;
using BranchstormDomain.Infrastructure.Database.SqLite;
using IdeaApiClassLibrary.Link.Repositories;
using IdeaApiClassLibrary.Link.Services;

namespace IdeaApi.Containers;

public static class DependencyInjectionContainers
{
    public static IServiceCollection AddDependencyInjectionContainers(
        this IServiceCollection services)
    {
        services.AddSingleton<IDatabaseConnection, SqliteConnectionFactory>();
        services.AddScoped<DatabaseInitializer>();

        services.UseLinkService();
        services.UseLinkRepository();

        return services;
    }
}
