using Microsoft.Extensions.DependencyInjection;

namespace BranchstormApiClientClassLibrary.User.Services.ApiServices;

public static class UserApiServiceUtil
{
    public static IServiceCollection UseUserApiService(this IServiceCollection services, string baseUrl)
    {
        services.AddHttpClient<IUserApiService, UserApiService>(client =>
        {
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        return services;
    }
}
