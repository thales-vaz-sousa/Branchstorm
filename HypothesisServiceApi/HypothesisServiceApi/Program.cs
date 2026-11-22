using BranchstormDomain.Infrastructure.Database;
using HypothesisServiceApi.Containers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDependencyInjectionContainers();
builder.Services.AddOpenApi();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider
        .GetRequiredService<DatabaseInitializer>();

    var schemaPath = Path.Combine(
        app.Environment.ContentRootPath,
        "Database",
        "schemaHypotheses.sql"
    );

    await initializer.InitializeAsync(schemaPath);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();
