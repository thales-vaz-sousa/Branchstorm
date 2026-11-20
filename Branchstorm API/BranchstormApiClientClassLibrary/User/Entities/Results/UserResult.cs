namespace BranchstormApiClientClassLibrary.User.Entities.Results;

public record UserResult
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
}
