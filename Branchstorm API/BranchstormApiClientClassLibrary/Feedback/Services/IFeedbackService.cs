using BranchstormApiClientClassLibrary.Feedback.Entities.Dtos;
using BranchstormApiClientClassLibrary.Feedback.Entities.Results;

namespace BranchstormApiClientClassLibrary.Feedback.Services;

public interface IFeedbackService
{
    Task<FeedbackResult?> CreateAsync(FeedbackDto feedbackDto);
    Task<IEnumerable<FeedbackResult>> FindByLinkAsync(string linkCode);
    Task<IEnumerable<FeedbackResult>> FindByLinksAsync(IEnumerable<string> linkCodes);
}
