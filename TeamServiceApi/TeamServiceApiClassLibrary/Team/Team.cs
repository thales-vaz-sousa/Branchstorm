using System.Text.Json.Serialization;

namespace TeamApiClassLibrary.Team
{
    public class Team
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public IEnumerable<TeamMember> Members { get; set; }
    }

    public class TeamMember
    {
        public string? TeamCode { get; set; }
        public string UserCode { get; set; }
        public TeamMemberRole Role { get; set; }
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum TeamMemberRole
    {
        Admin = 0,
        Owner = 1,
        Collaborator = 2,
        Evaluator = 3
    }
}
