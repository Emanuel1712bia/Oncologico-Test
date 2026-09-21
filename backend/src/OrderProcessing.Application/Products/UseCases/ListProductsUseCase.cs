using OrderProcessing.Application.Common;
using OrderProcessing.Application.Products.Dtos;
using OrderProcessing.Application.Products.Ports;

namespace OrderProcessing.Application.Products.UseCases;

public sealed class ListProductsUseCase
{
    private readonly IProductCatalogRepository _productCatalogRepository;

    public ListProductsUseCase(IProductCatalogRepository productCatalogRepository)
    {
        _productCatalogRepository = productCatalogRepository;
    }

    public async Task<PagedResult<ProductResponse>> ExecuteAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        var normalizedPage = page < 1 ? 1 : page;
        var normalizedPageSize = pageSize is < 1 or > 100 ? 20 : pageSize;

        var pagedProducts = await _productCatalogRepository.GetActivePagedAsync(normalizedPage, normalizedPageSize, cancellationToken);

        var items = pagedProducts.Items
            .Select(product => new ProductResponse(product.Id, product.Name, product.Description, product.Indication, product.Price))
            .ToList();

        return new PagedResult<ProductResponse>(items, pagedProducts.Page, pagedProducts.PageSize, pagedProducts.TotalCount);
    }
}
