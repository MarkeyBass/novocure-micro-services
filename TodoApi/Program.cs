using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;
using TodoApi.Consumers;
using TodoApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// 1. Register CORS policy (add this near the top, with your other services)
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowAngularDev",
        policy =>
        {
            policy
                .WithOrigins(
                    "http://localhost:4200",
                    "http://localhost:5173",
                    "https://localhost:4200",
                    "https://localhost:5173"
                )
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
    );
});

// RabbitMQ — bind settings and register the background consumer.
// The consumer starts automatically when the app starts (IHostedService contract).
builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection("RabbitMQ"));
builder.Services.AddHostedService<HousingApplicationCreatedConsumer>();

builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Added db context
// builder.Services.AddDbContext<TodoContext>(opt => opt.UseInMemoryDatabase("TodoList"));
builder.Services.AddDbContext<TodoContext>(opt =>
    opt.UseSqlServer(
        builder.Configuration.GetConnectionString("TodoContext"),
        // In containers, SQL Server can be reachable but not fully ready yet.
        // This retries transient connection/startup failures and pairs well with
        // `Database.Migrate()` below, but it is not a replacement for migrations.
        sqlOptions => sqlOptions.EnableRetryOnFailure()
    )
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    // Added swagger
    app.UseSwaggerUi(options =>
    {
        options.DocumentPath = "/openapi/v1.json";
    });
}

var isRunningInContainer = string.Equals(
    Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"),
    "true",
    StringComparison.OrdinalIgnoreCase
);

// Keep HTTPS redirection for host development, but skip it for the local
// containerized path where TodoApi is intentionally exposed over HTTP only.
if (!isRunningInContainer)
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();

// 2. Apply CORS policy
app.UseCors("AllowAngularDev");

app.MapControllers();

// Apply any pending EF Core migrations on startup.
// This creates the database and schema automatically when running in a container,
// so there's no need to run "dotnet ef database update" manually.
// Startup blocks here until migrations complete or fail, so the API will not
// serve requests before the schema is ready.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TodoContext>();
    db.Database.Migrate();
}

app.Run();
