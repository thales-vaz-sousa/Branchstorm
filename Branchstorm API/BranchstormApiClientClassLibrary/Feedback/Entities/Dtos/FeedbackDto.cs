using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BranchstormApiClientClassLibrary.Feedback.Entities.Dtos;

public class FeedbackDto
{
    [JsonIgnore]
    public string? Code { get; set; }

    [Required]
    public string LinkCode { get; init; } = string.Empty;

    [Required]
    public string AutorCode { get; init; } = string.Empty;

    [Required]
    public string Text { get; init; } = string.Empty;

    [Range(1, 5)]
    public decimal Rating { get; init; }
}
