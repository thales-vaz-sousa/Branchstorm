using TeamApiClassLibrary.Team.Entities.Dtos;
using TeamApiClassLibrary.Team.Entities.Results;

namespace TeamApiClassLibrary.Team.Repositories
{
    public interface ITeamRepository
    {
        Task<TeamResult> CreateAsync(TeamDto teamDto);
        Task<TeamResult?> FindOneAsync(string code);
        Task<TeamResult> UpdateMembersAsync(string code, TeamDto teamDto);
    }
}
