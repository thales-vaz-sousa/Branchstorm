using Microsoft.Extensions.DependencyInjection;

namespace BranchstormApiClientClassLibrary.Hypothesis.Services.ApiServices;

public static class HypothesisApiServiceUtil
{
    public static IServiceCollection UseHypothesisApiService(this IServiceCollection services, string baseUrl)
    {
        services.AddHttpClient<IHypothesisApiService, HypothesisApiService>(client =>
        {
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        return services;
    }
}
