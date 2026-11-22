using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FeedbackApiClassLibrary.Feedback.Entities.Dtos;

public class FeedbackDto
{
    [JsonIgnore]
    public string? Code { get; set; }

    [Required(ErrorMessage = "LinkCode is required")]
    public string LinkCode { get; init; } = string.Empty;

    [Required(ErrorMessage = "AutorCode is required")]
    public string AutorCode { get; init; } = string.Empty;

    [Required(ErrorMessage = "Text is required")]
    public string Text { get; init; } = string.Empty;

    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
    public decimal Rating { get; init; }
}
