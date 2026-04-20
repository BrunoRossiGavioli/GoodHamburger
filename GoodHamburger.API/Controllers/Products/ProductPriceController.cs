using GoodHamburger.API.Services.Products;
using GoodHamburger.Shared.Common;
using GoodHamburger.Shared.Constants;
using GoodHamburger.Shared.DTOs.Products;
using GoodHamburger.Shared.Models.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoodHamburger.API.Controllers.Products;

[ApiController]
[Route("api/product-prices")]
public class ProductPriceController : ControllerBase
{
    private readonly IProductPriceService _productPriceService;

    public ProductPriceController(IProductPriceService productPriceService)
    {
        _productPriceService = productPriceService;
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProductPrice), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid id)
    {
        var price = await _productPriceService.GetAsync(new GetProductPriceDto(id));
        if (price is null)
            return NotFound(new ErrorResponse($"ProductPrice {id} not found."));

        return Ok(price);
    }

    [HttpGet("search")]
    [ProducesResponseType(typeof(IEnumerable<ProductPrice>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Find([FromQuery] FindProductPriceDto dto)
    {
        var prices = await _productPriceService.FindAsync(dto);
        return Ok(prices);
    }

    [HttpPost]
    [Authorize(Roles = Roles.Administrador)]
    [ProducesResponseType(typeof(ProductPrice), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateProductPriceDto dto)
    {
        try
        {
            var price = await _productPriceService.CreateAsync(dto);
            return CreatedAtAction(nameof(Get), new { id = price.Id }, price);
        }
        catch (Exception ex)
        {
            return BadRequest(new ErrorResponse(ex.Message));
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = Roles.Administrador)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _productPriceService.DeleteAsync(new UpdateProductPriceDto(id, Guid.Empty, 0, DateTime.MinValue, null, string.Empty));
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return BadRequest(new ErrorResponse(ex.Message));
        }
    }
}
