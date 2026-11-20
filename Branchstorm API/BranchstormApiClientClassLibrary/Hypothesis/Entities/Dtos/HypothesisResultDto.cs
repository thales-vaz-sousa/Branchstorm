using System.ComponentModel.DataAnnotations;

namespace BranchstormApiClientClassLibrary.Hypothesis.Entities.Dtos;

public class HypothesisResultDto
{
    [Required]
    public string Result { get; init; } = string.Empty;
}
