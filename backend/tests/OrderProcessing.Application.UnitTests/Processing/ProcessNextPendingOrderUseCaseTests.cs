using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using OrderProcessing.Application.Processing.Ports;
using OrderProcessing.Application.Processing.UseCases;
using OrderProcessing.Domain.Orders;
using OrderProcessing.Domain.Processing;

namespace OrderProcessing.Application.UnitTests.Processing;

public sealed class ProcessNextPendingOrderUseCaseTests
{
    private readonly Mock<IOrderProcessingRepository> _orderProcessingRepository = new();
    private readonly Mock<IOrderIntegrationGateway> _orderIntegrationGateway = new();
    private readonly ProcessNextPendingOrderUseCase _useCase;

    public ProcessNextPendingOrderUseCaseTests()
    {
        _useCase = new ProcessNextPendingOrderUseCase(
            _orderProcessingRepository.Object,
            _orderIntegrationGateway.Object,
            NullLogger<ProcessNextPendingOrderUseCase>.Instance);
    }

    [Fact]
    public async Task ExecuteAsync_Should_ReturnFalse_When_ThereIsNoPendingOrder()
    {
        _orderProcessingRepository
            .Setup(repository => repository.ClaimNextPendingOrderAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrderClaim?)null);

        var processed = await _useCase.ExecuteAsync(CancellationToken.None);

        processed.Should().BeFalse();
        _orderIntegrationGateway.Verify(
            gateway => gateway.ProcessAsync(It.IsAny<OrderIntegrationRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_Should_FinalizeAsSucceeded_When_IntegrationSucceeds()
    {
        var claim = CreateClaim();
        _orderProcessingRepository
            .Setup(repository => repository.ClaimNextPendingOrderAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(claim);
        _orderIntegrationGateway
            .Setup(gateway => gateway.ProcessAsync(It.IsAny<OrderIntegrationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrderIntegrationResult(true, null));

        var processed = await _useCase.ExecuteAsync(CancellationToken.None);

        processed.Should().BeTrue();
        _orderProcessingRepository.Verify(
            repository => repository.FinalizeAttemptAsync(
                It.Is<OrderProcessingAttempt>(attempt =>
                    attempt.OrderId == claim.Order.Id &&
                    attempt.AttemptNumber == claim.AttemptNumber &&
                    attempt.Success),
                true,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_Should_FinalizeAsFailed_When_IntegrationFails()
    {
        var claim = CreateClaim();
        _orderProcessingRepository
            .Setup(repository => repository.ClaimNextPendingOrderAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(claim);
        _orderIntegrationGateway
            .Setup(gateway => gateway.ProcessAsync(It.IsAny<OrderIntegrationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrderIntegrationResult(false, "Simulated external integration failure."));

        var processed = await _useCase.ExecuteAsync(CancellationToken.None);

        processed.Should().BeTrue();
        _orderProcessingRepository.Verify(
            repository => repository.FinalizeAttemptAsync(
                It.Is<OrderProcessingAttempt>(attempt =>
                    attempt.OrderId == claim.Order.Id &&
                    !attempt.Success &&
                    attempt.ErrorMessage == "Simulated external integration failure."),
                false,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static OrderClaim CreateClaim()
    {
        var items = new[] { OrderItem.Create(Guid.NewGuid(), "Notebook", 6499.90m, 1) };
        var order = Order.Create("user-1", items);
        order.StartProcessing();

        return new OrderClaim(order, AttemptNumber: 1);
    }
}
