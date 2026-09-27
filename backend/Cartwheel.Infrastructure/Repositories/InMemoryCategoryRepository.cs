using Cartwheel.Domain;
using Cartwheel.Domain.Repositories;

namespace Cartwheel.Infrastructure.Repositories;

/// <summary>
/// Read-only category store. The dictionary is filled once in the constructor and never
/// written again, so many requests can read it at the same time safely.
/// </summary>
public class InMemoryCategoryRepository : ICategoryRepository
{
    private readonly Dictionary<Guid, Category> _categories = new();

    public InMemoryCategoryRepository(IEnumerable<Category>? initialCategories = null)
    {
        foreach (var category in initialCategories ?? [])
        {
            ArgumentNullException.ThrowIfNull(category);

            if (!_categories.TryAdd(category.Id, category))
            {
                throw new InvalidOperationException($"Category '{category.Id}' already exists.");
            }
        }
    }

    public Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        _categories.TryGetValue(id, out var category);
        return Task.FromResult(category);
    }

    public Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        var ordered = _categories.Values
            .OrderBy(c => c.Name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(c => c.Id)
            .ToList();

        return Task.FromResult<IReadOnlyList<Category>>(ordered);
    }
}
