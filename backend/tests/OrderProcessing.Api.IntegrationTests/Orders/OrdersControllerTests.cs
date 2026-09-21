using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using OrderProcessing.Api.IntegrationTests.Infrastructure;
using OrderProcessing.Application.Common;
using OrderProcessing.Application.Orders.Dtos;

namespace OrderProcessing.Api.IntegrationTests.Orders;

public sealed class OrdersControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _client;

    public OrdersControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateOrder_Should_ReturnAccepted_When_RequestIsValid()
    {
        var request = new CreateOrderRequest(new[]
        {
            new CreateOrderItemRequest(InMemoryProductCatalogRepository.Medication.Id, 2)
        });

        var response = await _client.PostAsJsonAsync("/api/orders", request);

        response.StatusCode.Should().Be(HttpStatusCode.Accepted);

        var accepted = await response.Content.ReadFromJsonAsync<OrderAcceptedResponse>(JsonOptions);
        accepted!.Status.Should().Be("Pending");

        var getResponse = await _client.GetAsync($"/api/orders/{accepted.OrderId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var order = await getResponse.Content.ReadFromJsonAsync<OrderDetailsResponse>(JsonOptions);
        order!.TotalAmount.Should().Be(InMemoryProductCatalogRepository.Medication.Price * 2);
    }

    [Fact]
    public async Task CreateOrder_Should_ReturnValidationProblemDetails_When_ItemsIsEmpty()
    {
        var request = new CreateOrderRequest(Array.Empty<CreateOrderItemRequest>());

        var response = await _client.PostAsJsonAsync("/api/orders", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateOrder_Should_ReturnValidationProblemDetails_When_ProductIsInactive()
    {
        var request = new CreateOrderRequest(new[]
        {
            new CreateOrderItemRequest(InMemoryProductCatalogRepository.InactiveProduct.Id, 1)
        });

        var response = await _client.PostAsJsonAsync("/api/orders", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetOrder_Should_ReturnNotFound_When_OrderDoesNotExist()
    {
        var response = await _client.GetAsync($"/api/orders/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ListOrders_Should_ReturnPagedResult()
    {
        var response = await _client.GetAsync("/api/orders?page=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var paged = await response.Content.ReadFromJsonAsync<PagedResult<OrderDetailsResponse>>(JsonOptions);
        paged.Should().NotBeNull();
    }
}
