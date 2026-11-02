namespace UserApiClassLibrary.User.Entities.Results
{
    public record UserResult
    {
        public Guid Code { get; init; }
        public string Name { get; init; }
    }
}
