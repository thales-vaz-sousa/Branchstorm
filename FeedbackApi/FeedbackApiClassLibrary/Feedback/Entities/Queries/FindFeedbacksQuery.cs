using System.ComponentModel.DataAnnotations;

namespace FeedbackApiClassLibrary.Feedback.Entities.Queries;

public record FindFeedbacksQuery
{
    [Required(ErrorMessage = "LinkCodes is required")]
    [MaxLength(100, ErrorMessage = "Max 100 link codes per query")]
    public string[]? LinkCodes { get; init; }
}
