using BranchstormApiClientClassLibrary.Team.Entities.Dtos;

namespace BranchstormApiClientClassLibrary.Team.Entities.Results;

public record TeamResult
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public IEnumerable<TeamMember> Members { get; init; } = [];
}
