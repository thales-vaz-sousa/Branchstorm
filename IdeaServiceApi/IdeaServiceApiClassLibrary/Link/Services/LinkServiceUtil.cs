using Microsoft.Extensions.DependencyInjection;

namespace IdeaApiClassLibrary.Link.Services
{
    public static class LinkServiceUtil
    {
        public static IServiceCollection UseLinkService(this IServiceCollection services)
        {
            services.AddTransient<ILinkService, LinkService>();
            return services;
        }
    }
}
