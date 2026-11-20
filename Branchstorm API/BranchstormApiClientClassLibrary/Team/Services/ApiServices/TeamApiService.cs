using System.Net;
using System.Net.Http.Json;
using BranchstormApiClientClassLibrary.Team.Entities.Dtos;
using BranchstormApiClientClassLibrary.Team.Entities.Results;

namespace BranchstormApiClientClassLibrary.Team.Services.ApiServices;

public class TeamApiService(HttpClient httpClient) : ITeamApiService
{
    private readonly HttpClient httpClient = httpClient;

    public async Task<TeamResult?> CreateAsync(TeamDto teamDto)
    {
        using HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/team", teamDto);
        if (!response.IsSuccessStatusCode)
        {
            string details = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Team API returned {(int)response.StatusCode}: {details}");
        }

        return await response.Content.ReadFromJsonAsync<TeamResult>();
    }

    public async Task<TeamResult?> FindOneAsync(string code)
    {
        using HttpResponseMessage response = await httpClient.GetAsync($"api/team/{code}");
        if (response.StatusCode is HttpStatusCode.NotFound) return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TeamResult>();
    }

    public async Task<TeamResult?> UpdateMembersAsync(string code, TeamDto teamDto)
    {
        using HttpResponseMessage response = await httpClient.PatchAsJsonAsync($"api/team/{code}", teamDto);
        if (response.StatusCode is HttpStatusCode.NotFound) return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TeamResult>();
    }
}
