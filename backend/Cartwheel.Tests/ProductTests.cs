using Cartwheel.Domain;
using Cartwheel.Domain.Exceptions;

namespace Cartwheel.Tests;

public class ProductTests
{
    private readonly Category _category = new("Laptops");

    // Builds a valid product. Each test overrides only the value it cares about.
    private Product CreateProduct(
        string name = "MacBook Air",
        decimal price = 1000m,
        int stock = 10,
        string? description = "A laptop")
    {
        return new Product(name, price, stock, _category, description);
    }

    // ---------- Constructor ----------

    [Fact]
    public void Constructor_WithValidData_KeepsAllValues()
    {
        // Act
        var product = new Product("MacBook Air", 1099.99m, 7, _category, "13-inch laptop");

        // Assert
        Assert.NotEqual(Guid.Empty, product.Id);
        Assert.Equal("MacBook Air", product.Name);
        Assert.Equal(1099.99m, product.Price);
        Assert.Equal(7, product.StockQuantity);
        Assert.Same(_category, product.Category);
        Assert.Equal("13-inch laptop", product.Description);
    }

    [Fact]
    public void Constructor_WithoutDescription_LeavesDescriptionNull()
    {
        var product = new Product("MacBook Air", 1000m, 1, _category);

        Assert.Null(product.Description);
    }

    [Fact]
    public void Constructor_WithZeroStock_IsAllowed()
    {
        var product = CreateProduct(stock: 0);

        Assert.Equal(0, product.StockQuantity);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-1000)]
    public void Constructor_WithNonPositivePrice_ThrowsArgumentException(int price)
    {
        Assert.Throws<ArgumentException>(() => CreateProduct(price: price));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void Constructor_WithBlankName_ThrowsArgumentException(string name)
    {
        Assert.Throws<ArgumentException>(() => CreateProduct(name: name));
    }

    [Fact]
    public void Constructor_WithNegativeStock_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => CreateProduct(stock: -1));
    }

    [Fact]
    public void Constructor_WithNullCategory_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new Product("MacBook Air", 1000m, 1, null!));
    }

    [Fact]
    public void SetPrice_WithPositivePrice_UpdatesPrice()
    {
        var product = CreateProduct(price: 1000m);

        product.SetPrice(899.50m);

        Assert.Equal(899.50m, product.Price);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void SetPrice_WithNonPositivePrice_ThrowsAndKeepsOldPrice(int newPrice)
    {
        var product = CreateProduct(price: 1000m);

        Assert.Throws<ArgumentException>(() => product.SetPrice(newPrice));
        Assert.Equal(1000m, product.Price);
    }

    [Fact]
    public void DecreaseStock_WithinStock_ReducesStock()
    {
        var product = CreateProduct(stock: 10);

        product.DecreaseStock(3);

        Assert.Equal(7, product.StockQuantity);
    }

    [Fact]
    public void DecreaseStock_ByExactlyAllStock_LeavesZero()
    {
        var product = CreateProduct(stock: 5);

        product.DecreaseStock(5);

        Assert.Equal(0, product.StockQuantity);
    }

    [Fact]
    public void DecreaseStock_MoreThanAvailable_ThrowsInsufficientStockAndKeepsStock()
    {
        // Arrange
        var product = CreateProduct(name: "MacBook Air", stock: 2);

        // Act
        var exception = Assert.Throws<InsufficientStockException>(() => product.DecreaseStock(5));

        // Assert
        Assert.Equal("MacBook Air", exception.ProductName);
        Assert.Equal(5, exception.RequestedQuantity);
        Assert.Equal(2, exception.AvailableQuantity);
        Assert.Equal(2, product.StockQuantity);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public void DecreaseStock_WithNonPositiveQuantity_ThrowsArgumentException(int quantity)
    {
        var product = CreateProduct(stock: 10);

        Assert.Throws<ArgumentException>(() => product.DecreaseStock(quantity));
        Assert.Equal(10, product.StockQuantity);
    }

    [Fact]
    public void IncreaseStock_WithPositiveQuantity_AddsToStock()
    {
        var product = CreateProduct(stock: 10);

        product.IncreaseStock(5);

        Assert.Equal(15, product.StockQuantity);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public void IncreaseStock_WithNonPositiveQuantity_ThrowsArgumentException(int quantity)
    {
        var product = CreateProduct(stock: 10);

        Assert.Throws<ArgumentException>(() => product.IncreaseStock(quantity));
        Assert.Equal(10, product.StockQuantity);
    }
}
