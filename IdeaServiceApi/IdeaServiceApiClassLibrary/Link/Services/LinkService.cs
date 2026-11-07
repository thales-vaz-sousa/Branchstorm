using BranchstormDomain.Domain;
using BranchstormDomain.Util;
using IdeaApiClassLibrary.Link.Entities;
using IdeaApiClassLibrary.Link.Entities.Dtos;
using IdeaApiClassLibrary.Link.Entities.Results;
using IdeaApiClassLibrary.Link.Repositories;

namespace IdeaApiClassLibrary.Link.Services
{
    public class LinkService(
        ILinkRepository linkRepository
        ) : ILinkService
    {
        public async Task<LinkResult> CreateAsync(LinkDto linkDto)
        {
            linkDto.ValidateDomain();

            if (linkDto.Type == LinkType.Problem && !string.IsNullOrWhiteSpace(linkDto.FatherCode))
            {
                throw new ArgumentException("A problem link cannot have a father.", nameof(linkDto));
            }

            if (linkDto.Type != LinkType.Problem && string.IsNullOrWhiteSpace(linkDto.FatherCode))
            {
                throw new ArgumentException("A solution or version link must have a father.", nameof(linkDto));
            }

            if (!string.IsNullOrWhiteSpace(linkDto.FatherCode))
            {
                var father = await linkRepository.FindOneAsync(linkDto.FatherCode);

                if (father is null)
                {
                    throw new ArgumentException($"Father link '{linkDto.FatherCode}' does not exist.", nameof(linkDto));
                }
            }

            linkDto.Code = DeterministicGuid.Generate(linkDto).ToString();

            return await linkRepository.CreateAsync(linkDto);
        }

        public Task<LinkResult?> FindOneAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentException("Code cannot be null or empty.", nameof(code));
            }

            return linkRepository.FindOneAsync(code);
        }

        public Task<IEnumerable<LinkResult>> FindTreeAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentException("Code cannot be null or empty.", nameof(code));
            }

            return linkRepository.FindTreeAsync(code);
        }

        public Task<LinkResult?> SelectAsync(string code, string? reason)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentException("Code cannot be null or empty.", nameof(code));
            }

            return linkRepository.SelectAsync(code, reason);
        }

        public Task<LinkResult?> UpdateValidationAsync(string code, LinkValidationDto validationDto)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentException("Code cannot be null or empty.", nameof(code));
            }

            validationDto.ValidateDomain();

            return linkRepository.UpdateValidationAsync(code, validationDto.Validation, validationDto.Reason);
        }
    }
}
