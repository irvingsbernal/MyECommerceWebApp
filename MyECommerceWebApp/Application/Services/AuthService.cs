using Microsoft.Extensions.Configuration;
using MyECommerceWebApp.Application.DTOs.Auth;
using MyECommerceWebApp.Application.Exceptions;
using MyECommerceWebApp.Application.Interfaces;
using MyECommerceWebApp.Application.Mappings;
using MyECommerceWebApp.Domain.Interfaces;

namespace MyECommerceWebApp.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _configuration;

    public AuthService(IUnitOfWork unitOfWork, ITokenService tokenService, IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
        _configuration = configuration;
    }

    public async Task<AuthResponse> IdentificarAsync(
        IdentificarRequest request,
        CancellationToken cancellationToken = default)
    {
        var cliente = await _unitOfWork.Clientes.GetByEmailAsync(request.Email.Trim(), cancellationToken);
        if (cliente is null || !cliente.Activo)
        {
            throw new NotFoundException("Cliente no existe o esta inactivo.");
        }

        var token = _tokenService.CreateClienteToken(
            cliente.ClienteId,
            cliente.Email,
            $"{cliente.Nombre} {cliente.Apellido}");

        return cliente.ToAuthResponse(token, "cliente");
    }

    public AuthResponse LoginAdmin(AdminLoginRequest request)
    {
        var expected = _configuration["DemoAdmin:Key"] ?? "demo-admin";
        if (!string.Equals(request.DemoKey, expected, StringComparison.Ordinal))
        {
            throw new UnauthorizedOperationException("Clave de administrador invalida.");
        }

        return new AuthResponse
        {
            Token = _tokenService.CreateAdminToken(),
            Role = "admin",
            Email = _configuration["DemoAdmin:Email"] ?? "admin@ecommerce.local",
            NombreCompleto = "Administrador demo"
        };
    }
}
