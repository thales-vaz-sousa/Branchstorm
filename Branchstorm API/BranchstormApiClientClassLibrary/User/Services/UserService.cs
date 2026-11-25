using BranchstormApiClientClassLibrary.User.Entities.Dtos;
using BranchstormApiClientClassLibrary.User.Entities.Queries;
using BranchstormApiClientClassLibrary.User.Entities.Results;
using BranchstormApiClientClassLibrary.User.Services.ApiServices;

namespace BranchstormApiClientClassLibrary.User.Services;

public class UserService(IUserApiService userApiService) : IUserService
{
    private readonly IUserApiService userApiService = userApiService;

    public async Task<UserResult?> CreateAsync(UserDto userDto)
    {
        return await userApiService.CreateAsync(userDto);
    }

    public async Task<IEnumerable<UserResult>> FindAsync(FindUsersQuery query)
    {
        return await userApiService.FindAsync(query);
    }

    public async Task<UserResult?> FindOneAsync(string code)
    {
        return await userApiService.FindOneAsync(code);
    }
}
