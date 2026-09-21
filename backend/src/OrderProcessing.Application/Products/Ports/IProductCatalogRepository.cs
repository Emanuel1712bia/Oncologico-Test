using OrderProcessing.Application.Common;
using OrderProcessing.Domain.Products;

namespace OrderProcessing.Application.Products.Ports;

public interface IProductCatalogRepository
{
    Task<PagedResult<Product>> GetActivePagedAsync(int page, int pageSize, CancellationToken cancellationToken);

    Task<IReadOnlyDictionary<Guid, Product>> GetByIdsAsync(IEnumerable<Guid> productIds, CancellationToken cancellationToken);
}
