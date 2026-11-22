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

    [HttpGet("{linkCode}")]
    public async Task<ActionResult<FeedbackResult>> FindOne([FromRoute] string linkCode)
    {
        IEnumerable<FeedbackResult> feedbacks = await feedbackService.FindByLinkAsync(linkCode);
        FeedbackResult? feedback = feedbacks.FirstOrDefault();
        return feedback is null ? NotFound() : Ok(feedback);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<FeedbackResult>>> Find([FromQuery] string[]? linkCodes)
    {
        ActionResult<IEnumerable<FeedbackResult>> result;

        if (linkCodes is not null && linkCodes.Length > 0)
        {
            IEnumerable<FeedbackResult> feedback = await feedbackService.FindByLinksAsync(linkCodes);
            result = Ok(feedback);
        }
        else
        {
            result = BadRequest("LinkCodes query parameter is required");
        }

        return result;
    }
}
