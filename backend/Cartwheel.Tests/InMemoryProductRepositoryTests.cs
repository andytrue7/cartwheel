using Cartwheel.Domain;
using Cartwheel.Infrastructure.Repositories;

namespace Cartwheel.Tests;

public class InMemoryProductRepositoryTests
{
    private readonly Category _category = new("Electronics");

    private Product CreateProduct(string name = "Laptop")
    {
        return new Product(name, 100m, 10, _category);
    }

    [Fact]
    public async Task GetByIdAsync_AfterAdd_ReturnsSameProduct()
    {
        var repository = new InMemoryProductRepository();
        var laptop = CreateProduct();

        await repository.AddAsync(laptop);
        var found = await repository.GetByIdAsync(laptop.Id);

        Assert.Same(laptop, found);
    }

    [Fact]
    public async Task GetByIdAsync_UnknownId_ReturnsNull()
    {
        var repository = new InMemoryProductRepository();

        var found = await repository.GetByIdAsync(Guid.NewGuid());

        Assert.Null(found);
    }

    [Fact]
    public async Task GetAllAsync_WithInitialProducts_ReturnsAllOfThem()
    {
        var laptop = CreateProduct("Laptop");
        var phone = CreateProduct("Phone");
        var repository = new InMemoryProductRepository([laptop, phone]);

        var all = await repository.GetAllAsync();

        Assert.Equal(2, all.Count);
        Assert.Contains(laptop, all);
        Assert.Contains(phone, all);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsSnapshot_NotAffectedByLaterAdds()
    {
        var repository = new InMemoryProductRepository([CreateProduct("Laptop")]);

        var before = await repository.GetAllAsync();
        await repository.AddAsync(CreateProduct("Phone"));

        Assert.Single(before);
    }
    
    [Fact]
    public async Task GetAllAsync_ReturnsProductsSortedByName()
    {
        var repository = new InMemoryProductRepository(
            [CreateProduct("Phone"), CreateProduct("laptop"), CreateProduct("Camera")]);

        var all = await repository.GetAllAsync();

        Assert.Equal(["Camera", "laptop", "Phone"], all.Select(p => p.Name));
    }

    [Fact]
    public async Task AddAsync_SameProductTwice_ThrowsInvalidOperationException()
    {
        var laptop = CreateProduct();
        var repository = new InMemoryProductRepository([laptop]);

        await Assert.ThrowsAsync<InvalidOperationException>(() => repository.AddAsync(laptop));
    }

    [Fact]
    public async Task AddAsync_NullProduct_ThrowsArgumentNullException()
    {
        var repository = new InMemoryProductRepository();

        await Assert.ThrowsAsync<ArgumentNullException>(() => repository.AddAsync(null!));
    }

    [Fact]
    public async Task RemoveAsync_ExistingProduct_RemovesItAndReturnsTrue()
    {
        var laptop = CreateProduct();
        var repository = new InMemoryProductRepository([laptop]);

        var removed = await repository.RemoveAsync(laptop.Id);

        Assert.True(removed);
        Assert.Null(await repository.GetByIdAsync(laptop.Id));
    }

    [Fact]
    public async Task RemoveAsync_UnknownId_ReturnsFalse()
    {
        var repository = new InMemoryProductRepository();

        var removed = await repository.RemoveAsync(Guid.NewGuid());

        Assert.False(removed);
    }

    [Fact]
    public async Task GetAllAsync_WhenCancelled_ThrowsOperationCanceledException()
    {
        var repository = new InMemoryProductRepository();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => repository.GetAllAsync(cancellation.Token));
    }
}
