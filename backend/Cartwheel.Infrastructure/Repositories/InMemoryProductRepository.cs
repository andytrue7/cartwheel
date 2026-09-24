using Cartwheel.Domain;
using Cartwheel.Domain.Repositories;

namespace Cartwheel.Infrastructure.Repositories;

public class InMemoryProductRepository : IProductRepository
{
    private readonly Dictionary<Guid, Product> _products = new();

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

        // ToList copies the values, so callers get a snapshot that later changes won't affect.
        return Task.FromResult<IReadOnlyList<Product>>(_products.Values.ToList());
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

        return Task.FromResult(_products.Remove(id));
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
