using UserApiClassLibrary.User.Repositories;
using UserApiClassLibrary.User.Services;

namespace UserApi.Containers
{
    public static class DependencyInjectionContainers
    {
        public static IServiceCollection AddDependencyInjectionContainers(this IServiceCollection services)
        {
            services.UseUserService();
            services.UseUserRepository();

            return services;
        }
    }
}

