using BranchstormApiClientClassLibrary.Idea.Entities.Dtos;
using BranchstormApiClientClassLibrary.Idea.Entities.Results;

namespace BranchstormApiClientClassLibrary.Idea.Services;

public interface IIdeaService
{
    Task<LinkResult?> CreateAsync(LinkDto linkDto);
    Task<LinkResult?> FindOneAsync(string code);
    Task<IEnumerable<LinkResult>> FindTreeAsync(string code);
    Task<LinkResult?> SelectAsync(string code, LinkValidationDto? body);
    Task<LinkResult?> UpdateValidationAsync(string code, LinkValidationDto validationDto);
}
