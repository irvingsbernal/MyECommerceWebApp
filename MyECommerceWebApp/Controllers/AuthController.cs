using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyECommerceWebApp.Application.DTOs.Auth;
using MyECommerceWebApp.Application.Interfaces;

namespace MyECommerceWebApp.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("identificar")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<AuthResponse>> Identificar(
        [FromBody] IdentificarRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await _authService.IdentificarAsync(request, cancellationToken));
    }

    [HttpPost("admin")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    public ActionResult<AuthResponse> Admin([FromBody] AdminLoginRequest request)
    {
        return Ok(_authService.LoginAdmin(request));
    }
}
