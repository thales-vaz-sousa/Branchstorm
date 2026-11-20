using Microsoft.Extensions.DependencyInjection;

namespace BranchstormApiClientClassLibrary.Idea.Services.ApiServices;

public static class IdeaApiServiceUtil
{
    public static IServiceCollection UseIdeaApiService(this IServiceCollection services, string baseUrl)
    {
        services.AddHttpClient<IIdeaApiService, IdeaApiService>(client =>
        {
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        return services;
    }
}
