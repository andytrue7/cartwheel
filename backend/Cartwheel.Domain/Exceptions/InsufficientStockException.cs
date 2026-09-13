namespace Cartwheel.Domain.Exceptions;

/// <summary>
/// Thrown when an operation would reduce a product's stock quantity below zero.
/// </summary>
public class InsufficientStockException : DomainException
{
    public string ProductName { get; }
    public int RequestedQuantity { get; }
    public int AvailableQuantity { get; }

    public InsufficientStockException(string productName, int requestedQuantity, int availableQuantity)
        : base($"Insufficient stock for '{productName}': requested {requestedQuantity}, but only {availableQuantity} available.")
    {
        ProductName = productName;
        RequestedQuantity = requestedQuantity;
        AvailableQuantity = availableQuantity;
    }
}
