using System.Net;
using System.Net.Http.Json;
using BranchstormApiClientClassLibrary.Feedback.Entities.Dtos;
using BranchstormApiClientClassLibrary.Feedback.Entities.Results;

namespace BranchstormApiClientClassLibrary.Feedback.Services.ApiServices;

public class FeedbackApiService(HttpClient httpClient) : IFeedbackApiService
{
    private readonly HttpClient httpClient = httpClient;

    public async Task<FeedbackResult?> CreateAsync(FeedbackDto feedbackDto)
    {
        using HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/feedbacks", feedbackDto);
        if (response.StatusCode is HttpStatusCode.NotFound) return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<FeedbackResult>();
    }

    public async Task<IEnumerable<FeedbackResult>> FindByLinkAsync(string linkCode)
    {
        using HttpResponseMessage response = await httpClient.GetAsync($"api/feedbacks?linkCode={Uri.EscapeDataString(linkCode)}");
        if (!response.IsSuccessStatusCode) return [];

        return await response.Content.ReadFromJsonAsync<IEnumerable<FeedbackResult>>() ?? [];
    }

    public async Task<IEnumerable<FeedbackResult>> FindByLinksAsync(IEnumerable<string> linkCodes)
    {
        string query = string.Join("&", linkCodes.Select(code => $"linkCodes={Uri.EscapeDataString(code)}"));
        using HttpResponseMessage response = await httpClient.GetAsync($"api/feedbacks?{query}");
        if (!response.IsSuccessStatusCode) return [];

        return await response.Content.ReadFromJsonAsync<IEnumerable<FeedbackResult>>() ?? [];
    }
}
