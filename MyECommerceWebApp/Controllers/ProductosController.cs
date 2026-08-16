using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyECommerceWebApp.Application.DTOs.Productos;
using MyECommerceWebApp.Application.Interfaces;

namespace MyECommerceWebApp.Controllers;

[ApiController]
[Route("api/productos")]
public class ProductosController : ControllerBase
{
    private readonly IProductoService _productoService;

    public ProductosController(IProductoService productoService)
    {
        _productoService = productoService;
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<ProductoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductoDto>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await _productoService.GetAllAsync(cancellationToken));
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ProductoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductoDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var producto = await _productoService.GetByIdAsync(id, cancellationToken);
        return producto is null ? NotFound() : Ok(producto);
    }

    [HttpPost]
    [Authorize(Policy = "Admin")]
    [ProducesResponseType(typeof(ProductoDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<ProductoDto>> Create(
        [FromBody] CreateProductoRequest request,
        CancellationToken cancellationToken)
    {
        var producto = await _productoService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = producto.ProductoId }, producto);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "Admin")]
    [ProducesResponseType(typeof(ProductoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductoDto>> Update(
        int id,
        [FromBody] UpdateProductoRequest request,
        CancellationToken cancellationToken)
    {
        var producto = await _productoService.UpdateAsync(id, request, cancellationToken);
        return producto is null ? NotFound() : Ok(producto);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await _productoService.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
