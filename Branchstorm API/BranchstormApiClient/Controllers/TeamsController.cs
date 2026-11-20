using BranchstormApiClientClassLibrary.Team.Entities.Dtos;
using BranchstormApiClientClassLibrary.Team.Entities.Results;
using BranchstormApiClientClassLibrary.Team.Services;
using Microsoft.AspNetCore.Mvc;

namespace BranchstormApiClient.Controllers;

[ApiController]
[Route("api/team")]
public class TeamController(
    ITeamService teamService
    ) : ControllerBase
{
    private readonly ITeamService teamService = teamService;

    [HttpPost]
    public async Task<ActionResult<TeamResult>> Create([FromBody] TeamDto teamDto)
    {
        TeamResult? team = await teamService.CreateAsync(teamDto);
        return team is null ? NotFound() : Ok(team);
    }

    [HttpGet("{code}")]
    public async Task<ActionResult<TeamResult>> FindOne([FromRoute] string code)
    {
        TeamResult? team = await teamService.FindOneAsync(code);
        return team is null ? NotFound() : Ok(team);
    }

    [HttpPatch("{code}")]
    public async Task<ActionResult<TeamResult>> UpdateMembers([FromRoute] string code, [FromBody] TeamDto teamDto)
    {
        TeamResult? team = await teamService.UpdateMembersAsync(code, teamDto);
        return team is null ? NotFound() : Ok(team);
    }
}
