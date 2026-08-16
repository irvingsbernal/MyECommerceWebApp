using MyECommerceWebApp.Application.DTOs.Ordenes;
using MyECommerceWebApp.Application.Exceptions;
using MyECommerceWebApp.Application.Interfaces;
using MyECommerceWebApp.Application.Mappings;
using MyECommerceWebApp.Application.Payments;
using MyECommerceWebApp.Domain.Constants;
using MyECommerceWebApp.Domain.Entities;
using MyECommerceWebApp.Domain.Interfaces;

namespace MyECommerceWebApp.Application.Services;

public class PagoService : IPagoService
{
    private readonly IUnitOfWork _unitOfWork;

    public PagoService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagoDto> ProcesarAsync(
        int ordenId,
        ProcesarPagoRequest request,
        CancellationToken cancellationToken = default)
    {
        var pago = await ProcesarInternoAsync(ordenId, request, cancellationToken);
        return pago.ToDto();
    }

    public async Task<PagoDto> ReintentarAsync(
        int ordenId,
        ProcesarPagoRequest request,
        CancellationToken cancellationToken = default)
    {
        var orden = await GetOrdenAsync(ordenId, cancellationToken);
        var intentos = await _unitOfWork.Pagos.GetMaxIntentosAsync(ordenId, cancellationToken);
        if (intentos >= PaymentSimulator.MaxIntentos)
        {
            throw new BusinessRuleException("Se alcanzo el maximo de reintentos de pago.");
        }

        if (orden.Estado is not (OrdenEstados.Pendiente or OrdenEstados.Rechazada))
        {
            throw new BusinessRuleException("Solo se pueden reintentar pagos de ordenes pendientes o rechazadas.");
        }

        if (orden.Estado == OrdenEstados.Rechazada)
        {
            orden.Estado = OrdenEstados.Pendiente;
            _unitOfWork.Ordenes.Update(orden);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        var pago = await ProcesarInternoAsync(ordenId, request, cancellationToken);
        return pago.ToDto();
    }

    internal async Task<Pago> ProcesarInternoAsync(
        int ordenId,
        ProcesarPagoRequest request,
        CancellationToken cancellationToken)
    {
        var orden = await GetOrdenAsync(ordenId, cancellationToken);
        if (orden.Estado != OrdenEstados.Pendiente)
        {
            throw new BusinessRuleException("Solo se pueden procesar pagos de ordenes pendientes.");
        }

        var intentosActuales = await _unitOfWork.Pagos.GetMaxIntentosAsync(ordenId, cancellationToken);
        if (intentosActuales >= PaymentSimulator.MaxIntentos)
        {
            throw new BusinessRuleException("Se alcanzo el maximo de reintentos de pago.");
        }

        var intento = intentosActuales + 1;
        var estadoPago = PaymentSimulator.Simular(orden.Total, request.Referencia, request.ForzarEstado);

        var pago = new Pago
        {
            OrdenId = ordenId,
            Monto = orden.Total,
            Estado = estadoPago,
            MetodoPago = request.MetodoPago,
            Referencia = request.Referencia,
            Intentos = intento,
            FechaRegistro = DateTime.UtcNow,
            FechaPago = estadoPago == PagoEstados.Autorizado ? DateTime.UtcNow : null,
            MensajeError = estadoPago == PagoEstados.Rechazado ? "Pago rechazado por el procesador." : null
        };

        if (estadoPago == PagoEstados.Autorizado)
        {
            orden.Estado = OrdenEstados.Confirmada;
        }
        else if (estadoPago == PagoEstados.Rechazado)
        {
            orden.Estado = OrdenEstados.Rechazada;
        }

        await _unitOfWork.Pagos.AddAsync(pago, cancellationToken);
        _unitOfWork.Ordenes.Update(orden);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await LogWriter.AddAsync(
            _unitOfWork,
            "Pagos",
            LogOperaciones.Pago,
            pago.PagoId.ToString(),
            $"Pago {estadoPago}. Intento #{intento}",
            cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return pago;
    }

    private async Task<Orden> GetOrdenAsync(int ordenId, CancellationToken cancellationToken)
    {
        var orden = await _unitOfWork.Ordenes.GetByIdAsync(ordenId, cancellationToken);
        if (orden is null)
        {
            throw new NotFoundException("La orden no existe.");
        }

        return orden;
    }
}
