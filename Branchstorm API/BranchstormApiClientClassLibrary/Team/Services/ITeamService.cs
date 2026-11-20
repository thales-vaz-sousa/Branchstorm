using BranchstormApiClientClassLibrary.Team.Entities.Dtos;
using BranchstormApiClientClassLibrary.Team.Entities.Results;

namespace BranchstormApiClientClassLibrary.Team.Services;

public interface ITeamService
{
    Task<TeamResult?> CreateAsync(TeamDto teamDto);
    Task<TeamResult?> FindOneAsync(string code);
    Task<TeamResult?> UpdateMembersAsync(string code, TeamDto teamDto);
}
