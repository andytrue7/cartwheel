using System.Globalization;
using Cartwheel.Domain;

namespace Cartwheel.Playground;

class Program
{
    private static readonly CultureInfo UsCulture = CultureInfo.GetCultureInfo("en-US");

    static void Main(string[] args)
    {
        var laptops = new Category("Laptops");
        var tvs = new Category("TVs");
        var smartphones = new Category("Smartphones");
        var headphones = new Category("Headphones");
        
        Product[] products =
        [
            new Product("MacBook Air M3", 1099m, 7, laptops, "13-inch everyday laptop"),
            new Product("MacBook Pro M4", 1999m, 3, laptops, "14-inch laptop for professionals"),
            new Product("Dell XPS 13", 949m, 0, laptops, "Compact Windows ultrabook"),
            new Product("LG OLED C4", 1499m, 4, tvs, "55-inch OLED TV"),
            new Product("Samsung Crystal UHD", 449m, 10, tvs, "50-inch 4K LED TV"),
            new Product("Sony Bravia X80", 699m, 6, tvs, "55-inch 4K TV"),
            new Product("iPhone 17", 899m, 12, smartphones, "Apple smartphone"),
            new Product("Pixel 10", 799m, 9, smartphones, "Google smartphone"),
            new Product("Galaxy S26", 899m, 0, smartphones, "Samsung smartphone"),
            new Product("AirPods Pro", 249m, 25, headphones, "Noise-cancelling earbuds"),
            new Product("Sony WH-1000XM6", 399m, 8, headphones, "Over-ear headphones"),
            new Product("JBL Tune 520", 49m, 40, headphones)
        ];

        PrintProducts("Catalog", products);

        RunCartDemo(products);

        PrintProducts("Category: laptops", products.InCategory("laptops"));
        PrintProducts("Search: \"mac\"", products.SearchByName("mac"));
        PrintProducts("Sorted by price, then by name", products.SortedByPrice());
        PrintProducts("3 most expensive", products.MostExpensive(3));

        PrintHeading("Per category");
        foreach (var summary in products.SummarizeByCategory())
        {
            Console.WriteLine($"{summary.CategoryName}: {summary.ProductCount} products, average {Money(summary.AveragePrice)}");
        }

        PrintProducts("In stock", products.InStock());

        PrintHeading("Checks");
        Console.WriteLine($"Any product over {Money(1000m)}: {products.AnyPricedAbove(1000m)}");
        Console.WriteLine($"All products have a description: {products.AllHaveDescription()}");
        Console.WriteLine($"Total inventory value: {Money(products.TotalInventoryValue())}");

        PrintHeading("Names and prices");
        foreach (var item in products.NamesAndPrices())
        {
            Console.WriteLine($"{item.Name} | {Money(item.Price)}");
        }

        // Runs last, because it sells out two products and changes the catalog.
        RunDeferredExecutionExperiment(products);
    }

    private static void RunCartDemo(Product[] products)
    {
        var macBookAir = products.Single(p => p.Name == "MacBook Air M3");
        var samsungTv = products.Single(p => p.Name == "Samsung Crystal UHD");

        var cart = new Cart();
        cart.AddItem(macBookAir, 2);
        cart.AddItem(macBookAir, 1); // merges into the existing line, quantity becomes 3
        cart.AddItem(samsungTv);
        cart.UpdateQuantity(samsungTv.Id, 4);

        PrintHeading("Cart");
        foreach (var item in cart.Items)
        {
            Console.WriteLine($"{item.Product.Name} x{item.Quantity} = {Money(item.LineTotal)}");
        }
        Console.WriteLine($"Total units: {cart.GetTotalUnits()}");
        Console.WriteLine($"Total price: {Money(cart.GetTotalPrice())}");
    }

    private static void RunDeferredExecutionExperiment(Product[] products)
    {
        PrintHeading("Deferred execution");

        // Lazy: this line only DESCRIBES the query. Nothing is filtered yet.
        IEnumerable<Product> lazyInStock = products.InStock();

        var pixel = products.Single(p => p.Name == "Pixel 10");
        int lazyCountBefore = lazyInStock.Count(); // the query runs now
        pixel.DecreaseStock(pixel.StockQuantity); // sell out Pixel 10
        int lazyCountAfter = lazyInStock.Count(); // the query runs AGAIN and sees the sale

        Console.WriteLine($"Lazy query, before selling out Pixel 10: {lazyCountBefore} in stock");
        Console.WriteLine($"Lazy query, after selling out Pixel 10:  {lazyCountAfter} in stock");
        Console.WriteLine($"Lazy query includes Pixel 10: {lazyInStock.Contains(pixel)}");

        // Eager: ToList runs the query immediately and stores a snapshot of the results.
        List<Product> eagerInStock = products.InStock().ToList();

        var airPods = products.Single(p => p.Name == "AirPods Pro");
        airPods.DecreaseStock(airPods.StockQuantity); // sell out AirPods Pro

        Console.WriteLine($"Eager list, after selling out AirPods Pro: {eagerInStock.Count} items");
        Console.WriteLine($"Eager list includes AirPods Pro: {eagerInStock.Contains(airPods)} (stale snapshot, real stock is {airPods.StockQuantity})");
    }

    private static void PrintProducts(string title, IEnumerable<Product> products)
    {
        PrintHeading(title);
        foreach (var product in products)
        {
            Console.WriteLine($"{product.Name} | {product.Category.Name} | {Money(product.Price)} | stock {product.StockQuantity}");
        }
    }

    private static void PrintHeading(string title)
    {
        Console.WriteLine();
        Console.WriteLine($"=== {title} ===");
    }

    private static string Money(decimal amount) => amount.ToString("C", UsCulture);
}
