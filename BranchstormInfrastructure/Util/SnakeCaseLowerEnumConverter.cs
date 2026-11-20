using System.Text.Json;
using System.Text.Json.Serialization;

namespace BranchstormDomain.Util;

public sealed class SnakeCaseLowerEnumConverter : JsonStringEnumConverter
{
    public SnakeCaseLowerEnumConverter()
        : base(JsonNamingPolicy.SnakeCaseLower, false)
    {
    }
}
