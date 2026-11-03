namespace TeamApiClassLibrary.Team.Entities.Dtos
{
    public class TeamDto
    {
        public string Code { get; init; }
        public string Name { get; init; }
        public IEnumerable<TeamMember> Members { get; init; }
    }
}
