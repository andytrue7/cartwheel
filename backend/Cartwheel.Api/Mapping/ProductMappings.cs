using Cartwheel.Domain;
using Cartwheel.Domain.Repositories;
using Cartwheel.Shared.Products;

namespace Cartwheel.Api.Mapping;

public static class ProductMappings
{
    public static ProductResponse ToResponse(this Product p) =>
        new(p.Id, p.Name, p.Description, p.Price, p.StockQuantity, p.Category.Id, p.Category.Name);

    public static PageRequest ToPageRequest(this ProductListQuery q) => new(q.Page, q.PageSize);

    public static PagedResponse<ProductResponse> ToResponse(this PagedResult<Product> result) =>
        new(
            result.Items.Select(p => p.ToResponse()).ToList(),
            result.Page.Page,
            result.Page.PageSize,
            result.TotalCount,
            // Ceiling division: 12 items in pages of 5 is 3 pages. No items means 0 pages.
            (int)Math.Ceiling(result.TotalCount / (double)result.Page.PageSize));

    public static ProductFilter ToFilter(this ProductListQuery q) =>
        new()
        {
            Search = q.Search,
            CategoryId = q.CategoryId,
            InStockOnly = q.InStock,
            SortOrder = q.Sort switch
            {
                ProductSort.Name => ProductSortOrder.Name,
                ProductSort.PriceAsc => ProductSortOrder.PriceAscending,
                ProductSort.PriceDesc => ProductSortOrder.PriceDescending,
                _ => throw new ArgumentOutOfRangeException(nameof(q), q.Sort, "Unknown sort value.")
            }
        };
}
