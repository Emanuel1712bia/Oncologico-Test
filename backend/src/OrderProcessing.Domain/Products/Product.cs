namespace OrderProcessing.Domain.Products;

public sealed class Product
{
    public Guid Id { get; }
    public string Name { get; }
    public string Description { get; }
    public string Indication { get; }
    public decimal Price { get; }
    public bool IsActive { get; }

    public Product(Guid id, string name, string description, string indication, decimal price, bool isActive)
    {
        Id = id;
        Name = name;
        Description = description;
        Indication = indication;
        Price = price;
        IsActive = isActive;
    }
}
