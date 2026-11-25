using BranchstormDomain.Domain;
using BranchstormDomain.Util;
using TeamApiClassLibrary.Team.Entities.Dtos;
using TeamApiClassLibrary.Team.Entities.Results;
using TeamApiClassLibrary.Team.Repositories;

namespace TeamApiClassLibrary.Team.Services
{
    public class TeamService(
        ITeamRepository teamRepository
        ) : ITeamService
    {
        public async Task<TeamResult> CreateAsync(TeamDto teamDto)
        {
            teamDto.ValidateDomain();

            teamDto.Code = DeterministicGuid.Generate(teamDto).ToString();

            return await teamRepository.CreateAsync(teamDto);
        }

        public Task<TeamResult?> FindOneAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentException("Code cannot be null or empty.", nameof(code));
            }

            return teamRepository.FindOneAsync(code);
        }

        public async Task<TeamResult> UpdateMembersAsync(string code, TeamDto teamDto)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentException("Code cannot be null or empty.", nameof(code));
            }

            teamDto.ValidateDomain();

            return await teamRepository.UpdateMembersAsync(code, teamDto);
        }
    }
}
