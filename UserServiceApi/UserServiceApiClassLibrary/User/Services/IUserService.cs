using UserApiClassLibrary.User.Entities.Dtos;
using UserApiClassLibrary.User.Entities.Queries;
using UserApiClassLibrary.User.Entities.Results;

namespace UserApiClassLibrary.User.Services
{
    public interface IUserService
    {
        Task<UserResult> CreateAsync(UserDto userDto);
        Task<UserResult?> FindOneAsync(string code);
        Task<IEnumerable<UserResult>> FindAsync(FindUsersQuery query);
    }
}