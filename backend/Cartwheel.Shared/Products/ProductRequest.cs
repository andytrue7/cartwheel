using System;
using System.ComponentModel.DataAnnotations;

namespace Cartwheel.Shared.Products;

public sealed class CreateProductRequest
{
    [Required]
    [StringLength(200)]
    public string Name { get; init; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; init; }

    // typeof(decimal) with string limits compares as decimal, not double.
    // Invariant culture makes "0.01" parse the same on every machine, whatever its region settings.
    [Range(typeof(decimal), "0.01", "1000000",
        ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true)]
    public decimal Price { get; init; }

    [Range(0, 100_000)]
    public int StockQuantity { get; init; }

    // Nullable on purpose: a missing Guid would otherwise become Guid.Empty and pass [Required].
    [Required]
    public Guid? CategoryId { get; init; }
}

/// <summary>
/// Replaces a product's editable details. Stock is deliberately absent:
/// it only changes through restock and sale operations.
/// </summary>
public sealed class UpdateProductRequest
{
    [Required]
    [StringLength(200)]
    public string Name { get; init; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; init; }

    [Range(typeof(decimal), "0.01", "1000000",
        ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true)]
    public decimal Price { get; init; }

    [Required]
    public Guid? CategoryId { get; init; }
}
