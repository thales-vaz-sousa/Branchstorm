using Microsoft.AspNetCore.Mvc;
using TeamApiClassLibrary.Team.Entities.Dtos;
using TeamApiClassLibrary.Team.Entities.Results;
using TeamApiClassLibrary.Team.Services;

namespace TeamServiceApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TeamController(
        ITeamService teamService
        ) : ControllerBase
    {
        private readonly ITeamService teamService = teamService;

        [HttpPost]
        public async Task<ActionResult<TeamResult>> CreateTeam([FromBody] TeamDto teamDto)
        {
            return Ok(await teamService.CreateAsync(teamDto));
        }

        [HttpGet("{code}")]
        public async Task<ActionResult<TeamResult>> FindTeam([FromRoute] string code)
        {
            TeamResult? team = await teamService.FindOneAsync(code);
            return team is null ? NotFound() : Ok(team);
        }

        [HttpPatch("{code}")]
        public async Task<ActionResult<TeamResult>> UpdateTeamMembers([FromRoute] string code, [FromBody] TeamDto teamDto)
        {
            return Ok(await teamService.UpdateMembersAsync(code, teamDto));
        }
    }
}
