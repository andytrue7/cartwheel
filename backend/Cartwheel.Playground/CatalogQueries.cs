using Cartwheel.Domain;

namespace Cartwheel.Playground;

/// <summary>Name, product count and average price for one category.</summary>
public record CategorySummary(string CategoryName, int ProductCount, decimal AveragePrice);

/// <summary>A lightweight projection of a product: just its name and price.</summary>
public record ProductNameAndPrice(string Name, decimal Price);

/// <summary>
/// Read-only queries over a sequence of products, written as extension methods.
/// Every method returns data and never prints; the caller decides how to show it.
/// Methods that return IEnumerable are lazy: they run only when the result is enumerated.
/// </summary>
public static class CatalogQueries
{
    public static IEnumerable<Product> InCategory(this IEnumerable<Product> products, string categoryName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(categoryName);

        return products.Where(p =>
            string.Equals(p.Category.Name, categoryName, StringComparison.OrdinalIgnoreCase));
    }

    public static IEnumerable<Product> SearchByName(this IEnumerable<Product> products, string searchTerm)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(searchTerm);

        return products.Where(p => p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
    }

    public static IEnumerable<Product> SortedByPrice(this IEnumerable<Product> products)
    {
        return products
            .OrderBy(p => p.Price)
            .ThenBy(p => p.Name);
    }

    public static IEnumerable<Product> MostExpensive(this IEnumerable<Product> products, int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);

        return products
            .OrderByDescending(p => p.Price)
            .ThenBy(p => p.Name)
            .Take(count);
    }

    public static IEnumerable<CategorySummary> SummarizeByCategory(this IEnumerable<Product> products)
    {
        return products
            .GroupBy(p => p.Category.Id)
            .Select(group => new CategorySummary(
                CategoryName: group.First().Category.Name,
                ProductCount: group.Count(),
                AveragePrice: group.Average(p => p.Price)))
            .OrderBy(summary => summary.CategoryName);
    }

    public static IEnumerable<Product> InStock(this IEnumerable<Product> products)
    {
        return products.Where(p => p.StockQuantity > 0);
    }

    public static bool AnyPricedAbove(this IEnumerable<Product> products, decimal threshold)
    {
        return products.Any(p => p.Price > threshold);
    }

    public static bool AllHaveDescription(this IEnumerable<Product> products)
    {
        return products.All(p => !string.IsNullOrWhiteSpace(p.Description));
    }

    public static decimal TotalInventoryValue(this IEnumerable<Product> products)
    {
        return products.Sum(p => p.Price * p.StockQuantity);
    }

    public static IEnumerable<ProductNameAndPrice> NamesAndPrices(this IEnumerable<Product> products)
    {
        return products.Select(p => new ProductNameAndPrice(p.Name, p.Price));
    }
}
