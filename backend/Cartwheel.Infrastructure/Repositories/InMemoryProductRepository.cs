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

    public Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        var ordered = _products.Values
            .OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(p => p.Id)
            .ToList();
        
        return Task.FromResult<IReadOnlyList<Product>>(ordered);
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

    public Task<bool> RemoveAsync(Guid id, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        return Task.FromResult(_products.TryRemove(id, out _));
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
