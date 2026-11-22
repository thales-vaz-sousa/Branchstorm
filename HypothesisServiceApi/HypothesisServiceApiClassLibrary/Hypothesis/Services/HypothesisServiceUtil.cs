using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HypothesisServiceApi.Hypothesis.Services
{
    public static class HypothesisServiceUtil
    {
        public static IServiceCollection UseHypothesisService(this IServiceCollection services)
        {
            services.AddHttpClient<IHypothesisService, HypothesisService>((provider, client) =>
            {
                IConfiguration configuration = provider.GetRequiredService<IConfiguration>();
                client.BaseAddress = new Uri(configuration["Services:IdeaApiUrl"]!);
            });

            return services;
        }
    }
}
