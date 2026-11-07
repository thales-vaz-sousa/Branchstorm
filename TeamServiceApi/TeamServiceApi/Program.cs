using BranchstormDomain.Infrastructure.Database;
using TeamApi.Containers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDependencyInjectionContainers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider
        .GetRequiredService<DatabaseInitializer>();

    var schemaPath = Path.Combine(
        app.Environment.ContentRootPath,
        "Database",
        "schemaTeams.sql"
    );

    await initializer.InitializeAsync(schemaPath);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
