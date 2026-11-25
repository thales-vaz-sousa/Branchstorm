using TeamApiClassLibrary.Team.Entities.Dtos;
using TeamApiClassLibrary.Team.Entities.Results;

namespace TeamApiClassLibrary.Team.Services
{
    public interface ITeamService
    {
        Task<TeamResult> CreateAsync(TeamDto teamDto);
        Task<TeamResult?> FindOneAsync(string code);
        Task<TeamResult> UpdateMembersAsync(string code, TeamDto teamDto);
    }
}
