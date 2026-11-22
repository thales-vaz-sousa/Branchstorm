namespace HypothesisServiceApi.Hypothesis
{
    public class Hypothesis
    {
        public required string Code { get; set; }
        public required string LinkCode { get; set; }
        public required string Description { get; set; }
        public required string SuccessCriteria { get; set; }
        public string? Result { get; set; }
        public DateTime CreationDate { get; set; }
    }
}

