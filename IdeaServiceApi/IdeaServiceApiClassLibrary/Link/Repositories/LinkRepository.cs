using BranchstormDomain.Infrastructure.Database;
using Dapper;
using IdeaApiClassLibrary.Link.Entities;
using IdeaApiClassLibrary.Link.Entities.Dtos;
using IdeaApiClassLibrary.Link.Entities.Results;

namespace IdeaApiClassLibrary.Link.Repositories
{
    internal class LinkRepository(
        IDatabaseConnection connectionDatabase
    ) : ILinkRepository
    {
        private readonly IDatabaseConnection connectionDatabase = connectionDatabase;

        private const string CreateLinkSql = @"
            INSERT INTO Links (
                Code,
                FatherCode,
                Type,
                Title,
                Description,
                Status,
                Validation,
                AutorCode,
                TeamCode,
                Reason,
                CreationDate
            )
            VALUES (
                @Code,
                @FatherCode,
                @Type,
                @Title,
                @Description,
                @Status,
                @Validation,
                @AutorCode,
                @TeamCode,
                @Reason,
                @CreationDate
            );
        ";
        private const string InsertHistorySql = @"
            INSERT INTO LinkHistory (
                Code,
                LinkCode,
                Event,
                CreationDate
            )
            VALUES (
                @Code,
                @LinkCode,
                @Event,
                @CreationDate
            );
        ";
        private const string FindOneSql = @"
            SELECT
                Code,
                FatherCode,
                Type,
                Title,
                Description,
                Status,
                Validation,
                AutorCode,
                TeamCode,
                Reason,
                CreationDate
            FROM
                Links
            WHERE
                Code = @Code;
        ";
        private const string FindHistorySql = @"
            SELECT
                Code,
                LinkCode,
                Event,
                CreationDate
            FROM
                LinkHistory
            WHERE
                LinkCode IN @Codes
            ORDER BY
                CreationDate;
        ";
        private const string FindTreeSql = @"
            WITH RECURSIVE Tree AS (
                SELECT
                    Code,
                    FatherCode,
                    Type,
                    Title,
                    Description,
                    Status,
                    Validation,
                    AutorCode,
                    TeamCode,
                    Reason,
                    CreationDate
                FROM
                    Links
                WHERE
                    Code = @Code

                UNION ALL

                SELECT
                    Links.Code,
                    Links.FatherCode,
                    Links.Type,
                    Links.Title,
                    Links.Description,
                    Links.Status,
                    Links.Validation,
                    Links.AutorCode,
                    Links.TeamCode,
                    Links.Reason,
                    Links.CreationDate
                FROM
                    Links
                INNER JOIN
                    Tree ON Links.FatherCode = Tree.Code
            )
            SELECT * FROM Tree;
        ";
        private const string UpdateStatusSql = @"
            UPDATE
                Links
            SET
                Status = @Status
            WHERE
                Code = @Code;
        ";
        private const string DiscardSiblingsSql = @"
            UPDATE
                Links
            SET
                Status = @DiscardedStatus
            WHERE
                FatherCode = @FatherCode
                AND Code <> @Code
                AND Status <> @FinalStatus;
        ";
        private const string FindSiblingCodesSql = @"
            SELECT
                Code
            FROM
                Links
            WHERE
                FatherCode = @FatherCode
                AND Code <> @Code
                AND Status <> @FinalStatus;
        ";
        private const string UpdateValidationSql = @"
            UPDATE
                Links
            SET
                Validation = @Validation
            WHERE
                Code = @Code;
        ";

        public async Task<LinkResult> CreateAsync(LinkDto linkDto)
        {
            await using var connection = connectionDatabase.CreateConnection();
            await connection.OpenAsync();
            await using var transaction = await connection.BeginTransactionAsync();

            await connection.ExecuteAsync(CreateLinkSql, new
            {
                linkDto.Code,
                linkDto.FatherCode,
                Type = linkDto.Type.ToString(),
                linkDto.Title,
                linkDto.Description,
                Status = LinkStatus.Open.ToString(),
                Validation = LinkValidation.NotTested.ToString(),
                linkDto.AutorCode,
                linkDto.TeamCode,
                linkDto.Reason,
                CreationDate = DateTime.UtcNow
            }, transaction);

            await InsertHistoryAsync(connection, transaction, linkDto.Code!, $"Created as {linkDto.Type}");

            await transaction.CommitAsync();

            return (await FindOneAsync(linkDto.Code!))!;
        }

        public async Task<LinkResult?> FindOneAsync(string code)
        {
            await using var connection = connectionDatabase.CreateConnection();
            await connection.OpenAsync();

            var link = await connection.QuerySingleOrDefaultAsync<LinkResult>(
                FindOneSql,
                new { Code = code }
            );

            if (link is null)
            {
                return null;
            }

            var history = await connection.QueryAsync<LinkHistoryResult>(
                FindHistorySql,
                new { Codes = new[] { code } }
            );

            return link with { History = history };
        }

        public async Task<IEnumerable<LinkResult>> FindTreeAsync(string code)
        {
            await using var connection = connectionDatabase.CreateConnection();
            await connection.OpenAsync();

            var links = (await connection.QueryAsync<LinkResult>(
                FindTreeSql,
                new { Code = code }
            )).ToList();

            if (links.Count == 0)
            {
                return links;
            }

            var history = await connection.QueryAsync<LinkHistoryResult>(
                FindHistorySql,
                new { Codes = links.Select(l => l.Code).ToArray() }
            );

            var historyByLink = history.GroupBy(h => h.LinkCode)
                .ToDictionary(g => g.Key, g => g.AsEnumerable());

            return links.Select(l => l with
            {
                History = historyByLink.TryGetValue(l.Code, out var h) ? h : []
            });
        }

        public async Task<LinkResult?> SelectAsync(string code, string? reason)
        {
            await using var connection = connectionDatabase.CreateConnection();
            await connection.OpenAsync();

            var link = await connection.QuerySingleOrDefaultAsync<LinkResult>(
                FindOneSql,
                new { Code = code }
            );

            if (link is null)
            {
                return null;
            }

            await using var transaction = await connection.BeginTransactionAsync();

            await connection.ExecuteAsync(UpdateStatusSql, new
            {
                Status = LinkStatus.Selected.ToString(),
                Code = code
            }, transaction);

            await InsertHistoryAsync(connection, transaction, code,
                string.IsNullOrWhiteSpace(reason) ? "Selected" : $"Selected: {reason}");

            if (!string.IsNullOrWhiteSpace(link.FatherCode))
            {
                var siblingCodes = (await connection.QueryAsync<string>(
                    FindSiblingCodesSql,
                    new
                    {
                        FatherCode = link.FatherCode,
                        Code = code,
                        FinalStatus = LinkStatus.Final.ToString()
                    }, transaction)).ToList();

                await connection.ExecuteAsync(DiscardSiblingsSql, new
                {
                    DiscardedStatus = LinkStatus.Discarded.ToString(),
                    FatherCode = link.FatherCode,
                    Code = code,
                    FinalStatus = LinkStatus.Final.ToString()
                }, transaction);

                foreach (var siblingCode in siblingCodes)
                {
                    await InsertHistoryAsync(connection, transaction, siblingCode,
                        $"Discarded: sibling {code} was selected");
                }
            }

            await transaction.CommitAsync();

            return await FindOneAsync(code);
        }

        public async Task<LinkResult?> UpdateValidationAsync(string code, LinkValidation validation, string? reason)
        {
            await using var connection = connectionDatabase.CreateConnection();
            await connection.OpenAsync();

            var link = await connection.QuerySingleOrDefaultAsync<LinkResult>(
                FindOneSql,
                new { Code = code }
            );

            if (link is null)
            {
                return null;
            }

            await using var transaction = await connection.BeginTransactionAsync();

            await connection.ExecuteAsync(UpdateValidationSql, new
            {
                Validation = validation.ToString(),
                Code = code
            }, transaction);

            await InsertHistoryAsync(connection, transaction, code,
                string.IsNullOrWhiteSpace(reason)
                    ? $"Validation updated to {validation}"
                    : $"Validation updated to {validation}: {reason}");

            await transaction.CommitAsync();

            return await FindOneAsync(code);
        }

        private static async Task InsertHistoryAsync(
            System.Data.Common.DbConnection connection,
            System.Data.Common.DbTransaction transaction,
            string linkCode,
            string eventDescription)
        {
            await connection.ExecuteAsync(InsertHistorySql, new
            {
                Code = Guid.NewGuid().ToString(),
                LinkCode = linkCode,
                Event = eventDescription,
                CreationDate = DateTime.UtcNow
            }, transaction);
        }
    }
}
