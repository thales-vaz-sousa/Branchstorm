namespace BranchstormApiClientClassLibrary.User.Entities.Queries;

public record FindUsersQuery
{
    public string? Code { get; init; }
    public string? Name { get; init; }
    public string? Email { get; init; }
    public string[]? Codes { get; init; }
}
