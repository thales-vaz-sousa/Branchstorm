using Microsoft.Extensions.DependencyInjection;

namespace TeamApiClassLibrary.Team.Repositories
{
    public static class TeamRepositoryUtil
    {
        public static IServiceCollection UseTeamRepository(this IServiceCollection services)
        {
            services.AddScoped<ITeamRepository, TeamRepository>();
            return services;
        }
    }
}
