using Microsoft.Extensions.DependencyInjection;

namespace UserApiClassLibrary.User.Repositories
{
    public static class UserRepositoryUtil
    {
        public static IServiceCollection UseUserRepository(this IServiceCollection services)
        {
            services.AddTransient<IUserRepository, UserRepository>();
            return services;
        }
    }
}
