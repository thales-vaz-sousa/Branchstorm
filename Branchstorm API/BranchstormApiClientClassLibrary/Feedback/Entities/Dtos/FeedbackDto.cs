using System.ComponentModel.DataAnnotations;

namespace BranchstormApiClientClassLibrary.Feedback.Entities.Dtos;

public class FeedbackDto
{
    [Required]
    public string LinkCode { get; init; } = string.Empty;

    [Required]
    public string AutorCode { get; init; } = string.Empty;

    [Required]
    public string Text { get; init; } = string.Empty;

    [Range(0, 5)]
    public int Rating { get; init; }
}
