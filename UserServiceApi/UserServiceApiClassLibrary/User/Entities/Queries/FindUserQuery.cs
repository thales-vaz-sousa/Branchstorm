using System.ComponentModel.DataAnnotations;

namespace UserApiClassLibrary.User.Entities.Queries
{
    public record FindUserQuery
    {
        [Required(ErrorMessage = "code is required")]
        public string? Code { get; init; }
        public string? Name { get; init; }
        public string? Email { get; init; }
    }
}
