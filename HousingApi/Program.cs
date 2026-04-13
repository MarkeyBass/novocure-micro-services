using HousingApi.Models;
using HousingApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// ================================
// Add MongoDB configuration for the housing service.
builder.Services.Configure<HousingDatabaseSettings>(
    builder.Configuration.GetSection("HousingDatabase")
);

// mongo service - makes queries to the database
builder.Services.AddSingleton<HousingLocationsService>();

// Use the default ASP.NET Core camelCase JSON naming so the Angular client can
// consume the API shape without extra mapping.
// Hence, we removed this line: .AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null);
builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    // Swagger UI
    app.UseSwaggerUi(options =>
    {
        options.DocumentPath = "/openapi/v1.json";
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
