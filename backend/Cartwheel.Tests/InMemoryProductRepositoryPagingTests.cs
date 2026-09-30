using Cartwheel.Domain.Repositories;
using Cartwheel.Infrastructure.Repositories;
using Cartwheel.Infrastructure.Seeding;

namespace Cartwheel.Tests;

public class InMemoryProductRepositoryPagingTests
{
    private readonly InMemoryProductRepository _repository = new(SeedCatalog.Create().Products);

    [Fact]
    public async Task GetPageAsync_FirstPage_ReturnsPageSizeItems()
    {
        var result = await _repository.GetPageAsync(null, new PageRequest(1, 5));

        Assert.Equal(5, result.Items.Count);
        Assert.Equal(12, result.TotalCount);
    }

    [Fact]
    public async Task GetPageAsync_LastPage_ReturnsTheRemainingItems()
    {
        var result = await _repository.GetPageAsync(null, new PageRequest(3, 5));

        Assert.Equal(2, result.Items.Count);
        Assert.Equal(12, result.TotalCount);
    }

    [Fact]
    public async Task GetPageAsync_PageBeyondTheLast_IsEmptyWithRealTotal()
    {
        var result = await _repository.GetPageAsync(null, new PageRequest(4, 5));

        Assert.Empty(result.Items);
        Assert.Equal(12, result.TotalCount);
    }

    [Fact]
    public async Task GetPageAsync_AllPages_CoverEveryProductOnceInSortedOrder()
    {
        var names = new List<string>();
        for (var page = 1; page <= 3; page++)
        {
            names.AddRange((await _repository.GetPageAsync(null, new PageRequest(page, 5))).Items.Select(p => p.Name));
        }

        var all = (await _repository.GetAllAsync()).Select(p => p.Name);
        Assert.Equal(all, names);
    }

    [Fact]
    public async Task GetPageAsync_WithFilter_CountsOnlyMatchingProducts()
    {
        var inStock = (await _repository.GetAllAsync(new ProductFilter { InStockOnly = true })).Count;
        Assert.InRange(inStock, 1, 11); // guards the test: the filter must actually remove something

        var result = await _repository.GetPageAsync(
            new ProductFilter { InStockOnly = true }, new PageRequest(1, 5));

        Assert.Equal(inStock, result.TotalCount);
        Assert.Equal(Math.Min(5, inStock), result.Items.Count);
        Assert.All(result.Items, p => Assert.True(p.StockQuantity > 0));
    }

    [Fact]
    public async Task GetPageAsync_ReturnsTheRequestItAnswered()
    {
        var request = new PageRequest(2, 5);

        var result = await _repository.GetPageAsync(null, request);

        Assert.Equal(request, result.Page);
    }
}
