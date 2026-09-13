namespace Cartwheel.Domain;

public class Category
{
    public Guid Id { get; }

    public string Name
    {
        get;
        private set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Category name cannot be empty");
            }
            field = value;
        }
    }

    public Category(string name)
    {
        Id = Guid.NewGuid();
        Name = name;
    }
}