using BranchstormDomain.Infrastructure.Database;
using Scalar.AspNetCore;
using UserApi.Containers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDependencyInjectionContainers();

builder.Services.AddOpenApi(options =>
{
    options.AddScalarTransformers();
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider
        .GetRequiredService<DatabaseInitializer>();

    var schemaPath = Path.Combine(
        app.Environment.ContentRootPath,
        "Database",
        "schemaUsers.sql"
    );

    await initializer.InitializeAsync(schemaPath);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();