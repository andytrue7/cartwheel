using Cartwheel.Api.ErrorHandling;
using Cartwheel.Domain.Repositories;
using Cartwheel.Domain.Services;
using Cartwheel.Infrastructure.Repositories;
using Cartwheel.Infrastructure.Seeding;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddRouting(options => options.LowercaseUrls = true);

// ProblemDetails for every error response, plus mapping of domain exceptions to 4xx.
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<DomainExceptionHandler>();

// One seed, so products and the category repository share the same Category objects.
var seed = SeedCatalog.Create();
builder.Services.AddSingleton<ICategoryRepository>(new InMemoryCategoryRepository(seed.Categories));
builder.Services.AddSingleton<IProductRepository>(new InMemoryProductRepository(seed.Products));
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

app.UseAuthorization();

app.MapControllers();

app.Run();
