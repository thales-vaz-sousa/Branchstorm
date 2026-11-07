namespace IdeaApiClassLibrary.Link.Entities
{
    public class Link
    {
        public string Code { get; set; }
        public string? FatherCode { get; set; }
        public LinkType Type { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public LinkStatus Status { get; set; }
        public LinkValidation Validation { get; set; }
        public string? AutorCode { get; set; }
        public string? TeamCode { get; set; }
        public string? Reason { get; set; }
        public DateTime CreationDate { get; set; }
    }

    public class LinkHistory
    {
        public string Code { get; set; }
        public string LinkCode { get; set; }
        public string Event { get; set; }
        public DateTime CreationDate { get; set; }
    }

    public enum LinkType
    {
        Problem,
        Solution,
        Version
    }

    public enum LinkStatus
    {
        Open,
        Selected,
        Discarded,
        Final
    }

    public enum LinkValidation
    {
        NotTested,
        Validated,
        Refuted
    }
}
