using BranchstormApiClientClassLibrary.Team.Entities.Dtos;
using BranchstormApiClientClassLibrary.Team.Entities.Results;
using BranchstormApiClientClassLibrary.Team.Services.ApiServices;

namespace BranchstormApiClientClassLibrary.Team.Services;

public class TeamService(ITeamApiService teamApiService) : ITeamService
{
    private readonly ITeamApiService teamApiService = teamApiService;

    public async Task<TeamResult?> CreateAsync(TeamDto teamDto)
    {
        return await teamApiService.CreateAsync(teamDto);
    }

    public async Task<TeamResult?> FindOneAsync(string code)
    {
        return await teamApiService.FindOneAsync(code);
    }

    public async Task<TeamResult?> UpdateMembersAsync(string code, TeamDto teamDto)
    {
        return await teamApiService.UpdateMembersAsync(code, teamDto);
    }
}
