using BranchstormApiClientClassLibrary.Feedback.Services;
using BranchstormApiClientClassLibrary.Feedback.Services.ApiServices;
using BranchstormApiClientClassLibrary.Hypothesis.Services;
using BranchstormApiClientClassLibrary.Hypothesis.Services.ApiServices;
using BranchstormApiClientClassLibrary.Idea.Services;
using BranchstormApiClientClassLibrary.Idea.Services.ApiServices;
using BranchstormApiClientClassLibrary.Team.Services;
using BranchstormApiClientClassLibrary.Team.Services.ApiServices;
using BranchstormApiClientClassLibrary.User.Services;
using BranchstormApiClientClassLibrary.User.Services.ApiServices;

namespace BranchstormApi.Containers;

public static class DependencyInjectionContainers
{
    public static IServiceCollection AddDependencyInjectionContainers(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.UseUserApiService(configuration["UserApi:BaseUrl"]!);
        services.UseUserService();

        services.UseTeamApiService(configuration["TeamApi:BaseUrl"]!);
        services.UseTeamService();

        services.UseIdeaApiService(configuration["IdeaApi:BaseUrl"]!);
        services.UseIdeaService();

        services.UseHypothesisApiService(configuration["HypothesisApi:BaseUrl"]!);
        services.UseHypothesisService();

        services.UseFeedbackApiService(configuration["FeedbackApi:BaseUrl"]!);
        services.UseFeedbackService();

        return services;
    }
}
