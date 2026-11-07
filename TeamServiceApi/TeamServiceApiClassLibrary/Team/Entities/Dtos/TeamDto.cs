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

        public IEnumerable<TeamMember> Members { get; init; }
    }
}
