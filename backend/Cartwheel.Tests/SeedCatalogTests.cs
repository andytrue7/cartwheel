using Cartwheel.Infrastructure.Seeding;

namespace Cartwheel.Tests;

public class SeedCatalogTests
{
    [Fact]
    public void Create_EveryProductUsesOneOfTheSeededCategoryObjects()
    {
        var seed = SeedCatalog.Create();

        // Category is a class, so Contains compares references: the product must hold
        // the very same object that the category repository will return.
        Assert.All(seed.Products, product => Assert.Contains(product.Category, seed.Categories));
    }

    [Fact]
    public void Create_ReturnsTheExpectedCatalogSize()
    {
        var seed = SeedCatalog.Create();

        Assert.Equal(4, seed.Categories.Count);
        Assert.Equal(12, seed.Products.Count);
    }
}
