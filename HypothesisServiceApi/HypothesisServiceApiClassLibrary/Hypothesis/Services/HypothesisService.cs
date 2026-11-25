using System.Net.Http.Json;
using BranchstormDomain.Domain;
using BranchstormDomain.Util;
using HypothesisServiceApi.Hypothesis.Entities.Dtos;
using HypothesisServiceApi.Hypothesis.Entities.Results;
using HypothesisServiceApi.Hypothesis.Repositories;

namespace HypothesisServiceApi.Hypothesis.Services
{
    public class HypothesisService(
        IHypothesisRepository hypothesisRepository,
        HttpClient httpClient
    ) : IHypothesisService
    {
        private readonly IHypothesisRepository hypothesisRepository = hypothesisRepository;
        private readonly HttpClient httpClient = httpClient;

        public async Task<HypothesisResult> CreateAsync(HypothesisDto hypothesisDto)
        {
            hypothesisDto.ValidateDomain();

            hypothesisDto.Code = DeterministicGuid.Generate(hypothesisDto).ToString();

            return await hypothesisRepository.CreateAsync(hypothesisDto);
        }

        public Task<HypothesisResult?> FindOneAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentException("Code cannot be null or empty.", nameof(code));
            }

            return hypothesisRepository.FindOneAsync(code);
        }

        public Task<IEnumerable<HypothesisResult>> FindByLinkCodeAsync(string linkCode)
        {
            if (string.IsNullOrWhiteSpace(linkCode))
            {
                throw new ArgumentException("LinkCode cannot be null or empty.", nameof(linkCode));
            }

            return hypothesisRepository.FindByLinkCodeAsync(linkCode);
        }

        public Task<IEnumerable<HypothesisResult>> FindByLinkCodesAsync(IEnumerable<string> linkCodes)
        {
            if (linkCodes == null || !linkCodes.Any())
            {
                throw new ArgumentException("LinkCodes cannot be null or empty.", nameof(linkCodes));
            }

            var codes = linkCodes.ToList();
            if (codes.Count > 100)
            {
                throw new ArgumentException("Maximum 100 link codes allowed at once.", nameof(linkCodes));
            }

            return hypothesisRepository.FindByLinkCodesAsync(codes);
        }

        public async Task<HypothesisResult> RegisterResultAsync(string code, string result)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentException("Code cannot be null or empty.", nameof(code));
            }

            if (string.IsNullOrWhiteSpace(result))
            {
                throw new ArgumentException("Result cannot be null or empty.", nameof(result));
            }

            var hypothesis = await hypothesisRepository.RegisterResultAsync(code, result);

            await UpdateLinkValidationAsync(hypothesis.LinkCode, result);

            return hypothesis;
        }

        private async Task UpdateLinkValidationAsync(string linkCode, string result)
        {
            try
            {
                var validationDto = new { Validation = result };
                await httpClient.PutAsJsonAsync($"api/idea/validation/{linkCode}", validationDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating link validation: {ex.Message}");
            }
        }
    }
}
