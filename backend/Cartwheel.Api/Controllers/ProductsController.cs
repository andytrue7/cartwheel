using Cartwheel.Api.Mapping;
using Cartwheel.Domain.Repositories;
using Cartwheel.Shared.Products;
using Microsoft.AspNetCore.Mvc;

namespace Cartwheel.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(IProductRepository products) : ControllerBase
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
    
}