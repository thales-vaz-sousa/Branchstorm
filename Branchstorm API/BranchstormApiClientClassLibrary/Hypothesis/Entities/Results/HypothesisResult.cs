namespace BranchstormApiClientClassLibrary.Hypothesis.Entities.Results;

public record HypothesisResult
{
    public string Code { get; init; } = string.Empty;
    public string LinkCode { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string SuccessCriteria { get; init; } = string.Empty;
    public string? Result { get; init; }
    public DateTime CreationDate { get; init; }
}
