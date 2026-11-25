using BranchstormDomain.Domain;
using BranchstormDomain.Util;
using FeedbackApiClassLibrary.Feedback.Entities.Dtos;
using FeedbackApiClassLibrary.Feedback.Entities.Queries;
using FeedbackApiClassLibrary.Feedback.Entities.Results;
using FeedbackApiClassLibrary.Feedback.Repositories;

namespace FeedbackApiClassLibrary.Feedback.Services;

public class FeedbackService(
    IFeedbackRepository feedbackRepository
) : IFeedbackService
{
    public async Task<FeedbackResult> CreateAsync(FeedbackDto feedbackDto)
    {
        feedbackDto.ValidateDomain();

        feedbackDto.Code = DeterministicGuid.Generate(feedbackDto).ToString();

        return await feedbackRepository.CreateAsync(feedbackDto);
    }

    public Task<IEnumerable<FeedbackResult>> FindAsync(FindFeedbacksQuery query)
    {
        query.ValidateDomain();

        return feedbackRepository.FindAsync(query);
    }

    public Task<FeedbackResult> FindOneAsync(FindFeedbackQuery query)
    {
        query.ValidateDomain(); 

        return feedbackRepository.FindOneAsync(query);
    }
}
