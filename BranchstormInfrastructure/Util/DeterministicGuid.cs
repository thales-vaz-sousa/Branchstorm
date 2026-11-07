using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace BranchstormDomain.Util
{
    public static class DeterministicGuid
    {
        public static Guid Generate<T>(T obj) where T : class
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            string jsonString = JsonSerializer.Serialize(obj);

            byte[] inputBytes = Encoding.UTF8.GetBytes(jsonString.ToLowerInvariant());
            byte[] hashBytes = MD5.HashData(inputBytes);
            return new Guid(hashBytes);
        }
    }
}
