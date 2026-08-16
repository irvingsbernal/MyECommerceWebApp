using FluentValidation;
using MyECommerceWebApp.Application.DTOs.Ordenes;

namespace MyECommerceWebApp.Application.Validators;

public class CrearOrdenRequestValidator : AbstractValidator<CrearOrdenRequest>
{
    public CrearOrdenRequestValidator()
    {
        RuleFor(request => request.ClienteId).GreaterThan(0);
        RuleFor(request => request.Lineas).NotEmpty().WithMessage("La orden debe tener al menos un producto.");
        RuleForEach(request => request.Lineas).SetValidator(new LineaOrdenRequestValidator());
        RuleFor(request => request.Lineas)
            .Must(lineas => lineas.Select(linea => linea.ProductoId).Distinct().Count() == lineas.Count)
            .WithMessage("No se permiten productos duplicados en la misma orden.");
    }
}

public class LineaOrdenRequestValidator : AbstractValidator<LineaOrdenRequest>
{
    public LineaOrdenRequestValidator()
    {
        RuleFor(linea => linea.ProductoId).GreaterThan(0);
        RuleFor(linea => linea.Cantidad).GreaterThan(0);
    }
}

public class ProcesarPagoRequestValidator : AbstractValidator<ProcesarPagoRequest>
{
    public ProcesarPagoRequestValidator()
    {
        RuleFor(request => request.MetodoPago).NotEmpty().MaximumLength(50);
        RuleFor(request => request.Referencia).MaximumLength(100);
        RuleFor(request => request.ForzarEstado)
            .Must(estado => estado is null || estado is "autorizado" or "rechazado" or "pendiente")
            .WithMessage("ForzarEstado debe ser autorizado, rechazado o pendiente.");
    }
}

public class ProcesarCompraRequestValidator : AbstractValidator<ProcesarCompraRequest>
{
    public ProcesarCompraRequestValidator()
    {
        RuleFor(request => request.ClienteId).GreaterThan(0);
        RuleFor(request => request.Lineas).NotEmpty();
        RuleForEach(request => request.Lineas).SetValidator(new LineaOrdenRequestValidator());
        RuleFor(request => request.MetodoPago).NotEmpty().MaximumLength(50);
        RuleFor(request => request.Referencia).MaximumLength(100);
    }
}
