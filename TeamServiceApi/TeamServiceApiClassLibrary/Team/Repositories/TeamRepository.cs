using BranchstormDomain.Infrastructure.Database;
using Dapper;
using TeamApiClassLibrary.Team.Entities.Dtos;
using TeamApiClassLibrary.Team.Entities.Results;

namespace TeamApiClassLibrary.Team.Repositories
{
    internal class TeamRepository(
        IDatabaseConnection connectionDatabase
    ) : ITeamRepository
    {
        private readonly IDatabaseConnection connectionDatabase = connectionDatabase;

        private const string CreateTeamSql = @"
            INSERT INTO Teams (
                Code,
                Name
            )
            VALUES (
                @Code,
                @Name
            );
        ";

        private const string InsertMemberSql = @"
            INSERT INTO TeamMembers (
                TeamCode,
                UserCode,
                Role
            )
            VALUES (
                @TeamCode,
                @UserCode,
                @Role
            );
        ";

        private const string DeleteMembersSql = @"
            DELETE FROM TeamMembers
            WHERE TeamCode = @TeamCode;
        ";

        private const string FindTeamSql = @"
            SELECT
                Code,
                Name
            FROM
                Teams
            WHERE
                Code = @Code;
        ";

        private const string FindMembersSql = @"
            SELECT
                TeamCode,
                UserCode,
                Role
            FROM
                TeamMembers
            WHERE
                TeamCode = @TeamCode;
        ";

        public async Task<TeamResult> CreateAsync(TeamDto teamDto)
        {
            await using var connection = connectionDatabase.CreateConnection();
            await connection.OpenAsync();
            await using var transaction = await connection.BeginTransactionAsync();

            await connection.ExecuteAsync(CreateTeamSql, teamDto, transaction);

            foreach (var member in teamDto.Members ?? [])
            {
                await connection.ExecuteAsync(InsertMemberSql, new
                {
                    TeamCode = teamDto.Code,
                    member.UserCode,
                    Role = member.Role.ToString()
                }, transaction);
            }

            await transaction.CommitAsync();

            return (await FindOneAsync(teamDto.Code))!;
        }

        public async Task<TeamResult?> FindOneAsync(string code)
        {
            await using var connection = connectionDatabase.CreateConnection();
            await connection.OpenAsync();

            var team = await connection.QuerySingleOrDefaultAsync<TeamResult>(
                FindTeamSql,
                new { Code = code }
            );

            if (team is null)
            {
                return null;
            }

            var members = await connection.QueryAsync<TeamMember>(
                FindMembersSql,
                new { TeamCode = code }
            );

            return team with { Members = members };
        }

        public async Task<TeamResult> UpdateMembersAsync(string code, TeamDto teamDto)
        {
            await using var connection = connectionDatabase.CreateConnection();
            await connection.OpenAsync();
            await using var transaction = await connection.BeginTransactionAsync();

            await connection.ExecuteAsync(DeleteMembersSql, new { TeamCode = code }, transaction);

            foreach (var member in teamDto.Members ?? [])
            {
                await connection.ExecuteAsync(InsertMemberSql, new
                {
                    TeamCode = code,
                    member.UserCode,
                    Role = member.Role.ToString()
                }, transaction);
            }

            await transaction.CommitAsync();

            return (await FindOneAsync(code))!;
        }
    }
}
