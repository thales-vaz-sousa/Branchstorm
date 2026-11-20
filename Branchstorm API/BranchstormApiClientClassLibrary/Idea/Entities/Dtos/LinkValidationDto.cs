using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using BranchstormDomain.Util;

namespace BranchstormApiClientClassLibrary.Idea.Entities.Dtos;

public class LinkValidationDto
{
    [Required]
    public LinkValidation Validation { get; init; }

    public string? Reason { get; init; }
}

[JsonConverter(typeof(SnakeCaseLowerEnumConverter))]
public enum LinkValidation
{
    NotTested,
    Validated,
    Refuted
}
