using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;
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

builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Added db context
// builder.Services.AddDbContext<TodoContext>(opt => opt.UseInMemoryDatabase("TodoList"));
builder.Services.AddDbContext<TodoContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("TodoContext"))
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

app.UseHttpsRedirection();

app.UseAuthorization();

// 2. Apply CORS policy
app.UseCors("AllowAngularDev");

app.MapControllers();

app.Run();
