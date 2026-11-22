using BranchstormDomain.Infrastructure.Database;
using Dapper;
using FeedbackApiClassLibrary.Feedback.Entities.Dtos;
using FeedbackApiClassLibrary.Feedback.Entities.Queries;
using FeedbackApiClassLibrary.Feedback.Entities.Results;

namespace FeedbackApiClassLibrary.Feedback.Repositories;

internal class FeedbackRepository(IDatabaseConnection connectionDatabase) : IFeedbackRepository
{
    private const string CreateFeedbackSql = @"
        INSERT INTO Feedbacks (
            Code,
            LinkCode,
            AutorCode,
            Text,
            Rating,
            CreationDate
        )
        VALUES (
            @Code,
            @LinkCode,
            @AutorCode,
            @Text,
            @Rating,
            @CreationDate
        )
        RETURNING Code, LinkCode, AutorCode, Text, Rating, CreationDate;
    ";

    private const string FindByLinkCodeSql = @"
        SELECT
            Code,
            LinkCode,
            AutorCode,
            Text,
            Rating,
            CreationDate
        FROM Feedbacks
        WHERE LinkCode = @LinkCode
        ORDER BY CreationDate DESC;
    ";

    private const string FindByLinkCodesSql = @"
        SELECT
            Code,
            LinkCode,
            AutorCode,
            Text,
            Rating,
            CreationDate
        FROM Feedbacks
        WHERE LinkCode IN @LinkCodes
        ORDER BY CreationDate DESC;
    ";

    public async Task<FeedbackResult> CreateAsync(FeedbackDto feedbackDto)
    {
        await using var connection = connectionDatabase.CreateConnection();
        await connection.OpenAsync();

        return await connection.QuerySingleAsync<FeedbackResult>(
            CreateFeedbackSql,
            new
            {
                feedbackDto.Code,
                feedbackDto.LinkCode,
                feedbackDto.AutorCode,
                feedbackDto.Text,
                feedbackDto.Rating,
                CreationDate = DateTime.UtcNow
            });
    }

    public async Task<IEnumerable<FeedbackResult>> FindAsync(FindFeedbacksQuery query)
    {
        await using var connection = connectionDatabase.CreateConnection();
        await connection.OpenAsync();

        return await connection.QueryAsync<FeedbackResult>(
            FindByLinkCodesSql,
            new { LinkCodes = query.LinkCodes });
    }

    public async Task<FeedbackResult> FindOneAsync(FindFeedbackQuery query)
    {
        await using var connection = connectionDatabase.CreateConnection();
        await connection.OpenAsync();
        return await connection.QueryFirstOrDefaultAsync<FeedbackResult>(
            FindByLinkCodeSql,
            new { LinkCode = query.LinkCode });
    }
}
