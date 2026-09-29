using Cartwheel.Api.Controllers;
using Cartwheel.Domain;
using Cartwheel.Domain.Repositories;
using Cartwheel.Shared.Categories;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Cartwheel.Tests.Controllers;

public class CategoriesControllerTests
{
    private readonly ICategoryRepository _categories = Substitute.For<ICategoryRepository>();
    private readonly CategoriesController _controller;

    public CategoriesControllerTests()
    {
        _controller = new CategoriesController(_categories);
    }

    [Fact]
    public async Task GetAll_CategoriesExist_ReturnsOkWithMappedCategoriesInRepositoryOrder()
    {
        // Arrange: not alphabetical on purpose, so a controller that re-sorted would fail.
        var phones = new Category("Phones");
        var laptops = new Category("Laptops");
        _categories.GetAllAsync(Arg.Any<CancellationToken>()).Returns([phones, laptops]);

        // Act
        var result = await _controller.GetAll(CancellationToken.None);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var body = Assert.IsAssignableFrom<IEnumerable<CategoryResponse>>(ok.Value);
        Assert.Equal(
        [
            new CategoryResponse(phones.Id, "Phones"),
            new CategoryResponse(laptops.Id, "Laptops")
        ], body);
    }
}
