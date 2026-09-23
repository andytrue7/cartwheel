using System.Collections.ObjectModel;
using Cartwheel.Domain.Exceptions;

namespace Cartwheel.Domain;

public class Cart
{
    private readonly List<CartItem> _items = [];
    private readonly ReadOnlyCollection<CartItem> _itemsReadOnly;

    public Guid Id { get; }
    public IReadOnlyList<CartItem> Items => _itemsReadOnly;

    public Cart()
    {
        Id = Guid.NewGuid();
        _itemsReadOnly = _items.AsReadOnly();
    }

    public void AddItem(Product product, int quantity = 1)
    {
        if (product == null)
        {
            throw new ArgumentNullException(nameof(product));
        }

        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));
        }

        CartItem? existingItem = _items.FirstOrDefault(x => x.Product.Id == product.Id);
        int requestedTotal = quantity + (existingItem?.Quantity ?? 0);

        if (requestedTotal > product.StockQuantity)
        {
            throw new InsufficientStockException(product.Name, requestedTotal, product.StockQuantity);
        }

        if (existingItem != null)
        {
            existingItem.AddQuantity(quantity);
        }
        else
        {
            CartItem cartItem = new CartItem(product, quantity);
            _items.Add(cartItem);
        }
    }

    public void UpdateQuantity(Guid productId, int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));
        }

        CartItem? item = _items.FirstOrDefault(x => x.Product.Id == productId);

        if (item == null)
        {
            throw new InvalidOperationException($"Product '{productId}' is not in the cart.");
        }

        if (quantity > item.Product.StockQuantity)
        {
            throw new InsufficientStockException(item.Product.Name, quantity, item.Product.StockQuantity);
        }

        item.SetQuantity(quantity);
    }

    public void RemoveItem(Guid productId)
    {
        CartItem? cartItem = Items.FirstOrDefault(x => x.Product.Id == productId);

        if (cartItem == null)
        {
            throw new InvalidOperationException($"Product '{productId}' is not in the cart.");
        }
        
        _items.Remove(cartItem);
    }

    public void Clear()
    {
        _items.Clear();
    }

    public decimal GetTotalPrice()
    {
        return Items.Sum(x => x.LineTotal);
    }

    public int GetTotalUnits()
    { 
        return Items.Sum(x => x.Quantity);
    }
}