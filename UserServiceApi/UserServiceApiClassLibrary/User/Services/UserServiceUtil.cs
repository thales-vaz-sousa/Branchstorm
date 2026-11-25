using Microsoft.Extensions.DependencyInjection;

namespace UserApiClassLibrary.User.Services
{
    public static class UserServiceUtil
    {
        public static IServiceCollection UseUserService(this IServiceCollection services)
        {
            services.AddTransient<IUserService, UserService>();
            return services;
        }
    }
}
