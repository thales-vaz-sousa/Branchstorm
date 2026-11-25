using FeedbackApiClassLibrary.Feedback.Entities.Dtos;
using FeedbackApiClassLibrary.Feedback.Entities.Queries;
using FeedbackApiClassLibrary.Feedback.Entities.Results;

namespace FeedbackApiClassLibrary.Feedback.Repositories;

public interface IFeedbackRepository
{
    Task<FeedbackResult> CreateAsync(FeedbackDto feedbackDto);
    Task<IEnumerable<FeedbackResult>> FindAsync(FindFeedbacksQuery query);
    Task<FeedbackResult> FindOneAsync(FindFeedbackQuery query);
}
