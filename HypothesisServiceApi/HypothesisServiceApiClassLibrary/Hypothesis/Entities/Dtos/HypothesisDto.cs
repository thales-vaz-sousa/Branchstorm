using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HypothesisServiceApi.Hypothesis.Entities.Dtos
{
    public class HypothesisDto
    {
        [JsonIgnore]
        public string? Code { get; set; }

        [Required(ErrorMessage = "LinkCode is required")]
        public required string LinkCode { get; init; }

        [Required(ErrorMessage = "Description is required")]
        public required string Description { get; init; }

        [Required(ErrorMessage = "SuccessCriteria is required")]
        public required string SuccessCriteria { get; init; }

        public string? Result { get; init; }
    }
}

