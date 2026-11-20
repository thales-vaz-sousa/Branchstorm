using BranchstormApiClientClassLibrary.Idea.Entities.Dtos;
using BranchstormApiClientClassLibrary.Idea.Entities.Results;
using BranchstormApiClientClassLibrary.Idea.Services.ApiServices;

namespace BranchstormApiClientClassLibrary.Idea.Services;

public class IdeaService(IIdeaApiService ideaApiService) : IIdeaService
{
    private readonly IIdeaApiService ideaApiService = ideaApiService;

    public async Task<LinkResult?> CreateAsync(LinkDto linkDto)
    {
        return await ideaApiService.CreateAsync(linkDto);
    }

    public async Task<LinkResult?> FindOneAsync(string code)
    {
        return await ideaApiService.FindOneAsync(code);
    }

    public async Task<IEnumerable<LinkResult>> FindTreeAsync(string code)
    {
        return await ideaApiService.FindTreeAsync(code);
    }

    public async Task<LinkResult?> SelectAsync(string code, LinkValidationDto? body)
    {
        return await ideaApiService.SelectAsync(code, body);
    }

    public async Task<LinkResult?> UpdateValidationAsync(string code, LinkValidationDto validationDto)
    {
        return await ideaApiService.UpdateValidationAsync(code, validationDto);
    }
}
