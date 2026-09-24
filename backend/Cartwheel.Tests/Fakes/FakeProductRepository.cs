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

    public Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct = default)
    {
        return Task.FromResult<IReadOnlyList<Product>>(_products.ToList());
    }

    public Task AddAsync(Product product, CancellationToken ct = default)
    {
        _products.Add(product);
        return Task.CompletedTask;
    }

    public Task<bool> RemoveAsync(Guid id, CancellationToken ct = default)
    {
        return Task.FromResult(_products.RemoveAll(p => p.Id == id) > 0);
    }
}
