using System.ComponentModel.DataAnnotations;

namespace Cartwheel.Shared.Products;

/// <summary>
/// Body for the stock increase and decrease endpoints. The URL says the direction,
/// so the quantity is always positive.
/// </summary>
public sealed class StockChangeRequest
{
    [Range(1, 100_000)]
    public int Quantity { get; init; }
}
