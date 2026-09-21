using FluentAssertions;
using OrderProcessing.Domain.Orders;

namespace OrderProcessing.Application.UnitTests.Domain;

public sealed class OrderItemTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_Should_Throw_When_QuantityIsNotPositive(int quantity)
    {
        var act = () => OrderItem.Create(Guid.NewGuid(), "Notebook", 1000m, quantity);

        act.Should().Throw<InvalidOrderItemQuantityException>();
    }

    [Fact]
    public void LineTotal_Should_Be_UnitPrice_Times_Quantity()
    {
        var item = OrderItem.Create(Guid.NewGuid(), "Notebook", 1000m, 3);

        item.LineTotal.Should().Be(3000m);
    }
}
