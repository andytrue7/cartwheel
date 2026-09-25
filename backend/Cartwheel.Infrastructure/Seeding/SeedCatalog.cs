using Cartwheel.Domain;

namespace Cartwheel.Infrastructure.Seeding;

public static class SeedCatalog
{
    public static IReadOnlyList<Product> CreateProducts()
    {
        var laptops = new Category("Laptops");
        var tvs = new Category("TVs");
        var smartphones = new Category("Smartphones");
        var headphones = new Category("Headphones");
        
        return 
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
    }
}