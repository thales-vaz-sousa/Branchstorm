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
    }
}
