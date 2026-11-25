using Microsoft.Extensions.DependencyInjection;

namespace BranchstormApiClientClassLibrary.Feedback.Services.ApiServices;

public static class FeedbackApiServiceUtil
{
    public static IServiceCollection UseFeedbackApiService(this IServiceCollection services, string baseUrl)
    {
        services.AddHttpClient<IFeedbackApiService, FeedbackApiService>(client =>
        {
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        return services;
    }
}
