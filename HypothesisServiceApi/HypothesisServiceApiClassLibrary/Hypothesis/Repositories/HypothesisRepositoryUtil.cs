using Microsoft.Extensions.DependencyInjection;

namespace HypothesisServiceApi.Hypothesis.Repositories
{
    public static class HypothesisRepositoryUtil
    {
        public static IServiceCollection UseHypothesisRepository(this IServiceCollection services)
        {
            services.AddScoped<IHypothesisRepository, HypothesisRepository>();
            return services;
        }
    }
}
