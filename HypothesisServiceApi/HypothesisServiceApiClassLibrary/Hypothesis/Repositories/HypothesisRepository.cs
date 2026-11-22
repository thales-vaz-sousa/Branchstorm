using BranchstormDomain.Infrastructure.Database;
using Dapper;
using HypothesisServiceApi.Hypothesis.Entities.Dtos;
using HypothesisServiceApi.Hypothesis.Entities.Results;

namespace HypothesisServiceApi.Hypothesis.Repositories
{
    internal class HypothesisRepository(
        IDatabaseConnection connectionDatabase
    ) : IHypothesisRepository
    {
        private readonly IDatabaseConnection connectionDatabase = connectionDatabase;

        private const string CreateHypothesisSql = @"
            INSERT INTO Hypotheses (
                Code,
                LinkCode,
                Description,
                SuccessCriteria,
                CreationDate
            )
            VALUES (
                @Code,
                @LinkCode,
                @Description,
                @SuccessCriteria,
                @CreationDate
            );
        ";

        private const string FindHypothesisSql = @"
            SELECT
                Code,
                LinkCode,
                Description,
                SuccessCriteria,
                Result,
                CreationDate
            FROM
                Hypotheses
            WHERE
                Code = @Code;
        ";

        private const string FindByLinkCodeSql = @"
            SELECT
                Code,
                LinkCode,
                Description,
                SuccessCriteria,
                Result,
                CreationDate
            FROM
                Hypotheses
            WHERE
                LinkCode = @LinkCode;
        ";

        private const string FindByLinkCodesSql = @"
            SELECT
                Code,
                LinkCode,
                Description,
                SuccessCriteria,
                Result,
                CreationDate
            FROM
                Hypotheses
            WHERE
                LinkCode IN @LinkCodes;
        ";

        private const string RegisterResultSql = @"
            UPDATE Hypotheses
            SET Result = @Result
            WHERE Code = @Code;
        ";

        public async Task<HypothesisResult> CreateAsync(HypothesisDto hypothesisDto)
        {
            if (string.IsNullOrWhiteSpace(hypothesisDto.Code))
            {
                throw new ArgumentException("Code cannot be null or empty.", nameof(hypothesisDto.Code));
            }

            await using var connection = connectionDatabase.CreateConnection();
            await connection.OpenAsync();

            await connection.ExecuteAsync(CreateHypothesisSql, new
            {
                hypothesisDto.Code,
                hypothesisDto.LinkCode,
                hypothesisDto.Description,
                hypothesisDto.SuccessCriteria,
                CreationDate = DateTime.UtcNow
            });

            return (await FindOneAsync(hypothesisDto.Code))!;
        }

        public async Task<HypothesisResult?> FindOneAsync(string code)
        {
            await using var connection = connectionDatabase.CreateConnection();
            await connection.OpenAsync();

            return await connection.QuerySingleOrDefaultAsync<HypothesisResult>(
                FindHypothesisSql,
                new { Code = code }
            );
        }

        public async Task<IEnumerable<HypothesisResult>> FindByLinkCodeAsync(string linkCode)
        {
            await using var connection = connectionDatabase.CreateConnection();
            await connection.OpenAsync();

            return await connection.QueryAsync<HypothesisResult>(
                FindByLinkCodeSql,
                new { LinkCode = linkCode }
            );
        }

        public async Task<IEnumerable<HypothesisResult>> FindByLinkCodesAsync(IEnumerable<string> linkCodes)
        {
            await using var connection = connectionDatabase.CreateConnection();
            await connection.OpenAsync();

            return await connection.QueryAsync<HypothesisResult>(
                FindByLinkCodesSql,
                new { LinkCodes = linkCodes }
            );
        }

        public async Task<HypothesisResult> RegisterResultAsync(string code, string result)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentException("Code cannot be null or empty.", nameof(code));
            }

            await using var connection = connectionDatabase.CreateConnection();
            await connection.OpenAsync();

            await connection.ExecuteAsync(RegisterResultSql, new { Code = code, Result = result });

            return (await FindOneAsync(code))!;
        }
    }
}
