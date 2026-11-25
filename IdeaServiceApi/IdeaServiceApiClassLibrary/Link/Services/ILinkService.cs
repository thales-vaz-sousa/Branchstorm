using IdeaApiClassLibrary.Link.Entities.Dtos;
using IdeaApiClassLibrary.Link.Entities.Results;

namespace IdeaApiClassLibrary.Link.Services
{
    public interface ILinkService
    {
        Task<LinkResult> CreateAsync(LinkDto linkDto);
        Task<LinkResult?> FindOneAsync(string code);
        Task<IEnumerable<LinkResult>> FindTreeAsync(string code);
        Task<LinkResult?> SelectAsync(string code, string? reason);
        Task<LinkResult?> UpdateValidationAsync(string code, LinkValidationDto validationDto);
    }
}
