using Microsoft.Extensions.DependencyInjection;

namespace BranchstormApiClientClassLibrary.Team.Services.ApiServices;

public static class TeamApiServiceUtil
{
    public static IServiceCollection UseTeamApiService(this IServiceCollection services, string baseUrl)
    {
        services.AddHttpClient<ITeamApiService, TeamApiService>(client =>
        {
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        return services;
    }
}
