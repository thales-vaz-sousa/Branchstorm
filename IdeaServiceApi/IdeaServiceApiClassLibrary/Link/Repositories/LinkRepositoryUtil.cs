using Microsoft.Extensions.DependencyInjection;

namespace IdeaApiClassLibrary.Link.Repositories
{
    public static class LinkRepositoryUtil
    {
        public static IServiceCollection UseLinkRepository(this IServiceCollection services)
        {
            services.AddScoped<ILinkRepository, LinkRepository>();
            return services;
        }
    }
}
