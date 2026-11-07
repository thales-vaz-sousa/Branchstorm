namespace TeamApiClassLibrary.Team.Entities.Results
{
    public record TeamResult
    {
        public string Code { get; init; }
        public string Name { get; init; }
        public IEnumerable<TeamMember> Members { get; init; }
    }
}
