using System.ComponentModel.DataAnnotations;

namespace HypothesisServiceApi.Hypothesis.Entities.Dtos
{
    public class HypothesisResultDto
    {
        [Required(ErrorMessage = "Result is required")]
        public required string Result { get; init; }
    }
}
