using System.Collections.Generic;

namespace Cartwheel.Shared.Products;

public sealed record PagedResponse<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);
