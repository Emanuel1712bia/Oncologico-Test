namespace OrderProcessing.Domain.Orders;

public sealed class OrderItem
{
    public Guid Id { get; }
    public Guid ProductId { get; }
    public string ProductName { get; }
    public decimal UnitPrice { get; }
    public int Quantity { get; }
    public decimal LineTotal => UnitPrice * Quantity;

    private OrderItem(Guid id, Guid productId, string productName, decimal unitPrice, int quantity)
    {
        Id = id;
        ProductId = productId;
        ProductName = productName;
        UnitPrice = unitPrice;
        Quantity = quantity;
    }

    public static OrderItem Create(Guid productId, string productName, decimal unitPrice, int quantity)
    {
        if (quantity <= 0)
        {
            throw new InvalidOrderItemQuantityException(quantity);
        }

        return new OrderItem(Guid.NewGuid(), productId, productName, unitPrice, quantity);
    }

    public static OrderItem Restore(Guid id, Guid productId, string productName, decimal unitPrice, int quantity)
    {
        return new OrderItem(id, productId, productName, unitPrice, quantity);
    }
}
