using Dapper;
using OrderProcessing.Application.Common;
using OrderProcessing.Application.Products.Ports;
using OrderProcessing.Domain.Products;

namespace OrderProcessing.Infrastructure.Persistence;

internal sealed class ProductCatalogRepository : IProductCatalogRepository
{
    private readonly INpgsqlConnectionFactory _connectionFactory;

    public ProductCatalogRepository(INpgsqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<PagedResult<Product>> GetActivePagedAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        const string countSql = "SELECT COUNT(*) FROM products WHERE is_active = true";

        const string pageSql = """
            SELECT id, name, description, indication, price, is_active
            FROM products
            WHERE is_active = true
            ORDER BY name
            OFFSET @Offset LIMIT @PageSize
            """;

        using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);

        var totalCount = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(countSql, cancellationToken: cancellationToken));

        var rows = await connection.QueryAsync<ProductRow>(new CommandDefinition(
            pageSql,
            new { Offset = (page - 1) * pageSize, PageSize = pageSize },
            cancellationToken: cancellationToken));

        var products = rows.Select(ToDomain).ToList();

        return new PagedResult<Product>(products, page, pageSize, totalCount);
    }

    public async Task<IReadOnlyDictionary<Guid, Product>> GetByIdsAsync(
        IEnumerable<Guid> productIds,
        CancellationToken cancellationToken)
    {
        var idsArray = productIds.ToArray();
        if (idsArray.Length == 0)
        {
            return new Dictionary<Guid, Product>();
        }

        const string sql = """
            SELECT id, name, description, indication, price, is_active
            FROM products
            WHERE id = ANY(@ProductIds) AND is_active = true
            """;

        using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<ProductRow>(
            new CommandDefinition(sql, new { ProductIds = idsArray }, cancellationToken: cancellationToken));

        return rows.Select(ToDomain).ToDictionary(product => product.Id);
    }

    private static Product ToDomain(ProductRow row)
    {
        return new Product(row.Id, row.Name, row.Description, row.Indication, row.Price, row.IsActive);
    }

    private sealed record ProductRow(Guid Id, string Name, string Description, string Indication, decimal Price, bool IsActive);
}
