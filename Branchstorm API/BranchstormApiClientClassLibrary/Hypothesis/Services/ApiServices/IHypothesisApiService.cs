using BranchstormApiClientClassLibrary.Hypothesis.Entities.Dtos;
using BranchstormApiClientClassLibrary.Hypothesis.Entities.Results;

namespace BranchstormApiClientClassLibrary.Hypothesis.Services.ApiServices;

public interface IHypothesisApiService
{
    Task<HypothesisResult?> CreateAsync(HypothesisDto hypothesisDto);
    Task<IEnumerable<HypothesisResult>> FindByLinkAsync(string linkCode);
    Task<HypothesisResult?> UpdateResultAsync(string code, HypothesisResultDto resultDto);
    Task<IEnumerable<HypothesisResult>> FindByLinksAsync(IEnumerable<string> linkCodes);
}
