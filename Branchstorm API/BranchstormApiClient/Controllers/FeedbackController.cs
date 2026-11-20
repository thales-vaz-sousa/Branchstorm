using BranchstormApiClientClassLibrary.Feedback.Entities.Dtos;
using BranchstormApiClientClassLibrary.Feedback.Entities.Results;
using BranchstormApiClientClassLibrary.Feedback.Services;
using Microsoft.AspNetCore.Mvc;

namespace BranchstormApiClient.Controllers;

[ApiController]
[Route("api/feedbacks")]
public class FeedbackController(IFeedbackService feedbackService) : ControllerBase
{
    private readonly IFeedbackService feedbackService = feedbackService;

    [HttpPost]
    public async Task<ActionResult<FeedbackResult>> Create([FromBody] FeedbackDto feedbackDto)
    {
        FeedbackResult? feedback = await feedbackService.CreateAsync(feedbackDto);
        return feedback is null ? NotFound() : Ok(feedback);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<FeedbackResult>>> Find([FromQuery] string? linkCode,[FromQuery] string[]? linkCodes)
    {
        ActionResult<IEnumerable<FeedbackResult>> result;

        if (!string.IsNullOrWhiteSpace(linkCode))
        {
            IEnumerable<FeedbackResult> feedback = await feedbackService.FindByLinkAsync(linkCode);
            result = Ok(feedback);
        }
        else if (linkCodes is not null && linkCodes.Length > 0)
        {
            IEnumerable<FeedbackResult> feedback = await feedbackService.FindByLinksAsync(linkCodes);
            result = Ok(feedback);
        }
        else
        {
            result = BadRequest();
        }

        return result;
    }
}
