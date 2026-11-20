using Microsoft.Extensions.DependencyInjection;

namespace BranchstormApiClientClassLibrary.Hypothesis.Services;

public static class HypothesisServiceUtil
{
    public static IServiceCollection UseHypothesisService(this IServiceCollection services)
    {
        services.AddTransient<IHypothesisService, HypothesisService>();
        return services;
    }
}
