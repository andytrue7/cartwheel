namespace Cartwheel.Domain;

public class CartItem
{
    public Product Product { get; private set; }

    public int Quantity
    {
        get;
        private set
        {
            if (value <= 0)
            {
                throw new ArgumentException("Quantity must be greater than 0.");
            }
            field = value;
        }
    }

    public decimal LineTotal => Product.Price * Quantity;

    internal CartItem(Product product, int quantity)
    {
        Product = product;
        Quantity = quantity;
    }

    internal void AddQuantity(int quantity)
    {
        Quantity += quantity;
    }

    internal void SetQuantity(int quantity)
    {
        Quantity = quantity;
    }
}