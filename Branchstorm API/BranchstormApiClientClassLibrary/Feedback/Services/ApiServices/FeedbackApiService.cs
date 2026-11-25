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
        return await ReadOneAsync(response);
    }

    public async Task<IEnumerable<FeedbackResult>> FindByLinkAsync(string linkCode)
    {
        using HttpResponseMessage response = await httpClient.GetAsync($"api/feedbacks/{linkCode}");
        var result = await ReadOneAsync(response);
        return result is not null ? [result] : [];
    }

    public async Task<IEnumerable<FeedbackResult>> FindByLinksAsync(IEnumerable<string> linkCodes)
    {
        string query = string.Join("&", linkCodes.Select(code => $"LinkCodes={code}"));
        using HttpResponseMessage response = await httpClient.GetAsync($"api/feedbacks?{query}");

        response.EnsureSuccessStatusCode();

        IEnumerable<FeedbackResult> result =
            await response.Content.ReadFromJsonAsync<IEnumerable<FeedbackResult>>() ?? [];

        return result;
    }

    private static async Task<FeedbackResult?> ReadOneAsync(HttpResponseMessage response)
    {
        FeedbackResult? result = null;

        if (response.StatusCode is not HttpStatusCode.NotFound)
        {
            response.EnsureSuccessStatusCode();
            result = await response.Content.ReadFromJsonAsync<FeedbackResult>();
        }

        return result;
    }
}
