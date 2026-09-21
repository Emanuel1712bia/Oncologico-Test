using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderProcessing.Application.Common;
using OrderProcessing.Application.Products.Dtos;
using OrderProcessing.Application.Products.UseCases;

namespace OrderProcessing.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/products")]
public sealed class ProductsController : ControllerBase
{
    private readonly ListProductsUseCase _listProductsUseCase;

    public ProductsController(ListProductsUseCase listProductsUseCase)
    {
        _listProductsUseCase = listProductsUseCase;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ProductResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ProductResponse>>> GetProducts(
        [FromQuery] int page,
        [FromQuery] int pageSize,
        CancellationToken cancellationToken)
    {
        var products = await _listProductsUseCase.ExecuteAsync(page == 0 ? 1 : page, pageSize == 0 ? 20 : pageSize, cancellationToken);
        return Ok(products);
    }
}
