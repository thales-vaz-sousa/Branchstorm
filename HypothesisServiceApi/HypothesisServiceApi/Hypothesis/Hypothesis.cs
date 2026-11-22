namespace HypothesisServiceApi.Hypothesis;

public enum HypothesisStatus
{
    Pending,
    Validated,
    Refuted
}

public sealed class Hypothesis
{
    public Guid Code { get; set; } = Guid.NewGuid();
    public Guid LinkCode { get; set; }
    public string Description { get; set; } = string.Empty;
    public string SuccessCriteria { get; set; } = string.Empty;
    public string? ResultDescription { get; set; }
    public HypothesisStatus Status { get; set; } = HypothesisStatus.Pending;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? EvaluatedAtUtc { get; set; }
}