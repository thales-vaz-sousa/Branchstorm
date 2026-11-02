using UserApiClassLibrary.User.Entities.Dtos;
using UserApiClassLibrary.User.Entities.Queries;
using UserApiClassLibrary.User.Entities.Results;

namespace UserApiClassLibrary.User.Repositories
{
    public interface IUserRepository
    {
        Task CreateAsync(UserDto userDto);
        Task<UserResult> FindOneAsync(FindUserQuery query);
        Task<IEnumerable<UserResult>> FindAsync();
    }
}
