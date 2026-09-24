using Cartwheel.Domain.Exceptions;
using Cartwheel.Domain.Repositories;

namespace Cartwheel.Domain.Services;

public class CartService(IProductRepository products)
{
    public async Task AddToCartAsync(Cart cart, Guid productId, int quantity = 1, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(cart);
        var product = await products.GetByIdAsync(productId, ct) ?? throw new ProductNotFoundException(productId);
        cart.AddItem(product, quantity);
    }
}