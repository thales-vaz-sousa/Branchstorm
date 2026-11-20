using BranchstormApiClientClassLibrary.Hypothesis.Entities.Dtos;
using BranchstormApiClientClassLibrary.Hypothesis.Entities.Results;
using BranchstormApiClientClassLibrary.Hypothesis.Services;
using Microsoft.AspNetCore.Mvc;

namespace BranchstormApiClient.Controllers;

[ApiController]
[Route("api/hypothesis")]
public class HypothesisController(
    IHypothesisService hypothesisService
) : ControllerBase
{
    private readonly IHypothesisService hypothesisService = hypothesisService;

    [HttpPost]
    public async Task<ActionResult<HypothesisResult>> Create([FromBody] HypothesisDto hypothesisDto)
    {
        HypothesisResult? hypothesis = await hypothesisService.CreateAsync(hypothesisDto);
        return hypothesis is null ? NotFound() : Ok(hypothesis);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<HypothesisResult>>> Find([FromQuery] string? linkCode, [FromQuery] string[]? linkCodes)
    {
        ActionResult<IEnumerable<HypothesisResult>> result;

        if (!string.IsNullOrWhiteSpace(linkCode))
        {
            IEnumerable<HypothesisResult> hypotheses = await hypothesisService.FindByLinkAsync(linkCode);
            result = Ok(hypotheses);
        }
        else if (linkCodes is not null && linkCodes.Length > 0)
        {
            IEnumerable<HypothesisResult> hypotheses = await hypothesisService.FindByLinksAsync(linkCodes);
            result = Ok(hypotheses);
        }
        else
        {
            result = BadRequest();
        }

        return result;
    }

    [HttpPut("result/{code}")]
    public async Task<ActionResult<HypothesisResult>> UpdateResult([FromRoute] string code,[FromBody] HypothesisResultDto resultDto)
    {
        HypothesisResult? hypothesis = await hypothesisService.UpdateResultAsync(code, resultDto);
        return hypothesis is null ? NotFound() : Ok(hypothesis);
    }
}
