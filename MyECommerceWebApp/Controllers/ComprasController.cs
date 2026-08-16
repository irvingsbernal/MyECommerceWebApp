using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyECommerceWebApp.Application.DTOs.Ordenes;
using MyECommerceWebApp.Application.Exceptions;
using MyECommerceWebApp.Application.Interfaces;

namespace MyECommerceWebApp.Controllers;

[ApiController]
[Route("api/compras")]
[Authorize(Policy = "Cliente")]
public class ComprasController : ControllerBase
{
    private readonly ICompraService _compraService;

    public ComprasController(ICompraService compraService)
    {
        _compraService = compraService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(CompraResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CompraResultDto>> Procesar(
        [FromBody] ProcesarCompraRequest request,
        CancellationToken cancellationToken)
    {
        var claim = User.FindFirstValue("clienteId") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(claim, out var tokenClienteId) || tokenClienteId != request.ClienteId)
        {
            throw new UnauthorizedOperationException("El cliente autenticado no coincide con la compra.");
        }

        return Ok(await _compraService.ProcesarAsync(request, cancellationToken));
    }
}
