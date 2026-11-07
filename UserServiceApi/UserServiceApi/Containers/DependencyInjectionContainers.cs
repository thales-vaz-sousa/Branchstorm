using BranchstormDomain.Infrastructure.Database;
using BranchstormDomain.Infrastructure.Database.SqLite;
using UserApiClassLibrary.User.Repositories;
using UserApiClassLibrary.User.Services;

namespace UserApi.Containers;

public static class DependencyInjectionContaine 
{
    public static IServiceCollection AddDependencyInjectionContainers(
        this IServiceCollection services)
    {
        services.AddSingleton<IDatabaseConnection, SqliteConnectionFactory>();
        services.AddScoped<DatabaseInitializer>();

        services.UseUserService();
        services.UseUserRepository();

        return services;
    }
}