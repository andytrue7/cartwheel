using Cartwheel.Domain;
using Cartwheel.Domain.Repositories;
using Cartwheel.Shared.Products;

namespace Cartwheel.Api.Mapping;

public static class ProductMappings
{
    public static ProductResponse ToResponse(this Product p) =>
        new(p.Id, p.Name, p.Description, p.Price, p.StockQuantity, p.Category.Id, p.Category.Name);

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
