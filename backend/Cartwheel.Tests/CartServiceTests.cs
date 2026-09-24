using Cartwheel.Domain;
using Cartwheel.Domain.Exceptions;
using Cartwheel.Domain.Services;
using Cartwheel.Tests.Fakes;

namespace Cartwheel.Tests;

// These tests use the fake repository, not the in-memory one.
// A bug in the real repository can then never make a cart service test fail.
public class CartServiceTests
{
    private readonly Category _category = new("Electronics");

    private Product CreateProduct(string name = "Laptop", decimal price = 100m, int stock = 10)
    {
        return new Product(name, price, stock, _category);
    }

    [Fact]
    public async Task AddToCartAsync_ExistingProduct_AddsItToCart()
    {
        // Arrange
        var laptop = CreateProduct();
        var repository = new FakeProductRepository(laptop);
        var service = new CartService(repository);
        var cart = new Cart();

        // Act
        await service.AddToCartAsync(cart, laptop.Id, 2);

        // Assert
        var line = Assert.Single(cart.Items);
        Assert.Same(laptop, line.Product);
        Assert.Equal(2, line.Quantity);
        Assert.Equal([laptop.Id], repository.RequestedIds);
    }

    [Fact]
    public async Task AddToCartAsync_MissingProduct_ThrowsAndLeavesCartEmpty()
    {
        var service = new CartService(new FakeProductRepository());
        var cart = new Cart();
        var missingId = Guid.NewGuid();

        var exception = await Assert.ThrowsAsync<ProductNotFoundException>(
            () => service.AddToCartAsync(cart, missingId));

        Assert.Equal(missingId, exception.ProductId);
        Assert.Empty(cart.Items);
    }

    [Fact]
    public async Task AddToCartAsync_MoreThanStock_ThrowsInsufficientStock()
    {
        var laptop = CreateProduct(stock: 1);
        var service = new CartService(new FakeProductRepository(laptop));
        var cart = new Cart();

        await Assert.ThrowsAsync<InsufficientStockException>(
            () => service.AddToCartAsync(cart, laptop.Id, 2));
        Assert.Empty(cart.Items);
    }

    [Fact]
    public async Task AddToCartAsync_NullCart_ThrowsArgumentNullException()
    {
        var service = new CartService(new FakeProductRepository());

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => service.AddToCartAsync(null!, Guid.NewGuid()));
    }
}
