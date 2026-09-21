using FluentAssertions;
using Moq;
using OrderProcessing.Application.Common;
using OrderProcessing.Application.Orders.Dtos;
using OrderProcessing.Application.Orders.Ports;
using OrderProcessing.Application.Orders.UseCases;
using OrderProcessing.Application.Orders.Validation;
using OrderProcessing.Application.Products.Ports;
using OrderProcessing.Domain.Orders;
using OrderProcessing.Domain.Products;

namespace OrderProcessing.Application.UnitTests.Orders;

public sealed class CreateOrderUseCaseTests
{
    private readonly Mock<IProductCatalogRepository> _productCatalogRepository = new();
    private readonly Mock<IOrderRepository> _orderRepository = new();
    private readonly CreateOrderUseCase _useCase;

    public CreateOrderUseCaseTests()
    {
        _useCase = new CreateOrderUseCase(
            new CreateOrderRequestValidator(),
            _productCatalogRepository.Object,
            _orderRepository.Object);
    }

    [Fact]
    public async Task ExecuteAsync_Should_PersistOrder_When_RequestIsValid()
    {
        var product = new Product(Guid.NewGuid(), "Abemaciclibe", "Comprimidos revestidos: 150 mg", "Câncer de mama", 8900.00m, true);
        _productCatalogRepository
            .Setup(repository => repository.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, Product> { [product.Id] = product });

        var request = new CreateOrderRequest(new[] { new CreateOrderItemRequest(product.Id, 2) });

        var response = await _useCase.ExecuteAsync("user-1", request, CancellationToken.None);

        response.Status.Should().Be(nameof(OrderStatus.Pending));
        _orderRepository.Verify(
            repository => repository.CreateAsync(
                It.Is<Order>(order =>
                    order.UserId == "user-1" &&
                    order.Items.Count == 1 &&
                    order.Items.Single().Quantity == 2 &&
                    order.Items.Single().UnitPrice == product.Price),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_Should_ThrowValidationAppException_When_ItemsIsEmpty()
    {
        var request = new CreateOrderRequest(Array.Empty<CreateOrderItemRequest>());

        var act = async () => await _useCase.ExecuteAsync("user-1", request, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationAppException>();
        _orderRepository.Verify(
            repository => repository.CreateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_Should_ThrowValidationAppException_When_QuantityIsInvalid()
    {
        var request = new CreateOrderRequest(new[] { new CreateOrderItemRequest(Guid.NewGuid(), 0) });

        var act = async () => await _useCase.ExecuteAsync("user-1", request, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationAppException>();
        _orderRepository.Verify(
            repository => repository.CreateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_Should_ThrowValidationAppException_When_ProductIsNotInCatalog()
    {
        _productCatalogRepository
            .Setup(repository => repository.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, Product>());

        var request = new CreateOrderRequest(new[] { new CreateOrderItemRequest(Guid.NewGuid(), 1) });

        var act = async () => await _useCase.ExecuteAsync("user-1", request, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationAppException>();
        _orderRepository.Verify(
            repository => repository.CreateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
