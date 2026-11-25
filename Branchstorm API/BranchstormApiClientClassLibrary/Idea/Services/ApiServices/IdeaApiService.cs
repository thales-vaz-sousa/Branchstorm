using System.Net;
using System.Net.Http.Json;
using BranchstormApiClientClassLibrary.Idea.Entities.Dtos;
using BranchstormApiClientClassLibrary.Idea.Entities.Results;

namespace BranchstormApiClientClassLibrary.Idea.Services.ApiServices;

public class IdeaApiService(HttpClient httpClient) : IIdeaApiService
{
    private readonly HttpClient httpClient = httpClient;

    public async Task<LinkResult?> CreateAsync(LinkDto linkDto)
    {
        using HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/idea", linkDto);
        return await ReadOneAsync(response);
    }

    public async Task<LinkResult?> FindOneAsync(string code)
    {
        using HttpResponseMessage response = await httpClient.GetAsync($"api/idea/{code}");
        return await ReadOneAsync(response);
    }

    public async Task<IEnumerable<LinkResult>> FindTreeAsync(string code)
    {
        using HttpResponseMessage response = await httpClient.GetAsync($"api/idea/tree/{code}");
        if (!response.IsSuccessStatusCode)
        {
            return [];
        }

        return await response.Content.ReadFromJsonAsync<IEnumerable<LinkResult>>() ?? [];
    }

    public async Task<LinkResult?> SelectAsync(string code, LinkValidationDto? body)
    {
        using HttpResponseMessage response = await httpClient.PutAsJsonAsync($"api/idea/select/{code}", body);
        return await ReadOneAsync(response);
    }

    public async Task<LinkResult?> UpdateValidationAsync(string code, LinkValidationDto validationDto)
    {
        using HttpResponseMessage response = await httpClient.PutAsJsonAsync($"api/idea/validation/{code}", validationDto);
        return await ReadOneAsync(response);
    }

    private static async Task<LinkResult?> ReadOneAsync(HttpResponseMessage response)
    {
        if (response.StatusCode is HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<LinkResult>();
    }
}
