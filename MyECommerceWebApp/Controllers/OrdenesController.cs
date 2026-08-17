using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyECommerceWebApp.Application.DTOs.Ordenes;
using MyECommerceWebApp.Application.Exceptions;
using MyECommerceWebApp.Application.Interfaces;
using MyECommerceWebApp.Domain.Constants;

namespace MyECommerceWebApp.Controllers;

[ApiController]
[Route("api/ordenes")]
[Authorize]
public class OrdenesController : ControllerBase
{
    private readonly IOrdenService _ordenService;
    private readonly IPagoService _pagoService;
    private readonly IInventarioService _inventarioService;

    public OrdenesController(
        IOrdenService ordenService,
        IPagoService pagoService,
        IInventarioService inventarioService)
    {
        _ordenService = ordenService;
        _pagoService = pagoService;
        _inventarioService = inventarioService;
    }

    [HttpPost]
    [Authorize(Policy = "Cliente")]
    [ProducesResponseType(typeof(OrdenEstadoDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<OrdenEstadoDto>> Crear(
        [FromBody] CrearOrdenRequest request,
        CancellationToken cancellationToken)
    {
        EnsureClienteOwns(request.ClienteId);
        var orden = await _ordenService.CrearAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetEstado), new { id = orden.OrdenId }, orden);
    }

    [HttpGet]
    [Authorize(Policy = "Admin")]
    [ProducesResponseType(typeof(IReadOnlyList<OrdenEstadoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<OrdenEstadoDto>>> Listar(
        [FromQuery] string estado = OrdenEstados.Pendiente,
        CancellationToken cancellationToken = default)
    {
        return Ok(await _ordenService.ListarPorEstadoAsync(estado, cancellationToken));
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(OrdenEstadoDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<OrdenEstadoDto>> GetEstado(int id, CancellationToken cancellationToken)
    {
        return Ok(await _ordenService.GetEstadoAsync(id, cancellationToken));
    }

    [HttpPost("{id:int}/pagos")]
    [Authorize(Policy = "Cliente")]
    [ProducesResponseType(typeof(PagoDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagoDto>> ProcesarPago(
        int id,
        [FromBody] ProcesarPagoRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await _pagoService.ProcesarAsync(id, request, cancellationToken));
    }

    [HttpPost("{id:int}/pagos/reintentar")]
    [Authorize(Policy = "Cliente")]
    [ProducesResponseType(typeof(PagoDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagoDto>> ReintentarPago(
        int id,
        [FromBody] ProcesarPagoRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await _pagoService.ReintentarAsync(id, request, cancellationToken));
    }

    [HttpPost("{id:int}/autorizar")]
    [Authorize(Policy = "Admin")]
    [ProducesResponseType(typeof(OrdenEstadoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<OrdenEstadoDto>> Autorizar(int id, CancellationToken cancellationToken)
    {
        return Ok(await _ordenService.AutorizarPendienteAsync(id, cancellationToken));
    }

    [HttpPost("{id:int}/inventario")]
    [Authorize(Policy = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ActualizarInventario(int id, CancellationToken cancellationToken)
    {
        await _inventarioService.ActualizarPorOrdenAsync(id, cancellationToken);
        return NoContent();
    }

    private void EnsureClienteOwns(int clienteId)
    {
        if (User.IsInRole("admin"))
        {
            return;
        }

        var claim = User.FindFirstValue("clienteId") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(claim, out var tokenClienteId) || tokenClienteId != clienteId)
        {
            throw new UnauthorizedOperationException("El cliente autenticado no coincide con la orden.");
        }
    }
}
