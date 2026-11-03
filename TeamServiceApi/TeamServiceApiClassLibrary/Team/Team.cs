namespace TeamApiClassLibrary.Team
{
    public class Team
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public IEnumerable<TeamMember> Members { get; set; }
    }

    public class TeamMember
    {
        public string TeamCode { get; set; }
        public string UserCode { get; set; }
        public string Role { get; set; }
    }
}
