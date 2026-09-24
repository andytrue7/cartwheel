namespace Cartwheel.Domain.Exceptions;

public class ProductNotFoundException : DomainException
{
    public Guid ProductId { get; }
    
    public ProductNotFoundException(Guid productId) : base($"Product with id: {productId} not found")
    {
        ProductId = productId;
    }
}