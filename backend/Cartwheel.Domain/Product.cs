namespace Cartwheel.Domain;

public class Product
{
    public Guid Id { get; }

    public string Name
    {
        get;
        private set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Product name cannot be empty");
            }
            
            field = value;
        }
    }

    public string? Description { get; private set; }

    public decimal Price
    {
        get;
        private set
        {
            if (value <= 0) {
                throw new ArgumentException("Price must be greater than zero");}

            field = value;
        }
    }

    public int StockQuantity
    {
        get;
        private set
        {
            if (value < 0) {
                throw new ArgumentException("Stock quantity can't be negative");}
            
            field = value;
        }
    }

    public Category Category
    {
        get;
        private set => field = value ?? throw new ArgumentNullException("Category cannot be null");
    }

    public Product(
        string name, 
        decimal price,  
        int stockQuantity, 
        Category category,
        string? description = null
        )
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        Price = price;
        StockQuantity = stockQuantity;
        Category = category;
    }
    
    public void SetPrice(decimal newPrice)
    {
        Price = newPrice;
    }
    
    public void SetStockQuantity(int newStockQuantity)
    {
        StockQuantity = newStockQuantity;
    }
}