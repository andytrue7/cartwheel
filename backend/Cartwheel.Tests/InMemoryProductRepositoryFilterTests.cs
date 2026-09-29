using Cartwheel.Domain;
using Cartwheel.Domain.Repositories;
using Cartwheel.Infrastructure.Repositories;

namespace Cartwheel.Tests;

public class InMemoryProductRepositoryFilterTests
{
    private readonly Category _laptops = new("Laptops");
    private readonly Category _phones = new("Phones");

    private readonly InMemoryProductRepository _repository;

    public InMemoryProductRepositoryFilterTests()
    {
        _repository = new InMemoryProductRepository(
        [
            new Product("MacBook Pro", 1999m, 3, _laptops),
            new Product("iMac", 1299m, 0, _laptops),
            new Product("Dell XPS", 1299m, 5, _laptops),
            new Product("Pixel", 799m, 9, _phones),
            new Product("iPhone", 899m, 0, _phones),
            new Product("Galaxy", 799m, 4, _phones)
        ]);
    }

    private async Task<IEnumerable<string>> NamesFor(ProductFilter filter) =>
        (await _repository.GetAllAsync(filter)).Select(p => p.Name);

    [Fact]
    public async Task GetAllAsync_EmptyFilter_ReturnsEverythingSortedByName()
    {
        Assert.Equal(
            ["Dell XPS", "Galaxy", "iMac", "iPhone", "MacBook Pro", "Pixel"],
            await NamesFor(new ProductFilter()));
    }

    [Fact]
    public async Task GetAllAsync_NullFilter_MatchesEmptyFilter()
    {
        var withNull = (await _repository.GetAllAsync(filter: null)).Select(p => p.Name);

        Assert.Equal(await NamesFor(new ProductFilter()), withNull);
    }

    [Theory]
    [InlineData("MAC")]
    [InlineData("mac")]
    [InlineData("  Mac  ")]
    public async Task GetAllAsync_Search_IgnoresCaseAndMatchesPartOfName(string search)
    {
        Assert.Equal(["iMac", "MacBook Pro"], await NamesFor(new ProductFilter { Search = search }));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetAllAsync_BlankSearch_ReturnsEverything(string search)
    {
        Assert.Equal(6, (await _repository.GetAllAsync(new ProductFilter { Search = search })).Count);
    }

    [Fact]
    public async Task GetAllAsync_SearchWithNoMatch_ReturnsEmpty()
    {
        Assert.Empty(await NamesFor(new ProductFilter { Search = "toaster" }));
    }

    [Fact]
    public async Task GetAllAsync_CategoryFilter_ReturnsOnlyThatCategory()
    {
        Assert.Equal(
            ["Galaxy", "iPhone", "Pixel"],
            await NamesFor(new ProductFilter { CategoryId = _phones.Id }));
    }

    [Fact]
    public async Task GetAllAsync_UnknownCategory_ReturnsEmpty()
    {
        Assert.Empty(await NamesFor(new ProductFilter { CategoryId = Guid.NewGuid() }));
    }

    [Fact]
    public async Task GetAllAsync_InStockOnly_ExcludesProductsWithZeroStock()
    {
        Assert.Equal(
            ["Dell XPS", "Galaxy", "MacBook Pro", "Pixel"],
            await NamesFor(new ProductFilter { InStockOnly = true }));
    }

    [Fact]
    public async Task GetAllAsync_SortByPriceAscending_BreaksTiesByName()
    {
        Assert.Equal(
            ["Galaxy", "Pixel", "iPhone", "Dell XPS", "iMac", "MacBook Pro"],
            await NamesFor(new ProductFilter { SortOrder = ProductSortOrder.PriceAscending }));
    }

    [Fact]
    public async Task GetAllAsync_SortByPriceDescending_BreaksTiesByName()
    {
        // Ties stay in name order (A→Z) even though prices run high→low.
        Assert.Equal(
            ["MacBook Pro", "Dell XPS", "iMac", "iPhone", "Galaxy", "Pixel"],
            await NamesFor(new ProductFilter { SortOrder = ProductSortOrder.PriceDescending }));
    }

    [Fact]
    public async Task GetAllAsync_CombinedFilters_AllMustMatch()
    {
        var filter = new ProductFilter
        {
            Search = "i",
            CategoryId = _laptops.Id,
            InStockOnly = true,
            SortOrder = ProductSortOrder.PriceDescending
        };

        // "i" matches iMac, iPhone and Pixel; the category leaves iMac; in-stock drops it too.
        Assert.Empty(await NamesFor(filter));

        // Without the in-stock restriction iMac comes back.
        Assert.Equal(["iMac"], await NamesFor(filter with { InStockOnly = false }));
    }

    [Fact]
    public async Task GetAllAsync_CategoryInStockAndPriceSort_Combine()
    {
        var filter = new ProductFilter
        {
            CategoryId = _laptops.Id,
            InStockOnly = true,
            SortOrder = ProductSortOrder.PriceAscending
        };

        Assert.Equal(["Dell XPS", "MacBook Pro"], await NamesFor(filter));
    }

    [Fact]
    public async Task GetAllAsync_SearchAndCategory_Combine()
    {
        var filter = new ProductFilter { Search = "a", CategoryId = _phones.Id };

        // "a" also matches iMac and MacBook Pro, but they are laptops.
        Assert.Equal(["Galaxy"], await NamesFor(filter));
    }
}
