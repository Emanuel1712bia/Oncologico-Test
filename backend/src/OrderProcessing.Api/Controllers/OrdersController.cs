using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderProcessing.Application.Common;
using OrderProcessing.Application.Orders.Dtos;
using OrderProcessing.Application.Orders.UseCases;
using OrderProcessing.Application.Security;

namespace OrderProcessing.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/orders")]
public sealed class OrdersController : ControllerBase
{
    private readonly CreateOrderUseCase _createOrderUseCase;
    private readonly GetOrderUseCase _getOrderUseCase;
    private readonly ListOrdersUseCase _listOrdersUseCase;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public OrdersController(
        CreateOrderUseCase createOrderUseCase,
        GetOrderUseCase getOrderUseCase,
        ListOrdersUseCase listOrdersUseCase,
        ICurrentUserAccessor currentUserAccessor)
    {
        _createOrderUseCase = createOrderUseCase;
        _getOrderUseCase = getOrderUseCase;
        _listOrdersUseCase = listOrdersUseCase;
        _currentUserAccessor = currentUserAccessor;
    }

    [HttpPost]
    [ProducesResponseType(typeof(OrderAcceptedResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrderAcceptedResponse>> CreateOrder(
        [FromBody] CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _createOrderUseCase.ExecuteAsync(_currentUserAccessor.UserId, request, cancellationToken);
        return AcceptedAtAction(nameof(GetOrder), new { id = response.OrderId }, response);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderDetailsResponse>> GetOrder(Guid id, CancellationToken cancellationToken)
    {
        var order = await _getOrderUseCase.ExecuteAsync(_currentUserAccessor.UserId, id, cancellationToken);
        return Ok(order);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<OrderDetailsResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<OrderDetailsResponse>>> ListOrders(
        [FromQuery] int page,
        [FromQuery] int pageSize,
        CancellationToken cancellationToken)
    {
        var orders = await _listOrdersUseCase.ExecuteAsync(_currentUserAccessor.UserId, page, pageSize, cancellationToken);
        return Ok(orders);
    }
}
