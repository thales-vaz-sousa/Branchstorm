using UserApiClassLibrary.User.Entities.Dtos;
using UserApiClassLibrary.User.Entities.Queries;
using UserApiClassLibrary.User.Entities.Results;
using UserApiClassLibrary.User.Repositories;

namespace UserApiClassLibrary.User.Services
{
    public class UserService(
        IUserRepository userRepository
        ) : IUserService
    {
        public Task CreateAsync(UserDto userDto)
        {
            return userRepository.CreateAsync(userDto);
        }

        public Task<IEnumerable<UserResult>> FindAsync()
        {
            return userRepository.FindAsync();
        }

        public Task<UserResult> FindOneAsync(FindUserQuery query)
        {
            return userRepository.FindOneAsync(query);
        }
    }
}
