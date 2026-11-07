using IdeaApiClassLibrary.Link.Entities;
using IdeaApiClassLibrary.Link.Entities.Dtos;
using IdeaApiClassLibrary.Link.Entities.Results;

namespace IdeaApiClassLibrary.Link.Repositories
{
    public interface ILinkRepository
    {
        Task<LinkResult> CreateAsync(LinkDto linkDto);
        Task<LinkResult?> FindOneAsync(string code);
        Task<IEnumerable<LinkResult>> FindTreeAsync(string code);
        Task<LinkResult?> SelectAsync(string code, string? reason);
        Task<LinkResult?> UpdateValidationAsync(string code, LinkValidation validation, string? reason);
    }
}
