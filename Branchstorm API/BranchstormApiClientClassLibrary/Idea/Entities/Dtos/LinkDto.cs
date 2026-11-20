using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using BranchstormDomain.Util;

namespace BranchstormApiClientClassLibrary.Idea.Entities.Dtos;

public class LinkDto
{
    [JsonIgnore]
    public string? Code { get; set; }

    public string? FatherCode { get; init; }

    [Required]
    public LinkType Type { get; init; }

    [Required]
    public string Title { get; init; }

    public string? Description { get; init; }
    public string? AutorCode { get; init; }
    public string? TeamCode { get; init; }
    public string? Reason { get; init; }
}

[JsonConverter(typeof(SnakeCaseLowerEnumConverter))]
public enum LinkType
{
    Problem,
    Solution,
    Version
}
