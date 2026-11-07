using IdeaApiClassLibrary.Link.Entities.Dtos;
using IdeaApiClassLibrary.Link.Entities.Results;
using IdeaApiClassLibrary.Link.Services;
using Microsoft.AspNetCore.Mvc;

namespace IdeaServiceApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IdeaController(
        ILinkService linkService
        ) : ControllerBase
    {
        private readonly ILinkService linkService = linkService;

        [HttpPost]
        public async Task<ActionResult<LinkResult>> CreateLink([FromBody] LinkDto linkDto)
        {
            return Ok(await linkService.CreateAsync(linkDto));
        }

        [HttpGet("{code}")]
        public async Task<ActionResult<LinkResult>> FindLink([FromRoute] string code)
        {
            LinkResult? link = await linkService.FindOneAsync(code);
            return link is null ? NotFound() : Ok(link);
        }

        [HttpGet("tree/{code}")]
        public async Task<ActionResult<IEnumerable<LinkResult>>> FindTree([FromRoute] string code)
        {
            var tree = await linkService.FindTreeAsync(code);
            return tree.Any() ? Ok(tree) : NotFound();
        }

        [HttpPut("select/{code}")]
        public async Task<ActionResult<LinkResult>> SelectLink([FromRoute] string code, [FromBody] LinkValidationDto? body)
        {
            LinkResult? link = await linkService.SelectAsync(code, body?.Reason);
            return link is null ? NotFound() : Ok(link);
        }

        [HttpPut("validation/{code}")]
        public async Task<ActionResult<LinkResult>> UpdateValidation([FromRoute] string code, [FromBody] LinkValidationDto validationDto)
        {
            LinkResult? link = await linkService.UpdateValidationAsync(code, validationDto);
            return link is null ? NotFound() : Ok(link);
        }
    }
}
