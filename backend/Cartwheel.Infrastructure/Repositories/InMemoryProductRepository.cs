using System.Collections.Concurrent;
using Cartwheel.Domain;
using Cartwheel.Domain.Repositories;

namespace Cartwheel.Infrastructure.Repositories;

public class InMemoryProductRepository : IProductRepository
{
    private readonly ConcurrentDictionary<Guid, Product> _products = new();

    public InMemoryProductRepository(IEnumerable<Product>? initialProducts = null)
    {
        foreach (var product in initialProducts ?? [])
        {
            Add(product);
        }
    }

    public Task<IReadOnlyList<Product>> GetAllAsync(ProductFilter? filter = null, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        filter ??= ProductFilter.None;

        var ordered = Sort(Filter(filter), filter.SortOrder).ToList();

        return Task.FromResult<IReadOnlyList<Product>>(ordered);
    }

    public Task<PagedResult<Product>> GetPageAsync(
        ProductFilter? filter, PageRequest page, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        filter ??= ProductFilter.None;

        var matching = Filter(filter).ToList();
        var items = Sort(matching, filter.SortOrder).Skip(page.Skip).Take(page.PageSize).ToList();

        return Task.FromResult(new PagedResult<Product>(items, matching.Count, page));
    }

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        _products.TryGetValue(id, out var product);
        return Task.FromResult(product);
    }

    public Task AddAsync(Product product, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        Add(product);
        return Task.CompletedTask;
    }

    public Task<bool> UpdateAsync(Product product, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(product);

        if (!_products.TryGetValue(product.Id, out var current))
        {
            return Task.FromResult(false);
        }
        
        return Task.FromResult(_products.TryUpdate(product.Id, product, current));
    }
    
    public Task<bool> RemoveAsync(Guid id, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        return Task.FromResult(_products.TryRemove(id, out _));
    }

    // The one place the in-memory filtering rules live: both list methods use it.
    private IEnumerable<Product> Filter(ProductFilter filter)
    {
        IEnumerable<Product> matching = _products.Values;

        var search = filter.Search?.Trim();
        if (!string.IsNullOrEmpty(search))
        {
            matching = matching.Where(p => p.Name.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        if (filter.CategoryId is { } categoryId)
        {
            matching = matching.Where(p => p.Category.Id == categoryId);
        }

        if (filter.InStockOnly)
        {
            matching = matching.Where(p => p.StockQuantity > 0);
        }

        return matching;
    }

    // Ties always fall back to name, then Id, so the order is the same on every call.
    private static IEnumerable<Product> Sort(IEnumerable<Product> products, ProductSortOrder sortOrder)
    {
        var sorted = sortOrder switch
        {
            ProductSortOrder.Name => products.OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase),
            ProductSortOrder.PriceAscending => products.OrderBy(p => p.Price)
                .ThenBy(p => p.Name, StringComparer.OrdinalIgnoreCase),
            ProductSortOrder.PriceDescending => products.OrderByDescending(p => p.Price)
                .ThenBy(p => p.Name, StringComparer.OrdinalIgnoreCase),
            _ => throw new ArgumentOutOfRangeException(nameof(sortOrder), sortOrder, "Unknown sort order.")
        };

        return sorted.ThenBy(p => p.Id);
    }

    private void Add(Product product)
    {
        ArgumentNullException.ThrowIfNull(product);

        if (!_products.TryAdd(product.Id, product))
        {
            throw new InvalidOperationException($"Product '{product.Id}' already exists.");
        }
    }
}
