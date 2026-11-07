using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace IdeaApiClassLibrary.Link.Entities.Dtos
{
    public class LinkDto
    {
        [JsonIgnore]
        public string? Code { get; set; }

        public string? FatherCode { get; init; }

        [Required(ErrorMessage = "Type is required")]
        public LinkType Type { get; init; }

        [Required(ErrorMessage = "Title is required")]
        public string Title { get; init; }

        public string? Description { get; init; }

        public string? AutorCode { get; init; }

        public string? TeamCode { get; init; }

        public string? Reason { get; init; }
    }
}
