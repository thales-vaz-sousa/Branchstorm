namespace FeedbackApiClassLibrary.Feedback;

public class Feedback
{
    public string Code { get; set; } = string.Empty;
    public string LinkCode { get; set; } = string.Empty;
    public string AutorCode { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public decimal Rating { get; set; }
    public DateTime CreationDate { get; set; }
}
