using System.ComponentModel.DataAnnotations;

namespace HypothesisServiceApi.Hypothesis;

public sealed class CreateHypothesisRequest
{
    [Required]
    public Guid LinkCode { get; init; }

    [Required, MinLength(5), MaxLength(1000)]
    public string Description { get; init; } = string.Empty;

    [Required, MinLength(3), MaxLength(1000)]
    public string SuccessCriteria { get; init; } = string.Empty;
}

public enum HypothesisResult
{
    Validated = 1,
    Refuted = 2
}

public sealed class RegisterHypothesisResultRequest
{
    [Required]
    public HypothesisResult Result { get; init; }

    [Required, MinLength(3), MaxLength(2000)]
    public string ResultDescription { get; init; } = string.Empty;
}