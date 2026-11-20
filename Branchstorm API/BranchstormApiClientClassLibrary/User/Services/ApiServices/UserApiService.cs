using System.Net;
using System.Net.Http.Json;
using BranchstormApiClientClassLibrary.User.Entities.Dtos;
using BranchstormApiClientClassLibrary.User.Entities.Queries;
using BranchstormApiClientClassLibrary.User.Entities.Results;

namespace BranchstormApiClientClassLibrary.User.Services.ApiServices;

public class UserApiService(HttpClient httpClient) : IUserApiService
{
    private readonly HttpClient httpClient = httpClient;

    public async Task<UserResult?> CreateAsync(UserDto userDto)
    {
        using HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/user", userDto);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<UserResult>();
    }

    public async Task<IEnumerable<UserResult>> FindAsync(FindUsersQuery query)
    {
        List<string> parameters = [];

        if (!string.IsNullOrWhiteSpace(query.Code)) parameters.Add($"code={Uri.EscapeDataString(query.Code)}");
        if (!string.IsNullOrWhiteSpace(query.Name)) parameters.Add($"name={Uri.EscapeDataString(query.Name)}");
        if (!string.IsNullOrWhiteSpace(query.Email)) parameters.Add($"email={Uri.EscapeDataString(query.Email)}");
        if (query.Codes is not null)
        {
            foreach (string code in query.Codes)
            {
                parameters.Add($"codes={Uri.EscapeDataString(code)}");
            }
        }

        string url = parameters.Count == 0 ? "api/user" : $"api/user?{string.Join("&", parameters)}";
        using HttpResponseMessage response = await httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode) return [];

        return await response.Content.ReadFromJsonAsync<IEnumerable<UserResult>>() ?? [];
    }

    public async Task<UserResult?> FindOneAsync(string code)
    {
        using HttpResponseMessage response = await httpClient.GetAsync($"api/user/{code}");
        if (response.StatusCode is HttpStatusCode.NotFound) return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<UserResult>();
    }
}
