using Cartwheel.Domain;
using Cartwheel.Infrastructure.Repositories;

namespace Cartwheel.Tests;

public class InMemoryCategoryRepositoryTests
{
    [Fact]
    public async Task GetByIdAsync_KnownId_ReturnsSameCategory()
    {
        var laptops = new Category("Laptops");
        var repository = new InMemoryCategoryRepository([laptops]);

        var found = await repository.GetByIdAsync(laptops.Id);

        Assert.Same(laptops, found);
    }

    [Fact]
    public async Task GetByIdAsync_UnknownId_ReturnsNull()
    {
        var repository = new InMemoryCategoryRepository([new Category("Laptops")]);

        var found = await repository.GetByIdAsync(Guid.NewGuid());

        Assert.Null(found);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsCategoriesSortedByName()
    {
        var repository = new InMemoryCategoryRepository(
            [new Category("TVs"), new Category("headphones"), new Category("Laptops")]);

        var all = await repository.GetAllAsync();

        Assert.Equal(["headphones", "Laptops", "TVs"], all.Select(c => c.Name));
    }

    [Fact]
    public void Constructor_WithSameCategoryTwice_ThrowsInvalidOperationException()
    {
        var laptops = new Category("Laptops");

        Assert.Throws<InvalidOperationException>(() => new InMemoryCategoryRepository([laptops, laptops]));
    }
}
