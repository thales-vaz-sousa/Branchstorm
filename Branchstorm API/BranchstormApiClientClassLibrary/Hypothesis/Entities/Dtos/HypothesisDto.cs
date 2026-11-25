using System.ComponentModel.DataAnnotations;

namespace BranchstormApiClientClassLibrary.Hypothesis.Entities.Dtos;

public class HypothesisDto
{
    [Required]
    public string LinkCode { get; init; } = string.Empty;

    [Required]
    public string Description { get; init; } = string.Empty;

    [Required]
    public string SuccessCriteria { get; init; } = string.Empty;
}
