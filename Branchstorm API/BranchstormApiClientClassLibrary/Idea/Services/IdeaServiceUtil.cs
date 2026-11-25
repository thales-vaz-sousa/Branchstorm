using Microsoft.Extensions.DependencyInjection;

namespace BranchstormApiClientClassLibrary.Idea.Services;

public static class IdeaServiceUtil
{
    public static IServiceCollection UseIdeaService(this IServiceCollection services)
    {
        services.AddTransient<IIdeaService, IdeaService>();
        return services;
    }
}
