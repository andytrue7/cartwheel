using Cartwheel.Domain;
using Cartwheel.Domain.Repositories;
using Cartwheel.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Cartwheel.Infrastructure.Repositories;

public class EfProductRepository(CartwheelDbContext context) : IProductRepository
{
    public async Task<IReadOnlyList<Product>> GetAllAsync(ProductFilter? filter = null, CancellationToken ct = default)
    {
        filter ??= ProductFilter.None;

        return await Sort(Filter(filter).Include(p => p.Category), filter.SortOrder).ToListAsync(ct);
    }

    public async Task<PagedResult<Product>> GetPageAsync(
        ProductFilter? filter, PageRequest page, CancellationToken ct = default)
    {
        filter ??= ProductFilter.None;
        var matching = Filter(filter);

        // Counting needs neither the join nor the sort, so the COUNT query has neither.
        var totalCount = await matching.CountAsync(ct);

        var items = await Sort(matching.Include(p => p.Category), filter.SortOrder)
            .Skip(page.Skip)
            .Take(page.PageSize)
            .ToListAsync(ct);

        return new PagedResult<Product>(items, totalCount, page);
    }

    // Tracked: the update flow changes this instance and then calls UpdateAsync.
    public Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task AddAsync(Product product, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(product);

        context.Products.Add(product);
        await context.SaveChangesAsync(ct);
    }

    public async Task<bool> UpdateAsync(Product product, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(product);

        try
        {
            await context.SaveChangesAsync(ct);
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            // The row was deleted after it was loaded: the UPDATE affected 0 rows.
            return false;
        }
    }

    // One DELETE statement, no SELECT first. It returns how many rows were deleted.
    public async Task<bool> RemoveAsync(Guid id, CancellationToken ct = default) =>
        await context.Products.Where(p => p.Id == id).ExecuteDeleteAsync(ct) > 0;

    // The one place the filtering rules live: GetAllAsync and GetPageAsync both build on it.
    // Nothing runs against the database here: every call only adds to the query.
    private IQueryable<Product> Filter(ProductFilter filter)
    {
        IQueryable<Product> query = context.Products.AsNoTracking();

        var search = filter.Search?.Trim();
        if (!string.IsNullOrEmpty(search))
        {
            // No StringComparison: EF can't translate it. The database collation is case-insensitive.
            query = query.Where(p => p.Name.Contains(search));
        }

        if (filter.CategoryId is { } categoryId)
        {
            query = query.Where(p => p.Category.Id == categoryId);
        }

        if (filter.InStockOnly)
        {
            query = query.Where(p => p.StockQuantity > 0);
        }

        return query;
    }

    // Ties always fall back to name, then Id, so the order is the same on every call.
    private static IOrderedQueryable<Product> Sort(IQueryable<Product> products, ProductSortOrder sortOrder)
    {
        var sorted = sortOrder switch
        {
            ProductSortOrder.Name => products.OrderBy(p => p.Name),
            ProductSortOrder.PriceAscending => products.OrderBy(p => p.Price).ThenBy(p => p.Name),
            ProductSortOrder.PriceDescending => products.OrderByDescending(p => p.Price).ThenBy(p => p.Name),
            _ => throw new ArgumentOutOfRangeException(nameof(sortOrder), sortOrder, "Unknown sort order.")
        };

        return sorted.ThenBy(p => p.Id);
    }
}
