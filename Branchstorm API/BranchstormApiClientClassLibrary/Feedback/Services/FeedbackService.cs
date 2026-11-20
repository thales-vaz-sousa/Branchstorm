using BranchstormApiClientClassLibrary.Feedback.Entities.Dtos;
using BranchstormApiClientClassLibrary.Feedback.Entities.Results;
using BranchstormApiClientClassLibrary.Feedback.Services.ApiServices;

namespace BranchstormApiClientClassLibrary.Feedback.Services;

public class FeedbackService(IFeedbackApiService feedbackApiService) : IFeedbackService
{
    private readonly IFeedbackApiService feedbackApiService = feedbackApiService;

    public async Task<FeedbackResult?> CreateAsync(FeedbackDto feedbackDto)
    {
        return await feedbackApiService.CreateAsync(feedbackDto);
    }

    public async Task<IEnumerable<FeedbackResult>> FindByLinkAsync(string linkCode)
    {
        return await feedbackApiService.FindByLinkAsync(linkCode);
    }

    public async Task<IEnumerable<FeedbackResult>> FindByLinksAsync(IEnumerable<string> linkCodes)
    {
        return await feedbackApiService.FindByLinksAsync(linkCodes);
    }
}
