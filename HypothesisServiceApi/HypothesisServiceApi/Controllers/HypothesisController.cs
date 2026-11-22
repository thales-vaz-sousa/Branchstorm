using Microsoft.AspNetCore.Mvc;
using HypothesisServiceApi.Hypothesis.Entities.Dtos;
using HypothesisServiceApi.Hypothesis.Entities.Results;
using HypothesisServiceApi.Hypothesis.Services;

namespace HypothesisServiceApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HypothesisController(
        IHypothesisService hypothesisService
    ) : ControllerBase
    {
        private readonly IHypothesisService hypothesisService = hypothesisService;

        [HttpPost]
        public async Task<ActionResult<HypothesisResult>> CreateHypothesis([FromBody] HypothesisDto hypothesisDto)
        {
            return Ok(await hypothesisService.CreateAsync(hypothesisDto));
        }

        [HttpGet("{code}")]
        public async Task<ActionResult<HypothesisResult>> FindHypothesis([FromRoute] string code)
        {
            HypothesisResult? hypothesis = await hypothesisService.FindOneAsync(code);
            return hypothesis is null ? NotFound() : Ok(hypothesis);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<HypothesisResult>>> Find(
            [FromQuery] string? linkCode,
            [FromQuery] string[]? linkCodes)
        {
            if (!string.IsNullOrWhiteSpace(linkCode))
            {
                return Ok(await hypothesisService.FindByLinkCodeAsync(linkCode));
            }

            if (linkCodes is not null && linkCodes.Length > 0)
            {
                return Ok(await hypothesisService.FindByLinkCodesAsync(linkCodes));
            }

            return BadRequest("linkCode or linkCodes query parameter is required");
        }

        [HttpPut("result/{code}")]
        public async Task<ActionResult<HypothesisResult>> RegisterResult(
            [FromRoute] string code,
            [FromBody] HypothesisResultDto resultDto)
        {
            return Ok(await hypothesisService.RegisterResultAsync(code, resultDto.Result));
        }
    }
}
