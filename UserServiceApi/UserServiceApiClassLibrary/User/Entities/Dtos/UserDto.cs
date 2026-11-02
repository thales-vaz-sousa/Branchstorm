using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;

namespace UserApiClassLibrary.User.Entities.Dtos
{
    public record UserDto
    {
        [JsonIgnore]
        public Guid Code { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }

        public UserDto(string name, string email)
        {
            Name = name;
            Email = email;

            Code = GenerateDeterministicGuid(name, email);
        }

        private static Guid GenerateDeterministicGuid(string name, string email)
        {
            Guid result;

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email))
            {
                throw new ArgumentException("Name and email must not be null or empty.");
            }
            else
            {
                string input = $"{name.ToLower().Trim()}_{email.ToLower().Trim()}";

                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = MD5.HashData(inputBytes);

                result = new Guid(hashBytes);
            }

            return result;
        }

    }
}
