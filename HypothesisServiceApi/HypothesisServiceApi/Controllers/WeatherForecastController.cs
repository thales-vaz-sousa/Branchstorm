using HypothesisServiceApi.Hypothesis;
using Microsoft.AspNetCore.Mvc;

namespace HypothesisServiceApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class HypothesisController(
    IHypothesisService hypothesisService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateHypothesisRequest request)
    {
        var hypothesis = await hypothesisService.CreateAsync(request);

        return CreatedAtAction(
            nameof(FindByLink),
            new { linkCode = hypothesis.LinkCode },
            hypothesis);
    }

    [HttpGet("link/{linkCode:guid}")]
    public async Task<IActionResult> FindByLink(Guid linkCode)
    {
        var hypotheses =
            await hypothesisService.FindByLinkCodeAsync(linkCode);

        return Ok(hypotheses);
    }

    [HttpPut("{code:guid}/result")]
    public async Task<IActionResult> RegisterResult(
        Guid code,
        [FromBody] RegisterHypothesisResultRequest request)
    {
        var hypothesis =
            await hypothesisService.RegisterResultAsync(code, request);

        return hypothesis is null ? NotFound() : Ok(hypothesis);
    }
}