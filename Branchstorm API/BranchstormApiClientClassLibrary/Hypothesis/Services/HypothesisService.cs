using BranchstormApiClientClassLibrary.Hypothesis.Entities.Dtos;
using BranchstormApiClientClassLibrary.Hypothesis.Entities.Results;
using BranchstormApiClientClassLibrary.Hypothesis.Services.ApiServices;

namespace BranchstormApiClientClassLibrary.Hypothesis.Services;

public class HypothesisService(IHypothesisApiService hypothesisApiService) : IHypothesisService
{
    private readonly IHypothesisApiService hypothesisApiService = hypothesisApiService;

    public async Task<HypothesisResult?> CreateAsync(HypothesisDto hypothesisDto)
    {
        return await hypothesisApiService.CreateAsync(hypothesisDto);
    }

    public async Task<IEnumerable<HypothesisResult>> FindByLinkAsync(string linkCode)
    {
        return await hypothesisApiService.FindByLinkAsync(linkCode);
    }

    public async Task<HypothesisResult?> UpdateResultAsync(string code, HypothesisResultDto resultDto)
    {
        return await hypothesisApiService.UpdateResultAsync(code, resultDto);
    }

    public async Task<IEnumerable<HypothesisResult>> FindByLinksAsync(IEnumerable<string> linkCodes)
    {
        return await hypothesisApiService.FindByLinksAsync(linkCodes);
    }
}
