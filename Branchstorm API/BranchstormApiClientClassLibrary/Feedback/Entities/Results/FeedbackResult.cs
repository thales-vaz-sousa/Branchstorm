namespace BranchstormApiClientClassLibrary.Feedback.Entities.Results;

public record FeedbackResult
{
    public string Code { get; init; } = string.Empty;
    public string LinkCode { get; init; } = string.Empty;
    public string AutorCode { get; init; } = string.Empty;
    public string Text { get; init; } = string.Empty;
    public int Rating { get; init; }
    public DateTime CreationDate { get; init; }
}
