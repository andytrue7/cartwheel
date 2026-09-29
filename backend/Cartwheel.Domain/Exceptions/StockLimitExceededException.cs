namespace Cartwheel.Domain.Exceptions;

/// <summary>
/// Thrown when an operation would raise a product's stock quantity above <see cref="Product.MaxStockQuantity"/>.
/// </summary>
public class StockLimitExceededException : DomainException
{
    public string ProductName { get; }
    public int RequestedQuantity { get; }
    public int CurrentQuantity { get; }
    public int MaxQuantity { get; }

    public StockLimitExceededException(string productName, int requestedQuantity, int currentQuantity, int maxQuantity)
        : base($"Cannot add {requestedQuantity} to '{productName}': stock is {currentQuantity} and the maximum is {maxQuantity}.")
    {
        ProductName = productName;
        RequestedQuantity = requestedQuantity;
        CurrentQuantity = currentQuantity;
        MaxQuantity = maxQuantity;
    }
}
