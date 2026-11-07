using System.Data.Common;

namespace BranchstormDomain.Infrastructure.Database
{
    public interface IDatabaseConnection
    {
        DbConnection CreateConnection();
    }
}