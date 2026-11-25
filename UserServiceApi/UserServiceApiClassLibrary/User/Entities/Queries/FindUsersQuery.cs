using System.ComponentModel.DataAnnotations;

namespace UserApiClassLibrary.User.Entities.Queries
{
    public record FindUsersQuery
    {
        public string? Code { get; init; }
        public string? Name { get; init; }
        public string? Email { get; init; }

        [MaxLength(100, ErrorMessage = "max 100 codes per query")]
        public string[]? Codes { get; init; }
    }
}
