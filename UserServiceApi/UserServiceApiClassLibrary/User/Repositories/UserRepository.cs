using UserApiClassLibrary.User.Entities.Dtos;
using UserApiClassLibrary.User.Entities.Queries;
using UserApiClassLibrary.User.Entities.Results;

namespace UserApiClassLibrary.User.Repositories
{
    internal class UserRepository (): IUserRepository
    {
        public Task CreateAsync(UserDto userDto)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<UserResult>> FindAsync()
        {
            throw new NotImplementedException();
        }

        public Task<UserResult> FindOneAsync(FindUserQuery query)
        {
            throw new NotImplementedException();
        }
    }
}
