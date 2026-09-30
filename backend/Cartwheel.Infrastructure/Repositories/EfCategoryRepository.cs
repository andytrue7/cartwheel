using Cartwheel.Domain;
using Cartwheel.Domain.Repositories;
using Cartwheel.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Cartwheel.Infrastructure.Repositories;

public class EfCategoryRepository(CartwheelDbContext context) : ICategoryRepository
{
    // Tracked on purpose: Create and Update attach products to the category returned here.
    public Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        context.Categories.FirstOrDefaultAsync(c => c.Id == id, ct);

    // Nothing changes these categories, so skip change tracking.
    public async Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken ct = default) =>
        await context.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ThenBy(c => c.Id)
            .ToListAsync(ct);
}
