public class DatabaseInitializer(
    IDatabaseConnection databaseConnection
)
{
    public async Task InitializeAsync()
    {
        var schema = await File.ReadAllTextAsync("Database/schemaUsers.sql");

        await using var connection = databaseConnection.CreateConnection();

        await connection.OpenAsync();
        await connection.ExecuteAsync(schema);
    }
}