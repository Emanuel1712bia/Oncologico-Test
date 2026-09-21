using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using OrderProcessing.Application.Orders.Dtos;
using OrderProcessing.Application.Orders.UseCases;
using OrderProcessing.Application.Orders.Validation;
using OrderProcessing.Application.Processing.UseCases;
using OrderProcessing.Application.Products.UseCases;

namespace OrderProcessing.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IValidator<CreateOrderRequest>, CreateOrderRequestValidator>();

        services.AddScoped<CreateOrderUseCase>();
        services.AddScoped<GetOrderUseCase>();
        services.AddScoped<ListOrdersUseCase>();
        services.AddScoped<ListProductsUseCase>();
        services.AddScoped<ProcessNextPendingOrderUseCase>();

        return services;
    }
}
