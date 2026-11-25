using BffApiClassLibrary.Services.Teams;
using BffApiClassLibrary.Views;
using Microsoft.AspNetCore.Mvc;

namespace BffServiceApi.Controllers;

/// <summary>Tela de equipe (RF02): membros ja com nome/e-mail e operações de um membro so.</summary>
[ApiController]
[Route("bff/teams")]
public class TeamsController(ITeamViewService teamService) : ControllerBase
{
    private readonly ITeamViewService teamService = teamService;

    [HttpPost]
    public async Task<ActionResult<TeamView>> Create([FromBody] CreateTeamRequest request, CancellationToken ct)
    {
        TeamView team = await teamService.CreateAsync(request, ct);
        return Created($"/bff/teams/{team.Code}", team);
    }

    [HttpGet("{code}")]
    public async Task<ActionResult<TeamView>> FindOne([FromRoute] string code, CancellationToken ct)
    {
        return Ok(await teamService.GetAsync(code, ct));
    }

    /// <summary>Substitui a lista inteira de membros (nome/descrição da equipe sao mantidos).</summary>
    [HttpPut("{code}/members")]
    public async Task<ActionResult<TeamView>> ReplaceMembers(
        [FromRoute] string code, [FromBody] ReplaceMembersRequest request, CancellationToken ct)
    {
        return Ok(await teamService.ReplaceMembersAsync(code, request, ct));
    }

    /// <summary>Adiciona um membro, ou troca o papel se ele ja estiver na equipe.</summary>
    [HttpPost("{code}/members")]
    public async Task<ActionResult<TeamView>> UpsertMember(
        [FromRoute] string code, [FromBody] TeamMemberInput member, CancellationToken ct)
    {
        return Ok(await teamService.UpsertMemberAsync(code, member, ct));
    }

    [HttpDelete("{code}/members/{userCode}")]
    public async Task<ActionResult<TeamView>> RemoveMember(
        [FromRoute] string code, [FromRoute] string userCode, CancellationToken ct)
    {
        return Ok(await teamService.RemoveMemberAsync(code, userCode, ct));
    }
}
