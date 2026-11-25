using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TeamApiClassLibrary.Team.Entities.Dtos
{
    public class TeamDto
    {
        [JsonIgnore]
        public string? Code { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; init; }

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; init; }

        [Required(ErrorMessage = "Members collection is required")]
        public IEnumerable<TeamMember> Members { get; init; }
    }
}
