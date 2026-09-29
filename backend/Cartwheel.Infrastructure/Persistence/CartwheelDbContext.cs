using Cartwheel.Domain;
using Microsoft.EntityFrameworkCore;

namespace Cartwheel.Infrastructure.Persistence;

public class CartwheelDbContext(DbContextOptions<CartwheelDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();

    // The mapping lives in the IEntityTypeConfiguration classes next to this file,
    // so the domain classes carry no database attributes.
    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CartwheelDbContext).Assembly);
}
