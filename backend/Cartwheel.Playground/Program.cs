using System.Globalization;
using Cartwheel.Domain;

namespace Cartwheel.Playground;

class Program
{
    static void Main(string[] args)
    {
        Category category1 = new Category("Laptops");
        Category category2 = new Category("TVs");
        Category category3 = new Category("Smartphones");

        Product[] products =
        [
            new Product("Mac M1", 100, 5, category1, "Cool laptop"),
            new Product("Mac M2", 100, 5, category1, "Cool laptop"),
            new Product("Mac M3", 100, 5, category1, "Cool laptop"),
            new Product("LG Smart tv", 200, 12, category2 , "Cool tv"),
            new Product("Iphone 17", 90, 8, category3, "Cool smartphone")
        ];

        var usCulture = CultureInfo.GetCultureInfo("en-US");

        foreach (var product in products)
        {
            Console.WriteLine($"{product.Category.Name} | {product.Name} | {product.Price.ToString("C", usCulture)} | {product.StockQuantity} |");
            Console.WriteLine("--------------------------");
        }
    }
}