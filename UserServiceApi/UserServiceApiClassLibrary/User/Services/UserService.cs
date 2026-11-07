using BranchstormDomain.Domain;
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
        public async Task<UserResult> CreateAsync(UserDto userDto)
        {
            userDto.ValidateDomain();

            userDto.Code = UserDto.GenerateDeterministicGuid(userDto.Name, userDto.Email);

            return await userRepository.CreateAsync(userDto);
        }

        public Task<IEnumerable<UserResult>> FindAsync(FindUsersQuery query)
        {
            query.ValidateDomain();

            query = query with { Codes = query.Codes?.Distinct().ToArray() };

            return userRepository.FindAsync(query);
        }

        public Task<UserResult?> FindOneAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentException("Code cannot be null or empty.", nameof(code));
            }

            return userRepository.FindOneAsync(code);
        }
    }
}
