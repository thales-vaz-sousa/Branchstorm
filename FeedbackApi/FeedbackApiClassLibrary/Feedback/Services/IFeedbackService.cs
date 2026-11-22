using FeedbackApiClassLibrary.Feedback.Entities.Dtos;
using FeedbackApiClassLibrary.Feedback.Entities.Queries;
using FeedbackApiClassLibrary.Feedback.Entities.Results;

namespace FeedbackApiClassLibrary.Feedback.Services;

public interface IFeedbackService
{
    Task<FeedbackResult> CreateAsync(FeedbackDto feedbackDto);
    Task<IEnumerable<FeedbackResult>> FindAsync(FindFeedbacksQuery query);
    Task<FeedbackResult> FindOneAsync(FindFeedbackQuery query);
}
