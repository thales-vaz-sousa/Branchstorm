using Microsoft.Extensions.DependencyInjection;

namespace FeedbackApiClassLibrary.Feedback.Repositories;

public static class FeedbackRepositoryUtil
{
    public static IServiceCollection UseFeedbackRepository(this IServiceCollection services)
    {
        services.AddScoped<IFeedbackRepository, FeedbackRepository>();
        return services;
    }
}
