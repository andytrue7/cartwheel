using Cartwheel.Domain.Repositories;
using Cartwheel.Domain.Services;
using Cartwheel.Infrastructure.Repositories;
using Cartwheel.Infrastructure.Seeding;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddRouting(options => options.LowercaseUrls = true);

builder.Services.AddSingleton<IProductRepository>(
    _ => new InMemoryProductRepository(SeedCatalog.CreateProducts()));
builder.Services.AddScoped<CartService>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
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
