using BranchstormDomain.Infrastructure.Database;
using FeedbackApi.Containers;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDependencyInjectionContainers();
builder.Services.AddHttpClient("IdeaApi");

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
        "schemaFeedbacks.sql"
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

