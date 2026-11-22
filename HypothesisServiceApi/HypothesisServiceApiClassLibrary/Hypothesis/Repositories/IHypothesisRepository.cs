using HypothesisServiceApi.Hypothesis.Entities.Dtos;
using HypothesisServiceApi.Hypothesis.Entities.Results;

namespace HypothesisServiceApi.Hypothesis.Repositories
{
    public interface IHypothesisRepository
    {
        Task<HypothesisResult> CreateAsync(HypothesisDto hypothesisDto);
        Task<HypothesisResult?> FindOneAsync(string code);
        Task<IEnumerable<HypothesisResult>> FindByLinkCodeAsync(string linkCode);
        Task<IEnumerable<HypothesisResult>> FindByLinkCodesAsync(IEnumerable<string> linkCodes);
        Task<HypothesisResult> RegisterResultAsync(string code, string result);
    }
}
