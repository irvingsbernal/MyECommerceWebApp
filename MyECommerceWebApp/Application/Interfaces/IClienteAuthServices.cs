using MyECommerceWebApp.Application.DTOs.Auth;
using MyECommerceWebApp.Application.DTOs.Clientes;

namespace MyECommerceWebApp.Application.Interfaces;

public interface IClienteService
{
    Task<AuthResponse> RegistrarAsync(RegistrarClienteRequest request, CancellationToken cancellationToken = default);

    Task<ClienteDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}

public interface IAuthService
{
    Task<AuthResponse> IdentificarAsync(IdentificarRequest request, CancellationToken cancellationToken = default);

    AuthResponse LoginAdmin(AdminLoginRequest request);
}

public interface ITokenService
{
    string CreateClienteToken(int clienteId, string email, string nombreCompleto);

    string CreateAdminToken();
}
