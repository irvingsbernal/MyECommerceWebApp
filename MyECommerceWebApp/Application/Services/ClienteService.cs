using MyECommerceWebApp.Application.DTOs.Auth;
using MyECommerceWebApp.Application.DTOs.Clientes;
using MyECommerceWebApp.Application.Exceptions;
using MyECommerceWebApp.Application.Interfaces;
using MyECommerceWebApp.Application.Mappings;
using MyECommerceWebApp.Domain.Constants;
using MyECommerceWebApp.Domain.Entities;
using MyECommerceWebApp.Domain.Interfaces;

namespace MyECommerceWebApp.Application.Services;

public class ClienteService : IClienteService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;

    public ClienteService(IUnitOfWork unitOfWork, ITokenService tokenService)
    {
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
    }

    public async Task<AuthResponse> RegistrarAsync(
        RegistrarClienteRequest request,
        CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim();
        if (await _unitOfWork.Clientes.ExistsByEmailAsync(email, cancellationToken))
        {
            throw new BusinessRuleException("Ya existe un cliente con ese email.");
        }

        var cliente = new Cliente
        {
            Nombre = request.Nombre.Trim(),
            Apellido = request.Apellido.Trim(),
            Email = email,
            Telefono = string.IsNullOrWhiteSpace(request.Telefono) ? null : request.Telefono.Trim(),
            Direccion = request.Direccion.Trim(),
            FechaRegistro = DateTime.UtcNow,
            Activo = true
        };

        await _unitOfWork.Clientes.AddAsync(cliente, cancellationToken);
        await LogWriter.AddAsync(
            _unitOfWork,
            "Clientes",
            LogOperaciones.Insert,
            null,
            $"Cliente registrado: {cliente.Email}",
            cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var token = _tokenService.CreateClienteToken(
            cliente.ClienteId,
            cliente.Email,
            cliente.Nombre);

        return cliente.ToAuthResponse(token, "cliente");
    }

    public async Task<ClienteDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var cliente = await _unitOfWork.Clientes.GetByIdAsync(id, cancellationToken);
        return cliente?.ToDto();
    }
}
