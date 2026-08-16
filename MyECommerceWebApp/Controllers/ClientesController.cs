using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyECommerceWebApp.Application.DTOs.Auth;
using MyECommerceWebApp.Application.DTOs.Clientes;
using MyECommerceWebApp.Application.Interfaces;

namespace MyECommerceWebApp.Controllers;

[ApiController]
[Route("api/clientes")]
public class ClientesController : ControllerBase
{
    private readonly IClienteService _clienteService;

    public ClientesController(IClienteService clienteService)
    {
        _clienteService = clienteService;
    }

    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<AuthResponse>> Registrar(
        [FromBody] RegistrarClienteRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _clienteService.RegistrarAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.ClienteId }, result);
    }

    [HttpGet("{id:int}")]
    [Authorize]
    [ProducesResponseType(typeof(ClienteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClienteDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var cliente = await _clienteService.GetByIdAsync(id, cancellationToken);
        return cliente is null ? NotFound() : Ok(cliente);
    }
}
