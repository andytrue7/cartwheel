using Cartwheel.Domain;
using Cartwheel.Shared.Categories;

namespace Cartwheel.Api.Mapping;

public static class CategoryMappings
{
    public static CategoryResponse ToResponse(this Category c) =>
        new(c.Id, c.Name);
}
