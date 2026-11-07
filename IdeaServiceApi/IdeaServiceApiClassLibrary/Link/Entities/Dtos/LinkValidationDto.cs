using System.ComponentModel.DataAnnotations;

namespace IdeaApiClassLibrary.Link.Entities.Dtos
{
    public class LinkValidationDto
    {
        [Required(ErrorMessage = "Validation is required")]
        public LinkValidation Validation { get; init; }

        public string? Reason { get; init; }
    }
}
