using System;

namespace Cartwheel.Shared.Products;

public sealed record ProductResponse(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
    int StockQuantity,
    Guid CategoryId,
    string CategoryName);