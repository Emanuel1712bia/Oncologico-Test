namespace OrderProcessing.Domain.Orders;

public sealed class Order
{
    private readonly List<OrderItem> _items;

    public Guid Id { get; }
    public string UserId { get; }
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; }
    public DateTime UpdatedAtUtc { get; private set; }
    public IReadOnlyCollection<OrderItem> Items => _items;
    public decimal TotalAmount => _items.Sum(item => item.LineTotal);

    private Order(
        Guid id,
        string userId,
        OrderStatus status,
        DateTime createdAtUtc,
        DateTime updatedAtUtc,
        List<OrderItem> items)
    {
        Id = id;
        UserId = userId;
        Status = status;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc;
        _items = items;
    }

    public static Order Create(string userId, IReadOnlyCollection<OrderItem> items)
    {
        if (items.Count == 0)
        {
            throw new EmptyOrderException();
        }

        var now = DateTime.UtcNow;
        return new Order(Guid.NewGuid(), userId, OrderStatus.Pending, now, now, items.ToList());
    }

    public static Order Restore(
        Guid id,
        string userId,
        OrderStatus status,
        DateTime createdAtUtc,
        DateTime updatedAtUtc,
        IReadOnlyCollection<OrderItem> items)
    {
        return new Order(id, userId, status, createdAtUtc, updatedAtUtc, items.ToList());
    }

    public void StartProcessing()
    {
        EnsureTransitionIsAllowed(OrderStatus.Processing);
        Status = OrderStatus.Processing;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Complete()
    {
        EnsureTransitionIsAllowed(OrderStatus.Completed);
        Status = OrderStatus.Completed;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Fail()
    {
        EnsureTransitionIsAllowed(OrderStatus.Failed);
        Status = OrderStatus.Failed;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    private void EnsureTransitionIsAllowed(OrderStatus target)
    {
        var isAllowed = (Status, target) switch
        {
            (OrderStatus.Pending, OrderStatus.Processing) => true,
            (OrderStatus.Processing, OrderStatus.Completed) => true,
            (OrderStatus.Processing, OrderStatus.Failed) => true,
            _ => false
        };

        if (!isAllowed)
        {
            throw new InvalidOrderStateTransitionException(Status, target);
        }
    }
}
