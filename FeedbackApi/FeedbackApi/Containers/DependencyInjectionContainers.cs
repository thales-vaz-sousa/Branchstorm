using BranchstormDomain.Infrastructure.Database;
using BranchstormDomain.Infrastructure.Database.SqLite;
using FeedbackApiClassLibrary.Feedback.Repositories;
using FeedbackApiClassLibrary.Feedback.Services;

namespace FeedbackApi.Containers;

public static class DependencyInjectionContainers
{
    public static IServiceCollection AddDependencyInjectionContainers(this IServiceCollection services)
    {
        services.AddSingleton<IDatabaseConnection, SqliteConnectionFactory>();
        services.AddScoped<DatabaseInitializer>();

        services.UseFeedbackService();
        services.UseFeedbackRepository();

        return services;
    }
}
