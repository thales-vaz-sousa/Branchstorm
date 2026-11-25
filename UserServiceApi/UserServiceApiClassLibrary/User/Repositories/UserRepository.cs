using BranchstormDomain.Infrastructure.Database;
using Dapper;
using UserApiClassLibrary.User.Entities.Dtos;
using UserApiClassLibrary.User.Entities.Queries;
using UserApiClassLibrary.User.Entities.Results;

namespace UserApiClassLibrary.User.Repositories
{
    internal class UserRepository (
    IDatabaseConnection connectionDatabase
) : IUserRepository
    {
        private readonly IDatabaseConnection connectionDatabase = connectionDatabase;

        private const string CreateUserSql = @"
            INSERT INTO Users (
                Code,
                Name,
                Email
            )
            VALUES (
                @Code,
                @Name,
                @Email
            )
            RETURNING Code, Name, Email;
        ";

        private const string FindFilteredSql = @"
            SELECT
                Code,
                Name,
                Email
            FROM 
                Users
            WHERE
                (@Code IS NULL OR Code = @Code)
                AND (@Name IS NULL OR Name LIKE '%' || @Name || '%')
                AND (@Email IS NULL OR Email = @Email);
        ";

        private const string FindOneSql = @"
            SELECT 
                Code, 
                Name, 
                Email
            FROM 
                Users
            WHERE 
                Code = @Code;
        ";

        const string FindByCodesSql = @"
            SELECT 
                Code, 
                Name, 
                Email 
            FROM 
                Users 
            WHERE 
                Code IN @Codes
        ";

        public async Task<UserResult> CreateAsync(UserDto userDto)
        {
            await using var connection = connectionDatabase.CreateConnection();
            await connection.OpenAsync();

            return await connection.QuerySingleAsync<UserResult>(CreateUserSql, userDto);
        }


        public async Task<IEnumerable<UserResult>> FindAsync(FindUsersQuery query)
        {
            await using var connection = connectionDatabase.CreateConnection();
            await connection.OpenAsync();

            bool byCodes = query.Codes is { Length: > 0 };

            string sql = byCodes ? FindByCodesSql : FindFilteredSql;

            object parameters = byCodes
                ? new { query.Codes }
                : new { query.Code, query.Name, query.Email };

            return await connection.QueryAsync<UserResult>(sql, parameters);
        }

        public async Task<UserResult?> FindOneAsync(string code)
        {
            await using var connection = connectionDatabase.CreateConnection();

            await connection.OpenAsync();

            return await connection.QuerySingleOrDefaultAsync<UserResult>(
                FindOneSql,
                new { Code = code }
            );
        }
    }
}
