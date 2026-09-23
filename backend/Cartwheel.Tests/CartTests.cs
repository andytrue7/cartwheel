using Cartwheel.Domain;
using Cartwheel.Domain.Exceptions;

namespace Cartwheel.Tests;

public class CartTests
{
    private readonly Category _category = new("Electronics");

    // Builds a valid product. Each test overrides only the value it cares about.
    private Product CreateProduct(string name = "Laptop", decimal price = 100m, int stock = 10)
    {
        return new Product(name, price, stock, _category);
    }

    // ---------- New cart ----------

    [Fact]
    public void NewCart_IsEmptyWithZeroTotals()
    {
        var cart = new Cart();

        Assert.Empty(cart.Items);
        Assert.Equal(0m, cart.GetTotalPrice());
        Assert.Equal(0, cart.GetTotalUnits());
    }

    // ---------- AddItem ----------

    [Fact]
    public void AddItem_NewProduct_CreatesOneLine()
    {
        // Arrange
        var cart = new Cart();
        var laptop = CreateProduct();

        // Act
        cart.AddItem(laptop, 2);

        // Assert
        var line = Assert.Single(cart.Items);
        Assert.Same(laptop, line.Product);
        Assert.Equal(2, line.Quantity);
    }

    [Fact]
    public void AddItem_WithoutQuantity_AddsOneUnit()
    {
        var cart = new Cart();

        cart.AddItem(CreateProduct());

        Assert.Equal(1, Assert.Single(cart.Items).Quantity);
    }

    [Fact]
    public void AddItem_SameProductTwice_MergesIntoOneLine()
    {
        var cart = new Cart();
        var laptop = CreateProduct(stock: 10);

        cart.AddItem(laptop, 2);
        cart.AddItem(laptop, 3);

        Assert.Equal(5, Assert.Single(cart.Items).Quantity);
    }

    [Fact]
    public void AddItem_DifferentProducts_CreatesSeparateLines()
    {
        var cart = new Cart();

        cart.AddItem(CreateProduct(name: "Laptop"));
        cart.AddItem(CreateProduct(name: "Phone"));

        Assert.Equal(2, cart.Items.Count);
    }

    [Fact]
    public void AddItem_MoreThanStock_ThrowsAndLeavesCartEmpty()
    {
        var cart = new Cart();
        var laptop = CreateProduct(stock: 3);

        Assert.Throws<InsufficientStockException>(() => cart.AddItem(laptop, 4));
        Assert.Empty(cart.Items);
    }

    [Fact]
    public void AddItem_MergeExceedingStock_ThrowsAndKeepsOriginalQuantity()
    {
        // Arrange
        var cart = new Cart();
        var laptop = CreateProduct(stock: 5);
        cart.AddItem(laptop, 4);

        // Act
        var exception = Assert.Throws<InsufficientStockException>(() => cart.AddItem(laptop, 2));

        // Assert: the check uses the combined quantity, and the line is untouched.
        Assert.Equal(6, exception.RequestedQuantity);
        Assert.Equal(5, exception.AvailableQuantity);
        Assert.Equal(4, Assert.Single(cart.Items).Quantity);
    }

    [Fact]
    public void AddItem_OutOfStockProduct_ThrowsInsufficientStock()
    {
        var cart = new Cart();
        var soldOut = CreateProduct(stock: 0);

        Assert.Throws<InsufficientStockException>(() => cart.AddItem(soldOut));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void AddItem_WithNonPositiveQuantity_ThrowsArgumentException(int quantity)
    {
        var cart = new Cart();

        Assert.Throws<ArgumentException>(() => cart.AddItem(CreateProduct(), quantity));
        Assert.Empty(cart.Items);
    }

    [Fact]
    public void AddItem_NullProduct_ThrowsArgumentNullException()
    {
        var cart = new Cart();

        Assert.Throws<ArgumentNullException>(() => cart.AddItem(null!));
    }

    // ---------- Totals ----------

    [Fact]
    public void GetTotalPrice_WithSeveralProducts_SumsLineTotalsExactly()
    {
        // Arrange: prices with cents, which double could not add up exactly.
        var cart = new Cart();
        cart.AddItem(CreateProduct(name: "Cable", price: 10.10m), 3);   // 30.30
        cart.AddItem(CreateProduct(name: "Adapter", price: 5.25m), 2);  // 10.50

        // Act
        var total = cart.GetTotalPrice();

        // Assert
        Assert.Equal(40.80m, total);
    }

    [Fact]
    public void GetTotalUnits_WithSeveralLines_SumsQuantities()
    {
        var cart = new Cart();
        cart.AddItem(CreateProduct(name: "Laptop"), 2);
        cart.AddItem(CreateProduct(name: "Phone"), 3);

        Assert.Equal(5, cart.GetTotalUnits());
    }

    // ---------- UpdateQuantity ----------

    [Fact]
    public void UpdateQuantity_WithinStock_SetsNewQuantity()
    {
        var cart = new Cart();
        var laptop = CreateProduct(stock: 10);
        cart.AddItem(laptop, 2);

        cart.UpdateQuantity(laptop.Id, 7);

        Assert.Equal(7, Assert.Single(cart.Items).Quantity);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-2)]
    public void UpdateQuantity_WithNonPositiveQuantity_ThrowsAndKeepsQuantity(int quantity)
    {
        var cart = new Cart();
        var laptop = CreateProduct();
        cart.AddItem(laptop, 2);

        Assert.Throws<ArgumentException>(() => cart.UpdateQuantity(laptop.Id, quantity));
        Assert.Equal(2, Assert.Single(cart.Items).Quantity);
    }

    [Fact]
    public void UpdateQuantity_MoreThanStock_ThrowsAndKeepsQuantity()
    {
        var cart = new Cart();
        var laptop = CreateProduct(stock: 5);
        cart.AddItem(laptop, 2);

        Assert.Throws<InsufficientStockException>(() => cart.UpdateQuantity(laptop.Id, 6));
        Assert.Equal(2, Assert.Single(cart.Items).Quantity);
    }

    [Fact]
    public void UpdateQuantity_ProductNotInCart_ThrowsInvalidOperationException()
    {
        var cart = new Cart();

        Assert.Throws<InvalidOperationException>(() => cart.UpdateQuantity(Guid.NewGuid(), 1));
    }

    // ---------- RemoveItem and Clear ----------

    [Fact]
    public void RemoveItem_ExistingProduct_RemovesOnlyThatLine()
    {
        var cart = new Cart();
        var laptop = CreateProduct(name: "Laptop");
        var phone = CreateProduct(name: "Phone");
        cart.AddItem(laptop);
        cart.AddItem(phone);

        cart.RemoveItem(laptop.Id);

        Assert.Same(phone, Assert.Single(cart.Items).Product);
    }

    [Fact]
    public void RemoveItem_ProductNotInCart_ThrowsInvalidOperationException()
    {
        var cart = new Cart();

        Assert.Throws<InvalidOperationException>(() => cart.RemoveItem(Guid.NewGuid()));
    }

    [Fact]
    public void Clear_WithItems_EmptiesCart()
    {
        var cart = new Cart();
        cart.AddItem(CreateProduct(name: "Laptop"));
        cart.AddItem(CreateProduct(name: "Phone"));

        cart.Clear();

        Assert.Empty(cart.Items);
        Assert.Equal(0m, cart.GetTotalPrice());
    }

    // ---------- Encapsulation ----------

    [Fact]
    public void Items_CannotBeModifiedFromOutside_EvenAfterCasting()
    {
        var cart = new Cart();
        cart.AddItem(CreateProduct());

        // The property is a read-only wrapper, not the cart's real list...
        Assert.IsNotType<List<CartItem>>(cart.Items);

        // ...so even after casting to a writable interface, changes are rejected.
        var asList = (IList<CartItem>)cart.Items;
        Assert.Throws<NotSupportedException>(() => asList.Clear());
        Assert.Single(cart.Items);
    }
}
