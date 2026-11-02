using System.ComponentModel.DataAnnotations;

namespace UserApiClassLibrary.User.Entities.Queries
{
    public record FindUserQuery
    {
        public Guid Code { get; init; }
        public string? Name { get; init; }
        public string? Email { get; init; }
    }
}
