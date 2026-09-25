using Cartwheel.Domain;
using Cartwheel.Shared.Products;

namespace Cartwheel.Api.Mapping;

public static class ProductMappings
{
    public static ProductResponse ToResponse(this Product p) =>
        new(p.Id, p.Name, p.Description, p.Price, p.StockQuantity, p.Category.Id, p.Category.Name);
}