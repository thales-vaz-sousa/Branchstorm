using BranchstormApiClientClassLibrary.User.Entities.Dtos;
using BranchstormApiClientClassLibrary.User.Entities.Queries;
using BranchstormApiClientClassLibrary.User.Entities.Results;

namespace BranchstormApiClientClassLibrary.User.Services;

public interface IUserService
{
    Task<UserResult?> CreateAsync(UserDto userDto);
    Task<IEnumerable<UserResult>> FindAsync(FindUsersQuery query);
    Task<UserResult?> FindOneAsync(string code);
}
