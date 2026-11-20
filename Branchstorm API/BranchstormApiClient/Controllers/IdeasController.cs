using BranchstormApiClientClassLibrary.Idea.Entities.Dtos;
using BranchstormApiClientClassLibrary.Idea.Entities.Results;
using BranchstormApiClientClassLibrary.Idea.Services;
using Microsoft.AspNetCore.Mvc;

namespace BranchstormApiClient.Controllers;

[ApiController]
[Route("api/idea")]
public class IdeaController(
    IIdeaService ideaService
    ) : ControllerBase
{
    private readonly IIdeaService ideaService = ideaService;

    [HttpPost]
    public async Task<ActionResult<LinkResult>> Create([FromBody] LinkDto linkDto)
    {
        LinkResult? link = await ideaService.CreateAsync(linkDto);
        return link is null ? NotFound() : Ok(link);
    }

    [HttpGet("{code}")]
    public async Task<ActionResult<LinkResult>> FindOne([FromRoute] string code)
    {
        LinkResult? link = await ideaService.FindOneAsync(code);
        return link is null ? NotFound() : Ok(link);
    }

    [HttpGet("tree/{code}")]
    public async Task<ActionResult<IEnumerable<LinkResult>>> FindTree([FromRoute] string code)
    {
        IEnumerable<LinkResult> tree = await ideaService.FindTreeAsync(code);
        return tree.Any() ? Ok(tree) : NotFound();
    }

    [HttpPut("select/{code}")]
    public async Task<ActionResult<LinkResult>> Select([FromRoute] string code, [FromBody] LinkValidationDto? body)
    {
        LinkResult? link = await ideaService.SelectAsync(code, body);
        return link is null ? NotFound() : Ok(link);
    }

    [HttpPut("validation/{code}")]
    public async Task<ActionResult<LinkResult>> UpdateValidation([FromRoute] string code, [FromBody] LinkValidationDto validationDto)
    {
        LinkResult? link = await ideaService.UpdateValidationAsync(code, validationDto);
        return link is null ? NotFound() : Ok(link);
    }
}
