using Microsoft.Extensions.DependencyInjection;

namespace FeedbackApiClassLibrary.Feedback.Services;

public static class FeedbackServiceUtil
{
    public static IServiceCollection UseFeedbackService(this IServiceCollection services)
    {
        services.AddTransient<IFeedbackService, FeedbackService>();
        return services;
    }
}
