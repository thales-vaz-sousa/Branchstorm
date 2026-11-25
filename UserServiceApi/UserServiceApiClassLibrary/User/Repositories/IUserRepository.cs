using UserApiClassLibrary.User.Entities.Dtos;
using UserApiClassLibrary.User.Entities.Queries;
using UserApiClassLibrary.User.Entities.Results;

namespace UserApiClassLibrary.User.Repositories
{
    public interface IUserRepository
    {
        Task<UserResult> CreateAsync(UserDto userDto);
        Task<IEnumerable<UserResult>> FindAsync(FindUsersQuery query);
        Task<UserResult?> FindOneAsync(string code);
    }
}
