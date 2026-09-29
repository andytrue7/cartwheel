namespace Cartwheel.Domain.Repositories;

public enum ProductSortOrder
{
    Name,
    PriceAscending,
    PriceDescending
}

/// <summary>
/// Criteria for listing products. Every criterion is optional; the default filter
/// matches every product and sorts by name.
/// </summary>
public sealed record ProductFilter
{
    /// <summary>Case-insensitive part of the product name. Null or blank means no search.</summary>
    public string? Search { get; init; }

    public Guid? CategoryId { get; init; }

    /// <summary>When true, only products with stock above zero are returned.</summary>
    public bool InStockOnly { get; init; }

    public ProductSortOrder SortOrder { get; init; } = ProductSortOrder.Name;

    public static ProductFilter None { get; } = new();
}
