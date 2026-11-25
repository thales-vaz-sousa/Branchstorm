using System.Data.Common;
using BranchstormDomain.Infrastructure.Database;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace BranchstormDomain.Infrastructure.Database.SqLite
{
    public class SqliteConnectionFactory(
           IConfiguration configuration
       ) : IDatabaseConnection
    {
        private readonly string connectionString = configuration.GetConnectionString("DefaultConnection")!;

        public DbConnection CreateConnection()
        {
            return new SqliteConnection(connectionString);
        }
    }
}
