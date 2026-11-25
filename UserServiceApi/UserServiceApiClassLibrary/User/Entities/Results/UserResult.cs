namespace UserApiClassLibrary.User.Entities.Results
{
    public record UserResult
    {
        public string Code { get; init; }
        public string Name { get; init; }
        public string Email { get; init; }
    }
}
