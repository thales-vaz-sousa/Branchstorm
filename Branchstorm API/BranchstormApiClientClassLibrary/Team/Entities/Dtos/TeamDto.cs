using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BranchstormApiClientClassLibrary.Team.Entities.Dtos;

public class TeamDto
{
    [JsonIgnore]
    public string? Code { get; init; }

    [Required]
    public string Name { get; init; } = string.Empty;

    public IEnumerable<TeamMember> Members { get; init; } = [];
}

public class TeamMember
{
    public string? TeamCode { get; init; }
    public string UserCode { get; init; } = string.Empty;
    public TeamMemberRole Role { get; init; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TeamMemberRole
{
    Admin,
    Owner,
    Collaborator,
    Evaluator
}
