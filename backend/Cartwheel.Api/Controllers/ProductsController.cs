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
    public async Task<ActionResult<IReadOnlyList<ProductResponse>>> GetAll(CancellationToken ct)
    {
        var all = await products.GetAllAsync(ct);
        return Ok(all.Select(p => p.ToResponse()).ToList());
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

    private ActionResult UnknownCategory()
    {
        ModelState.AddModelError(nameof(CreateProductRequest.CategoryId), "Category does not exist.");
        return ValidationProblem(ModelState);
    }
    
}