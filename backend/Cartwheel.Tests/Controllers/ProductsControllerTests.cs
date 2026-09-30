using Cartwheel.Api.Controllers;
using Cartwheel.Domain;
using Cartwheel.Domain.Exceptions;
using Cartwheel.Domain.Repositories;
using Cartwheel.Shared.Products;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Cartwheel.Tests.Controllers;

// Unit tests call the action methods directly: no routing, model binding, validation filters,
// or exception handler. They check the controller's own decisions, not the HTTP pipeline.
public class ProductsControllerTests
{
    // xUnit creates a new instance of this class for every test, so each test gets fresh substitutes.
    private readonly IProductRepository _products = Substitute.For<IProductRepository>();
    private readonly ICategoryRepository _categories = Substitute.For<ICategoryRepository>();
    private readonly ProductsController _controller;
    private readonly Category _category = new("Laptops");

    public ProductsControllerTests()
    {
        _controller = new ProductsController(_products, _categories);
    }

    // Builds a valid product. Each test overrides only the value it cares about.
    private Product CreateProduct(
        string name = "MacBook Air",
        decimal price = 1000m,
        int stock = 10,
        string? description = "A laptop")
    {
        return new Product(name, price, stock, _category, description);
    }

    // Makes the product repository know about this product.
    private void GivenExistingProduct(Product product)
    {
        _products.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);
    }

    // The trap: outside the pipeline, ValidationProblem() does not return a 400.
    // ControllerBase.ValidationProblem builds its body with a ProblemDetailsFactory that it gets from
    // HttpContext.RequestServices. A controller created with "new" has no HttpContext, so there is no
    // factory, and ControllerBase falls back to "new ValidationProblemDetails(ModelState)" with
    // Status = null. A problem without Status 400 comes back as a plain ObjectResult whose StatusCode
    // is null, not as a BadRequestObjectResult. In the running API the factory fills in 400, the
    // title, and the type link, and the result becomes a BadRequestObjectResult.
    // So the status code belongs to the framework, and asserting 400 here would test something the
    // controller doesn't control. The controller does control three things: it returns an
    // ObjectResult (BadRequestObjectResult derives from it, so this check holds in both cases),
    // the body is ValidationProblemDetails, and the error is on the right field.
    private static void AssertValidationProblem(IActionResult? result, string field)
    {
        var objectResult = Assert.IsAssignableFrom<ObjectResult>(result);
        var problem = Assert.IsType<ValidationProblemDetails>(objectResult.Value);
        var error = Assert.Single(problem.Errors);
        Assert.Equal(field, error.Key);
    }

    // ---------- GetAll ----------

    // NSubstitute can't invent a PagedResult (it isn't an interface or a collection), so an unstubbed
    // GetPageAsync returns null. Tests that only check what the repository received use this.
    private void GivenEmptyPage()
    {
        _products.GetPageAsync(Arg.Any<ProductFilter?>(), Arg.Any<PageRequest>(), Arg.Any<CancellationToken>())
            .Returns(new PagedResult<Product>([], 0, PageRequest.First));
    }

    [Fact]
    public async Task GetAll_ProductsExist_ReturnsOkWithMappedPage()
    {
        // Arrange
        var laptop = CreateProduct();
        var phone = CreateProduct(name: "iPhone", price: 800m, stock: 3, description: null);
        var request = new PageRequest(1, 2);
        _products.GetPageAsync(Arg.Any<ProductFilter?>(), Arg.Any<PageRequest>(), Arg.Any<CancellationToken>())
            .Returns(new PagedResult<Product>([laptop, phone], 5, request));

        // Act
        var result = await _controller.GetAll(new ProductListQuery { PageSize = 2 }, CancellationToken.None);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var body = Assert.IsType<PagedResponse<ProductResponse>>(ok.Value);
        Assert.Equal(
        [
            new ProductResponse(laptop.Id, "MacBook Air", "A laptop", 1000m, 10, _category.Id, "Laptops"),
            new ProductResponse(phone.Id, "iPhone", null, 800m, 3, _category.Id, "Laptops")
        ], body.Items);
        Assert.Equal(1, body.Page);
        Assert.Equal(2, body.PageSize);
        Assert.Equal(5, body.TotalCount);
        Assert.Equal(3, body.TotalPages);
    }

    [Fact]
    public async Task GetAll_PageBeyondTheLast_ReturnsOkWithEmptyItemsAndRealTotal()
    {
        _products.GetPageAsync(Arg.Any<ProductFilter?>(), Arg.Any<PageRequest>(), Arg.Any<CancellationToken>())
            .Returns(new PagedResult<Product>([], 12, new PageRequest(9, 5)));

        var result = await _controller.GetAll(new ProductListQuery { Page = 9, PageSize = 5 }, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var body = Assert.IsType<PagedResponse<ProductResponse>>(ok.Value);
        Assert.Empty(body.Items);
        Assert.Equal(12, body.TotalCount);
        Assert.Equal(3, body.TotalPages);
    }

    [Fact]
    public async Task GetAll_NothingMatches_ReturnsZeroTotalPages()
    {
        _products.GetPageAsync(Arg.Any<ProductFilter?>(), Arg.Any<PageRequest>(), Arg.Any<CancellationToken>())
            .Returns(new PagedResult<Product>([], 0, PageRequest.First));

        var result = await _controller.GetAll(new ProductListQuery(), CancellationToken.None);

        var body = Assert.IsType<PagedResponse<ProductResponse>>(Assert.IsType<OkObjectResult>(result.Result).Value);
        Assert.Equal(0, body.TotalPages);
    }

    [Fact]
    public async Task GetAll_AllQueryParameters_PassesMatchingFilterToRepository()
    {
        GivenEmptyPage();
        var categoryId = Guid.NewGuid();
        var query = new ProductListQuery
        {
            Search = "mac",
            CategoryId = categoryId,
            InStock = true,
            Sort = ProductSort.PriceDesc
        };
        var expectedFilter = new ProductFilter
        {
            Search = "mac",
            CategoryId = categoryId,
            InStockOnly = true,
            SortOrder = ProductSortOrder.PriceDescending
        };

        await _controller.GetAll(query, CancellationToken.None);

        // ProductFilter is a record, so Arg.Is(expectedFilter) matches by value, not by reference.
        await _products.Received(1).GetPageAsync(
            Arg.Is(expectedFilter), Arg.Any<PageRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAll_EmptyQuery_PassesDefaultFilterAndFirstPageToRepository()
    {
        GivenEmptyPage();
        await _controller.GetAll(new ProductListQuery(), CancellationToken.None);

        await _products.Received(1).GetPageAsync(
            Arg.Is(ProductFilter.None), Arg.Is(new PageRequest(1, PageRequest.DefaultPageSize)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAll_PageAndPageSize_ReachRepositoryAsPageRequest()
    {
        GivenEmptyPage();
        await _controller.GetAll(new ProductListQuery { Page = 2, PageSize = 5 }, CancellationToken.None);

        await _products.Received(1).GetPageAsync(
            Arg.Any<ProductFilter?>(), Arg.Is(new PageRequest(2, 5)), Arg.Any<CancellationToken>());
    }

    // ---------- GetById ----------

    [Fact]
    public async Task GetById_ExistingProduct_ReturnsOkWithMappedProduct()
    {
        var laptop = CreateProduct();
        GivenExistingProduct(laptop);

        var result = await _controller.GetById(laptop.Id, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var body = Assert.IsType<ProductResponse>(ok.Value);
        Assert.Equal(laptop.Id, body.Id);
        Assert.Equal("MacBook Air", body.Name);
        Assert.Equal("A laptop", body.Description);
        Assert.Equal(1000m, body.Price);
        Assert.Equal(10, body.StockQuantity);
        Assert.Equal(_category.Id, body.CategoryId);
        Assert.Equal("Laptops", body.CategoryName);
    }

    [Fact]
    public async Task GetById_MissingProduct_ReturnsNotFound()
    {
        _products.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Product?)null);

        var result = await _controller.GetById(Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    // ---------- Create ----------

    [Fact]
    public async Task Create_ValidRequest_ReturnsCreatedAtGetByIdAndAddsProduct()
    {
        _categories.GetByIdAsync(_category.Id, Arg.Any<CancellationToken>()).Returns(_category);
        var request = new CreateProductRequest
        {
            Name = "MacBook Pro",
            Description = "14-inch",
            Price = 2000m,
            StockQuantity = 5,
            CategoryId = _category.Id
        };

        var result = await _controller.Create(request, CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(ProductsController.GetById), created.ActionName);
        var body = Assert.IsType<ProductResponse>(created.Value);
        Assert.Equal("MacBook Pro", body.Name);
        Assert.Equal(_category.Id, body.CategoryId);

        // The controller creates the id, so the test reads it from the response and then checks
        // that the Location route and the product saved to the repository both use it.
        Assert.NotNull(created.RouteValues);
        Assert.Equal(body.Id, created.RouteValues["id"]);
        await _products.Received(1).AddAsync(
            Arg.Is<Product>(p => p.Id == body.Id && p.Name == "MacBook Pro" && p.Category == _category),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Create_UnknownCategory_ReturnsValidationProblemAndAddsNothing()
    {
        _categories.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Category?)null);
        var request = new CreateProductRequest
        {
            Name = "MacBook Pro",
            Price = 2000m,
            StockQuantity = 5,
            CategoryId = Guid.NewGuid()
        };

        var result = await _controller.Create(request, CancellationToken.None);

        AssertValidationProblem(result.Result, nameof(CreateProductRequest.CategoryId));
        await _products.DidNotReceive().AddAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>());
    }

    // ---------- Update ----------

    private UpdateProductRequest CreateUpdateRequest(Guid categoryId) => new()
    {
        Name = "MacBook Pro",
        Description = "14-inch",
        Price = 2000m,
        CategoryId = categoryId
    };

    [Fact]
    public async Task Update_MissingProduct_ReturnsNotFoundWithoutLookingUpCategory()
    {
        _products.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Product?)null);

        var result = await _controller.Update(
            Guid.NewGuid(), CreateUpdateRequest(_category.Id), CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
        await _categories.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _products.DidNotReceive().UpdateAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Update_UnknownCategory_ReturnsValidationProblemAndLeavesProductUnchanged()
    {
        var laptop = CreateProduct();
        GivenExistingProduct(laptop);
        _categories.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Category?)null);

        var result = await _controller.Update(
            laptop.Id, CreateUpdateRequest(Guid.NewGuid()), CancellationToken.None);

        AssertValidationProblem(result, nameof(UpdateProductRequest.CategoryId));
        await _products.DidNotReceive().UpdateAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>());
        Assert.Equal("MacBook Air", laptop.Name);
        Assert.Equal("A laptop", laptop.Description);
        Assert.Equal(1000m, laptop.Price);
        Assert.Same(_category, laptop.Category);
    }

    [Fact]
    public async Task Update_ValidRequest_ReturnsNoContentAndSavesChanges()
    {
        var laptop = CreateProduct();
        GivenExistingProduct(laptop);
        var workstations = new Category("Workstations");
        _categories.GetByIdAsync(workstations.Id, Arg.Any<CancellationToken>()).Returns(workstations);
        _products.UpdateAsync(laptop, Arg.Any<CancellationToken>()).Returns(true);

        var result = await _controller.Update(
            laptop.Id, CreateUpdateRequest(workstations.Id), CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        await _products.Received(1).UpdateAsync(laptop, Arg.Any<CancellationToken>());
        Assert.Equal("MacBook Pro", laptop.Name);
        Assert.Same(workstations, laptop.Category);
    }

    // The product existed when it was loaded but was deleted before the save.
    [Fact]
    public async Task Update_ProductRemovedBeforeSave_ReturnsNotFound()
    {
        var laptop = CreateProduct();
        GivenExistingProduct(laptop);
        _categories.GetByIdAsync(_category.Id, Arg.Any<CancellationToken>()).Returns(_category);
        _products.UpdateAsync(laptop, Arg.Any<CancellationToken>()).Returns(false);

        var result = await _controller.Update(
            laptop.Id, CreateUpdateRequest(_category.Id), CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    // ---------- Delete ----------

    [Fact]
    public async Task Delete_RepositoryRemovesProduct_ReturnsNoContent()
    {
        var id = Guid.NewGuid();
        _products.RemoveAsync(id, Arg.Any<CancellationToken>()).Returns(true);

        var result = await _controller.Delete(id, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_RepositoryFindsNothing_ReturnsNotFound()
    {
        var id = Guid.NewGuid();
        _products.RemoveAsync(id, Arg.Any<CancellationToken>()).Returns(false);

        var result = await _controller.Delete(id, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    // ---------- Stock ----------

    [Fact]
    public async Task DecreaseStock_WithinStock_ReturnsOkWithNewStock()
    {
        var laptop = CreateProduct(stock: 10);
        GivenExistingProduct(laptop);
        _products.UpdateAsync(laptop, Arg.Any<CancellationToken>()).Returns(true);

        var result = await _controller.DecreaseStock(
            laptop.Id, new StockChangeRequest { Quantity = 3 }, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var body = Assert.IsType<ProductResponse>(ok.Value);
        Assert.Equal(7, body.StockQuantity);
        await _products.Received(1).UpdateAsync(laptop, Arg.Any<CancellationToken>());
    }

    // Without the pipeline nothing turns the exception into a 409, so the test sees it thrown.
    // DomainExceptionHandler's mapping needs its own tests.
    [Fact]
    public async Task DecreaseStock_BeyondStock_ThrowsAndSavesNothing()
    {
        var laptop = CreateProduct(stock: 2);
        GivenExistingProduct(laptop);

        await Assert.ThrowsAsync<InsufficientStockException>(() => _controller.DecreaseStock(
            laptop.Id, new StockChangeRequest { Quantity = 5 }, CancellationToken.None));

        await _products.DidNotReceive().UpdateAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>());
        Assert.Equal(2, laptop.StockQuantity);
    }

    // The same race as in Update, through the shared ChangeStock helper.
    [Fact]
    public async Task DecreaseStock_ProductRemovedBeforeSave_ReturnsNotFound()
    {
        var laptop = CreateProduct(stock: 10);
        GivenExistingProduct(laptop);
        _products.UpdateAsync(laptop, Arg.Any<CancellationToken>()).Returns(false);

        var result = await _controller.DecreaseStock(
            laptop.Id, new StockChangeRequest { Quantity = 3 }, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    // Line coverage counted IncreaseStock's lambda as covered by the 404 test alone, because the
    // lambda is created on the same line. Only this test actually runs it.
    [Fact]
    public async Task IncreaseStock_WithinLimit_ReturnsOkWithNewStock()
    {
        var laptop = CreateProduct(stock: 10);
        GivenExistingProduct(laptop);
        _products.UpdateAsync(laptop, Arg.Any<CancellationToken>()).Returns(true);

        var result = await _controller.IncreaseStock(
            laptop.Id, new StockChangeRequest { Quantity = 5 }, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var body = Assert.IsType<ProductResponse>(ok.Value);
        Assert.Equal(15, body.StockQuantity);
        await _products.Received(1).UpdateAsync(laptop, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task IncreaseStock_BeyondLimit_ThrowsAndSavesNothing()
    {
        var laptop = CreateProduct(stock: Product.MaxStockQuantity);
        GivenExistingProduct(laptop);

        await Assert.ThrowsAsync<StockLimitExceededException>(() => _controller.IncreaseStock(
            laptop.Id, new StockChangeRequest { Quantity = 1 }, CancellationToken.None));

        await _products.DidNotReceive().UpdateAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>());
        Assert.Equal(Product.MaxStockQuantity, laptop.StockQuantity);
    }

    [Fact]
    public async Task IncreaseStock_MissingProduct_ReturnsNotFound()
    {
        _products.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Product?)null);

        var result = await _controller.IncreaseStock(
            Guid.NewGuid(), new StockChangeRequest { Quantity = 1 }, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result.Result);
        await _products.DidNotReceive().UpdateAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>());
    }
}
