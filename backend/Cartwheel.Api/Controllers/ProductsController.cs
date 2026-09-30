using Cartwheel.Api.Mapping;
using Cartwheel.Domain;
using Cartwheel.Domain.Repositories;
using Cartwheel.Shared.Products;
using Microsoft.AspNetCore.Mvc;

namespace Cartwheel.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(
    IProductRepository products,
    ICategoryRepository categories) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResponse<ProductResponse>>> GetAll(
        [FromQuery] ProductListQuery query,
        CancellationToken ct)
    {
        // An unknown category simply matches nothing: filters narrow a list, they don't validate ids.
        // A page past the last one is not an error: 200 with no items and the real total.
        var page = await products.GetPageAsync(query.ToFilter(), query.ToPageRequest(), ct);
        return Ok(page.ToResponse());
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponse>> GetById(Guid id, CancellationToken ct)
    {
        var product = await products.GetByIdAsync(id, ct);
        return product is null ? NotFound() : Ok(product.ToResponse());
    }
    
    [HttpPost]
    [ProducesResponseType<ProductResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductResponse>> Create(CreateProductRequest request, CancellationToken ct)
    {
        // [Required] + [ApiController] guarantee CategoryId is non-null by the time we get here.
        var category = await categories.GetByIdAsync(request.CategoryId!.Value, ct);
        if (category is null)
        {
            return UnknownCategory();
        }

        var product = new Product(request.Name, request.Price, request.StockQuantity, category, request.Description);
        await products.AddAsync(product, ct);

        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product.ToResponse());
    }
    
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, UpdateProductRequest request, CancellationToken ct)
    {
        var product = await products.GetByIdAsync(id, ct);
        if (product is null)
        {
            return NotFound();
        }

        var category = await categories.GetByIdAsync(request.CategoryId!.Value, ct);
        if (category is null)
        {
            return UnknownCategory();
        }

        // One call, validated as a whole: either every detail changes or none does.
        product.UpdateDetails(request.Name, request.Description, request.Price, category);

        return await products.UpdateAsync(product, ct) ? NoContent() : NotFound();
    }
    
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct) =>
        await products.RemoveAsync(id, ct) ? NoContent() : NotFound();

    [HttpPost("{id:guid}/stock/increase")]
    [ProducesResponseType<ProductResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<ActionResult<ProductResponse>> IncreaseStock(
        Guid id, StockChangeRequest request, CancellationToken ct) =>
        ChangeStock(id, product => product.IncreaseStock(request.Quantity), ct);

    // Not enough stock throws InsufficientStockException; DomainExceptionHandler turns it into a 409.
    [HttpPost("{id:guid}/stock/decrease")]
    [ProducesResponseType<ProductResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public Task<ActionResult<ProductResponse>> DecreaseStock(
        Guid id, StockChangeRequest request, CancellationToken ct) =>
        ChangeStock(id, product => product.DecreaseStock(request.Quantity), ct);

    // Load, apply one domain method, save, return the new state: both stock actions share this.
    private async Task<ActionResult<ProductResponse>> ChangeStock(
        Guid id, Action<Product> change, CancellationToken ct)
    {
        var product = await products.GetByIdAsync(id, ct);
        if (product is null)
        {
            return NotFound();
        }

        change(product);

        return await products.UpdateAsync(product, ct) ? Ok(product.ToResponse()) : NotFound();
    }

    private ActionResult UnknownCategory()
    {
        ModelState.AddModelError(nameof(CreateProductRequest.CategoryId), "Category does not exist.");
        return ValidationProblem(ModelState);
    }
    
}