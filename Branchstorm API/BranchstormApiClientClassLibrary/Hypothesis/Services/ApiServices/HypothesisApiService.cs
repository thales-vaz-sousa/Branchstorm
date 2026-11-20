using System.Net;
using System.Net.Http.Json;
using BranchstormApiClientClassLibrary.Hypothesis.Entities.Dtos;
using BranchstormApiClientClassLibrary.Hypothesis.Entities.Results;

namespace BranchstormApiClientClassLibrary.Hypothesis.Services.ApiServices;

public class HypothesisApiService(HttpClient httpClient) : IHypothesisApiService
{
    private readonly HttpClient httpClient = httpClient;

    public async Task<HypothesisResult?> CreateAsync(HypothesisDto hypothesisDto)
    {
        using HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/hypothesis", hypothesisDto);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<HypothesisResult>();
    }

    public async Task<IEnumerable<HypothesisResult>> FindByLinkAsync(string linkCode)
    {
        using HttpResponseMessage response = await httpClient.GetAsync($"api/hypothesis?linkCode={Uri.EscapeDataString(linkCode)}");
        if (!response.IsSuccessStatusCode) return [];

        return await response.Content.ReadFromJsonAsync<IEnumerable<HypothesisResult>>() ?? [];
    }

    public async Task<HypothesisResult?> UpdateResultAsync(string code, HypothesisResultDto resultDto)
    {
        using HttpResponseMessage response = await httpClient.PutAsJsonAsync($"api/hypothesis/result/{code}", resultDto);
        if (response.StatusCode is HttpStatusCode.NotFound) return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<HypothesisResult>();
    }

    public async Task<IEnumerable<HypothesisResult>> FindByLinksAsync(IEnumerable<string> linkCodes)
    {
        string query = string.Join("&", linkCodes.Select(code => $"linkCodes={Uri.EscapeDataString(code)}"));
        using HttpResponseMessage response = await httpClient.GetAsync($"api/hypothesis?{query}");
        if (!response.IsSuccessStatusCode) return [];

        return await response.Content.ReadFromJsonAsync<IEnumerable<HypothesisResult>>() ?? [];
    }
}
