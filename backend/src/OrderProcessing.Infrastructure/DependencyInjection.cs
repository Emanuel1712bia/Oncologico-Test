using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderProcessing.Application.Orders.Ports;
using OrderProcessing.Application.Processing.Ports;
using OrderProcessing.Application.Products.Ports;
using OrderProcessing.Infrastructure.Options;
using OrderProcessing.Infrastructure.Persistence;
using OrderProcessing.Infrastructure.Processing;

namespace OrderProcessing.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

        services.Configure<PostgresOptions>(configuration.GetSection(PostgresOptions.SectionName));
        services.Configure<IntegrationSimulatorOptions>(configuration.GetSection(IntegrationSimulatorOptions.SectionName));

        services.AddSingleton<INpgsqlConnectionFactory, NpgsqlConnectionFactory>();

        services.AddScoped<IProductCatalogRepository, ProductCatalogRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOrderProcessingRepository, OrderProcessingRepository>();

        services.AddScoped<IOrderIntegrationGateway, SimulatedOrderIntegrationGateway>();

        return services;
    }
}
