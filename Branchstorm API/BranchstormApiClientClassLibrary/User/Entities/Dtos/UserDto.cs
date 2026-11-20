using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BranchstormApiClientClassLibrary.User.Entities.Dtos;

public record UserDto
{
    [JsonIgnore]
    public string? Code { get; init; }

    [Required]
    public string Name { get; init; } = string.Empty;

    [EmailAddress]
    [Required]
    public string Email { get; init; } = string.Empty;
}
