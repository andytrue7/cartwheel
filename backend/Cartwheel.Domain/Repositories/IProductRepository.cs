namespace Cartwheel.Domain.Repositories;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Returns the products matching the filter; a null filter returns all of them, sorted by name.</summary>
    Task<IReadOnlyList<Product>> GetAllAsync(ProductFilter? filter = null, CancellationToken ct = default);

    Task AddAsync(Product product, CancellationToken ct = default);

    /// <summary>
    /// Saves changes made to an existing product. Returns false if the product doesn't exist.
    /// In memory the object is already changed; a database implementation must write it.
    /// </summary>
    Task<bool> UpdateAsync(Product product, CancellationToken ct = default);

    Task<bool> RemoveAsync(Guid id, CancellationToken ct = default);
}
