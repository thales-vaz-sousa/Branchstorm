using FeedbackApiClassLibrary.Feedback.Entities.Dtos;
using FeedbackApiClassLibrary.Feedback.Entities.Queries;
using FeedbackApiClassLibrary.Feedback.Entities.Results;
using FeedbackApiClassLibrary.Feedback.Services;
using Microsoft.AspNetCore.Mvc;

namespace FeedbackApi.Controllers;

[ApiController]
[Route("api/feedbacks")]
public class FeedbackController(IFeedbackService feedbackService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<FeedbackResult>> CreateFeedback([FromBody] FeedbackDto feedbackDto)
    {
        FeedbackResult feedback = await feedbackService.CreateAsync(feedbackDto);
        return Ok(feedback);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<FeedbackResult>>> Find([FromQuery] FindFeedbacksQuery query)
    {
        IEnumerable<FeedbackResult> feedbacks = await feedbackService.FindAsync(query);
        return Ok(feedbacks);
    }

    [HttpGet("{linkCode}")]
    public async Task<ActionResult<FeedbackResult>> FindOne([FromRoute] string linkCode)
    {
        FindFeedbackQuery query = new ()
        {
            LinkCode = linkCode
        };

        FeedbackResult feedback = await feedbackService.FindOneAsync(query);
        return Ok(feedback);
    }

}

