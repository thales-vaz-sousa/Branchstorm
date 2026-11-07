namespace IdeaApiClassLibrary.Link.Entities.Results
{
    public record LinkHistoryResult
    {
        public string Code { get; init; }
        public string LinkCode { get; init; }
        public string Event { get; init; }
        public DateTime CreationDate { get; init; }
    }
}
