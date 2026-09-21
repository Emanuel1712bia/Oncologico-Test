using OrderProcessing.Domain.Common;

namespace OrderProcessing.Domain.Orders;

public sealed class EmptyOrderException : DomainException
{
    public EmptyOrderException() : base("An order must contain at least one item.")
    {
    }
}

public sealed class InvalidOrderItemQuantityException : DomainException
{
    public InvalidOrderItemQuantityException(int quantity)
        : base($"Order item quantity must be greater than zero. Received: {quantity}.")
    {
    }
}

public sealed class InvalidOrderStateTransitionException : DomainException
{
    public InvalidOrderStateTransitionException(OrderStatus current, OrderStatus target)
        : base($"Cannot transition order from {current} to {target}.")
    {
    }
}
