using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;

namespace UserApiClassLibrary.User.Entities.Dtos
{
    public record UserDto
    {
        [JsonIgnore]
        public Guid Code { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; init; }

        [EmailAddress(ErrorMessage = "Invalid email address.")]
        [Required(ErrorMessage = "email is required")]
        public string Email { get; init; }

        public UserDto(string name, string email)
        {
            Name = name;
            Email = email;

        }

        public static Guid GenerateDeterministicGuid(string name, string email)
        {
            string input = $"{name.ToLower().Trim()}_{email.ToLower().Trim()}";
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = MD5.HashData(inputBytes);
            return new Guid(hashBytes);
        }

    }
}
