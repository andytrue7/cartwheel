using Cartwheel.Api.Mapping;
using Cartwheel.Domain.Repositories;
using Cartwheel.Shared.Categories;
using Microsoft.AspNetCore.Mvc;

namespace Cartwheel.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController(ICategoryRepository categories) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CategoryResponse>>> GetAll(CancellationToken ct)
    {
        var all = await categories.GetAllAsync(ct);
        return Ok(all.Select(c => c.ToResponse()).ToList());
    }
}
