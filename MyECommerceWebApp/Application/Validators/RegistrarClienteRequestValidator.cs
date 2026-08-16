using FluentValidation;
using MyECommerceWebApp.Application.DTOs.Clientes;

namespace MyECommerceWebApp.Application.Validators;

public class RegistrarClienteRequestValidator : AbstractValidator<RegistrarClienteRequest>
{
    public RegistrarClienteRequestValidator()
    {
        RuleFor(request => request.Nombre).NotEmpty().MaximumLength(100);
        RuleFor(request => request.Apellido).NotEmpty().MaximumLength(100);
        RuleFor(request => request.Email).NotEmpty().EmailAddress().MaximumLength(255);
        RuleFor(request => request.Direccion).NotEmpty().MaximumLength(300);
        RuleFor(request => request.Telefono).MaximumLength(20);
    }
}
