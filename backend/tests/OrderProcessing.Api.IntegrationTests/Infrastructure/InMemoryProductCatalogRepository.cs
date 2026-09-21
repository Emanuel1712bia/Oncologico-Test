using OrderProcessing.Application.Common;
using OrderProcessing.Application.Products.Ports;
using OrderProcessing.Domain.Products;

namespace OrderProcessing.Api.IntegrationTests.Infrastructure;

public sealed class InMemoryProductCatalogRepository : IProductCatalogRepository
{
    public static readonly Product Medication = new(
        Guid.Parse("2a75bc20-0fee-43e5-bc4d-7313e5bf8396"),
        "Abemaciclibe",
        "Comprimidos revestidos: 50 mg, 100 mg, 150 mg, 200 mg",
        "Câncer de mama",
        8900.00m,
        true);

    public static readonly Product InactiveProduct = new(
        Guid.Parse("f0687932-9333-4cf7-8a78-4d855a3c65e5"),
        "Carfilzomibe",
        "Pó para solução injetável - frasco-ampola de 60 mg",
        "Mieloma múltiplo",
        6250.00m,
        false);

    private readonly Dictionary<Guid, Product> _products = new()
    {
        [Medication.Id] = Medication,
        [InactiveProduct.Id] = InactiveProduct,
    };

    public Task<PagedResult<Product>> GetActivePagedAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        var activeProducts = _products.Values.Where(product => product.IsActive).ToList();
        var pageItems = activeProducts.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return Task.FromResult(new PagedResult<Product>(pageItems, page, pageSize, activeProducts.Count));
    }

    public Task<IReadOnlyDictionary<Guid, Product>> GetByIdsAsync(
        IEnumerable<Guid> productIds,
        CancellationToken cancellationToken)
    {
        IReadOnlyDictionary<Guid, Product> matches = productIds
            .Where(id => _products.ContainsKey(id) && _products[id].IsActive)
            .ToDictionary(id => id, id => _products[id]);

        return Task.FromResult(matches);
    }
}
