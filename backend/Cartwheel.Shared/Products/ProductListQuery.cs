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

    // The limits repeat PageRequest.MaxPage and PageRequest.MaxPageSize: this project targets
    // netstandard2.0 and can't reference the domain.
    [Range(1, 1_000_000)]
    public int Page { get; init; } = 1;

    [Range(1, 100)]
    public int PageSize { get; init; } = 20;
}
