using Microsoft.Extensions.DependencyInjection;

namespace TeamApiClassLibrary.Team.Services
{
    public static class TeamServiceUtil
    {
        public static IServiceCollection UseTeamService(this IServiceCollection services)
        {
            services.AddTransient<ITeamService, TeamService>();
            return services;
        }
    }
}
