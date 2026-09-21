using FluentValidation;
using OrderProcessing.Application.Common;
using OrderProcessing.Application.Orders.Dtos;
using OrderProcessing.Application.Orders.Ports;
using OrderProcessing.Application.Products.Ports;
using OrderProcessing.Domain.Orders;
using OrderProcessing.Domain.Products;

namespace OrderProcessing.Application.Orders.UseCases;

public sealed class CreateOrderUseCase
{
    private readonly IValidator<CreateOrderRequest> _validator;
    private readonly IProductCatalogRepository _productCatalogRepository;
    private readonly IOrderRepository _orderRepository;

    public CreateOrderUseCase(
        IValidator<CreateOrderRequest> validator,
        IProductCatalogRepository productCatalogRepository,
        IOrderRepository orderRepository)
    {
        _validator = validator;
        _productCatalogRepository = productCatalogRepository;
        _orderRepository = orderRepository;
    }

    public async Task<OrderAcceptedResponse> ExecuteAsync(
        string userId,
        CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        (await _validator.ValidateAsync(request, cancellationToken)).ThrowIfInvalid();

        var requestedProductIds = request.Items.Select(item => item.ProductId).Distinct().ToList();
        var catalog = await _productCatalogRepository.GetByIdsAsync(requestedProductIds, cancellationToken);

        EnsureAllProductsAreAvailable(requestedProductIds, catalog);

        var orderItems = request.Items
            .Select(item =>
            {
                var product = catalog[item.ProductId];
                return OrderItem.Create(product.Id, product.Name, product.Price, item.Quantity);
            })
            .ToList();

        var order = Order.Create(userId, orderItems);

        await _orderRepository.CreateAsync(order, cancellationToken);

        return new OrderAcceptedResponse(order.Id, order.Status.ToString());
    }

    private static void EnsureAllProductsAreAvailable(
        IReadOnlyCollection<Guid> requestedProductIds,
        IReadOnlyDictionary<Guid, Product> catalog)
    {
        var unavailableProductIds = requestedProductIds
            .Where(productId => !catalog.ContainsKey(productId))
            .Select(productId => productId.ToString())
            .ToArray();

        if (unavailableProductIds.Length > 0)
        {
            throw new ValidationAppException(new Dictionary<string, string[]>
            {
                ["items"] = new[]
                {
                    $"The following products are not available in the catalog: {string.Join(", ", unavailableProductIds)}."
                }
            });
        }
    }
}
