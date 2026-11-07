namespace IdeaApiClassLibrary.Link.Entities.Results
{
    public record LinkResult
    {
        public string Code { get; init; }
        public string? FatherCode { get; init; }
        public string Type { get; init; }
        public string Title { get; init; }
        public string? Description { get; init; }
        public string Status { get; init; }
        public string Validation { get; init; }
        public string? AutorCode { get; init; }
        public string? TeamCode { get; init; }
        public string? Reason { get; init; }
        public DateTime CreationDate { get; init; }
        public IEnumerable<LinkHistoryResult> History { get; init; } = [];
    }
}
