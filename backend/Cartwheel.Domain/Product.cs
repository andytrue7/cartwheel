using Cartwheel.Domain.Exceptions;

namespace Cartwheel.Domain;

public class Product
{
    /// <summary>
    /// Upper bound for stock. Far below <see cref="int.MaxValue"/>, so stock arithmetic can never overflow.
    /// </summary>
    public const int MaxStockQuantity = 1_000_000;

    public Guid Id { get; }

    public string Name
    {
        get;
        private set => field = ValidateName(value);
    }

    public string? Description { get; private set; }

    public decimal Price
    {
        get;
        private set => field = ValidatePrice(value);
    }

    public int StockQuantity
    {
        get;
        private set
        {
            if (value < 0)
            {
                throw new ArgumentException("Stock quantity can't be negative");
            }

            if (value > MaxStockQuantity)
            {
                throw new ArgumentException($"Stock quantity can't exceed {MaxStockQuantity}");
            }

            field = value;
        }
    }

    public Category Category
    {
        get;
        private set => field = ValidateCategory(value);
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
    
    public void IncreaseStock(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));
        }

        // Compare against the room left, not StockQuantity + quantity: the sum itself could overflow.
        if (quantity > MaxStockQuantity - StockQuantity)
        {
            throw new StockLimitExceededException(Name, quantity, StockQuantity, MaxStockQuantity);
        }

        StockQuantity += quantity;
    }

    public void DecreaseStock(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));
        }

        if (quantity > StockQuantity)
        {
            throw new InsufficientStockException(Name, quantity, StockQuantity);
        }

        StockQuantity -= quantity;
    }
    
    public void Rename(string newName) => Name = newName;
    
    public void ChangeDescription(string? newDescription) => Description = newDescription;
    
    public void ChangeCategory(Category newCategory) => Category = newCategory;

    /// <summary>
    /// Changes all editable details at once. Every value is validated before anything is assigned,
    /// so a failed update never leaves the product half-changed. Stock is not editable here.
    /// </summary>
    public void UpdateDetails(string name, string? description, decimal price, Category category)
    {
        ValidateName(name);
        ValidatePrice(price);
        ValidateCategory(category);

        Name = name;
        Description = description;
        Price = price;
        Category = category;
    }

    private static string ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Product name cannot be empty");
        }

        return name;
    }

    private static decimal ValidatePrice(decimal price)
    {
        if (price <= 0)
        {
            throw new ArgumentException("Price must be greater than zero");
        }

        return price;
    }

    private static Category ValidateCategory(Category category) =>
        category ?? throw new ArgumentNullException(nameof(category));
}
