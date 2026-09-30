using Cartwheel.Api.ErrorHandling;
using Cartwheel.Domain.Repositories;
using Cartwheel.Domain.Services;
using Cartwheel.Infrastructure.Persistence;
using Cartwheel.Infrastructure.Repositories;
using Cartwheel.Infrastructure.Seeding;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

DotNetEnv.Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

var connectionString = new SqlConnectionStringBuilder(
    builder.Configuration.GetConnectionString("Cartwheel")
    ?? throw new InvalidOperationException("Connection string 'Cartwheel' is missing."))
{
    Password = builder.Configuration["MSSQL_SA_PASSWORD"]
               ?? throw new InvalidOperationException("MSSQL_SA_PASSWORD is not set. See .env.example.")
}.ConnectionString;

builder.Services.AddDbContext<CartwheelDbContext>(o => o
    .UseSqlServer(connectionString)
    .UseSeeding(CartwheelSeeder.Seed)
    .UseAsyncSeeding(CartwheelSeeder.SeedAsync));

// Add services to the container.

// Browsers block cross-origin calls unless the API says the origin is allowed. Origins come from
// configuration, so each environment lists its own.
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(o => o.AddPolicy("Frontend", policy => policy
    .WithOrigins(allowedOrigins)
    .AllowAnyHeader()
    .AllowAnyMethod()));

builder.Services.AddControllers();
builder.Services.AddRouting(options => options.LowercaseUrls = true);

// ProblemDetails for every error response, plus mapping of domain exceptions to 4xx.
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<DomainExceptionHandler>();

// Scoped, because DbContext is scoped: one context (and one set of tracked entities) per request.
builder.Services.AddScoped<ICategoryRepository, EfCategoryRepository>();
builder.Services.AddScoped<IProductRepository, EfProductRepository>();
builder.Services.AddScoped<CartService>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.

// First, so it wraps everything registered after it. Unhandled exceptions become a 500 ProblemDetails.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(o => o.SwaggerEndpoint("/openapi/v1.json", "Cartwheel v1"));
}
else
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseCors("Frontend");

app.UseAuthorization();

app.MapControllers();

app.Run();
