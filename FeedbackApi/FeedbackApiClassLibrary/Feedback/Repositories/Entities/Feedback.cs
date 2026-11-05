namespace FeedbackApiClassLibrary.Feedback
{
    public class Feedback
    {
        public Guid Code { get; set; }
        public string LinkCode { get; set; }
        public string AutorCode { get; set; }
        public string Test { get; set; }
        public int Rating { get; set; }
        public DateTime CreationDate { get; set; }

    }
}
