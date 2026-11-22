namespace HypothesisServiceApi.Hypothesis.Entities.Results
{
    public record HypothesisResult
    {
        public required string Code { get; init; }
        public required string LinkCode { get; init; }
        public required string Description { get; init; }
        public required string SuccessCriteria { get; init; }
        public string? Result { get; init; }
        public DateTime CreationDate { get; init; }
    }
}

