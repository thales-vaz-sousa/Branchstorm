using Dapper;

namespace BranchstormDomain.Infrastructure.Database;

public class DatabaseInitializer(
    IDatabaseConnection databaseConnection
)
{
    public async Task InitializeAsync(string schemaPath)
    {
        var schema = await File.ReadAllTextAsync(schemaPath);

        await using var connection = databaseConnection.CreateConnection();

        await connection.OpenAsync();
        await connection.ExecuteAsync(schema);
    }
}