using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using MyECommerceWebApp.Application.Interfaces;
using MyECommerceWebApp.Application.Services;

namespace MyECommerceWebApp.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<Validators.RegistrarClienteRequestValidator>();

        services.AddScoped<IClienteService, ClienteService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IProductoService, ProductoService>();
        services.AddScoped<OrdenService>();
        services.AddScoped<IOrdenService>(provider => provider.GetRequiredService<OrdenService>());
        services.AddScoped<PagoService>();
        services.AddScoped<IPagoService>(provider => provider.GetRequiredService<PagoService>());
        services.AddScoped<InventarioService>();
        services.AddScoped<IInventarioService>(provider => provider.GetRequiredService<InventarioService>());
        services.AddScoped<ICompraService, CompraService>();
        services.AddScoped<ILogService, LogService>();

        return services;
    }
}
