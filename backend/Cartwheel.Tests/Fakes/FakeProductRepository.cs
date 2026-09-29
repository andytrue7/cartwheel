using Cartwheel.Domain;
using Cartwheel.Domain.Repositories;

namespace Cartwheel.Tests.Fakes;

/// <summary>
/// A minimal, test-owned repository. Tests control exactly which products exist,
/// and it records lookups so a test can check how the service used it.
/// </summary>
public class FakeProductRepository(params Product[] products) : IProductRepository
{
    private readonly List<Product> _products = [.. products];

    public List<Guid> RequestedIds { get; } = [];

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        RequestedIds.Add(id);
        return Task.FromResult(_products.FirstOrDefault(p => p.Id == id));
    }

    /// <summary>
    /// Honours the search, category and in-stock criteria but ignores sorting:
    /// products come back in the order they were given.
    /// </summary>
    public Task<IReadOnlyList<Product>> GetAllAsync(ProductFilter? filter = null, CancellationToken ct = default)
    {
        filter ??= ProductFilter.None;

        var matching = _products
            .Where(p => string.IsNullOrWhiteSpace(filter.Search)
                        || p.Name.Contains(filter.Search.Trim(), StringComparison.OrdinalIgnoreCase))
            .Where(p => filter.CategoryId is null || p.Category.Id == filter.CategoryId)
            .Where(p => !filter.InStockOnly || p.StockQuantity > 0)
            .ToList();

        return Task.FromResult<IReadOnlyList<Product>>(matching);
    }

    public Task AddAsync(Product product, CancellationToken ct = default)
    {
        _products.Add(product);
        return Task.CompletedTask;
    }

    public Task<bool> UpdateAsync(Product product, CancellationToken ct = default)
    {
        var index = _products.FindIndex(p => p.Id == product.Id);
        if (index < 0)
        {
            return Task.FromResult(false);
        }

        _products[index] = product;
        return Task.FromResult(true);
    }
    
    public Task<bool> RemoveAsync(Guid id, CancellationToken ct = default)
    {
        return Task.FromResult(_products.RemoveAll(p => p.Id == id) > 0);
    }
}
