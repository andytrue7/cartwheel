using Cartwheel.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Cartwheel.Infrastructure.Seeding;

/// <summary>
/// Fills an empty database with the demo catalog. The ids are random, so HasData can't be used;
/// UseSeeding / UseAsyncSeeding run on Migrate() and on "dotnet ef database update".
/// The "any categories?" check makes it safe to run every time.
/// </summary>
public static class CartwheelSeeder
{
    // EF's tools call the sync version, the application calls the async one.
    public static void Seed(DbContext context, bool _)
    {
        var db = (CartwheelDbContext)context;
        if (db.Categories.Any())
        {
            return;
        }

        var seed = SeedCatalog.Create();
        db.Categories.AddRange(seed.Categories);
        db.Products.AddRange(seed.Products);
        db.SaveChanges();
    }

    public static async Task SeedAsync(DbContext context, bool _, CancellationToken ct)
    {
        var db = (CartwheelDbContext)context;
        if (await db.Categories.AnyAsync(ct))
        {
            return;
        }

        var seed = SeedCatalog.Create();
        db.Categories.AddRange(seed.Categories);
        db.Products.AddRange(seed.Products);
        await db.SaveChangesAsync(ct);
    }
}
