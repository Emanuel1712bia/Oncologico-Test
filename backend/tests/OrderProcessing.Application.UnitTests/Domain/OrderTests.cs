using FluentAssertions;
using OrderProcessing.Domain.Orders;

namespace OrderProcessing.Application.UnitTests.Domain;

public sealed class OrderTests
{
    [Fact]
    public void Create_Should_Throw_When_ThereAreNoItems()
    {
        var act = () => Order.Create("user-1", Array.Empty<OrderItem>());

        act.Should().Throw<EmptyOrderException>();
    }

    [Fact]
    public void Create_Should_ComputeTotalAmount_From_ItemLineTotals()
    {
        var items = new[]
        {
            OrderItem.Create(Guid.NewGuid(), "Notebook", 1000m, 2),
            OrderItem.Create(Guid.NewGuid(), "Mouse", 50m, 3),
        };

        var order = Order.Create("user-1", items);

        order.TotalAmount.Should().Be(2150m);
        order.Status.Should().Be(OrderStatus.Pending);
    }

    [Fact]
    public void Complete_Should_Throw_When_OrderIsStillPending()
    {
        var order = Order.Create("user-1", new[] { OrderItem.Create(Guid.NewGuid(), "Notebook", 1000m, 1) });

        var act = () => order.Complete();

        act.Should().Throw<InvalidOrderStateTransitionException>();
    }

    [Fact]
    public void Fail_Should_Succeed_When_OrderIsProcessing()
    {
        var order = Order.Create("user-1", new[] { OrderItem.Create(Guid.NewGuid(), "Notebook", 1000m, 1) });
        order.StartProcessing();

        order.Fail();

        order.Status.Should().Be(OrderStatus.Failed);
    }
}
