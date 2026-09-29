using System;
using System.ComponentModel.DataAnnotations;

namespace Cartwheel.Shared.Products;

/// <summary>Values accepted by the <c>sort</c> query parameter (case-insensitive).</summary>
public enum ProductSort
{
    Name,
    PriceAsc,
    PriceDesc
}

/// <summary>
/// Query string for listing products, e.g. <c>?search=mac&amp;categoryId=...&amp;inStock=true&amp;sort=priceDesc</c>.
/// Every parameter is optional.
/// </summary>
public sealed class ProductListQuery
{
    [StringLength(100)]
    public string? Search { get; init; }

    public Guid? CategoryId { get; init; }

    public bool InStock { get; init; }

    public ProductSort Sort { get; init; } = ProductSort.Name;
}
