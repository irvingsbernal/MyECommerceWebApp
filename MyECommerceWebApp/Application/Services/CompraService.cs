using MyECommerceWebApp.Application.DTOs.Ordenes;
using MyECommerceWebApp.Application.Exceptions;
using MyECommerceWebApp.Application.Interfaces;
using MyECommerceWebApp.Application.Payments;
using MyECommerceWebApp.Domain.Constants;
using MyECommerceWebApp.Domain.Interfaces;

namespace MyECommerceWebApp.Application.Services;

public class CompraService : ICompraService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly OrdenService _ordenService;
    private readonly PagoService _pagoService;
    private readonly InventarioService _inventarioService;

    public CompraService(
        IUnitOfWork unitOfWork,
        OrdenService ordenService,
        PagoService pagoService,
        InventarioService inventarioService)
    {
        _unitOfWork = unitOfWork;
        _ordenService = ordenService;
        _pagoService = pagoService;
        _inventarioService = inventarioService;
    }

    public async Task<CompraResultDto> ProcesarAsync(
        ProcesarCompraRequest request,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var orden = await _ordenService.CrearInternoAsync(
                request.ClienteId,
                request.Lineas,
                request.Observaciones,
                cancellationToken);

            PagoDto? ultimoPago = null;
            var pagoExitoso = false;

            for (var intento = 1; intento <= PaymentSimulator.MaxIntentos && !pagoExitoso; intento++)
            {
                var pagoRequest = new ProcesarPagoRequest
                {
                    MetodoPago = request.MetodoPago,
                    Referencia = request.Referencia
                };

                var pago = await _pagoService.ProcesarInternoAsync(orden.OrdenId, pagoRequest, cancellationToken);
                ultimoPago = new PagoDto
                {
                    PagoId = pago.PagoId,
                    Estado = pago.Estado,
                    Intentos = pago.Intentos,
                    Monto = pago.Monto,
                    MetodoPago = pago.MetodoPago
                };

                if (pago.Estado == PagoEstados.Autorizado)
                {
                    pagoExitoso = true;
                }
                else if (pago.Estado == PagoEstados.Pendiente)
                {
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);
                    return new CompraResultDto
                    {
                        OrdenId = orden.OrdenId,
                        EstadoOrden = OrdenEstados.Pendiente,
                        EstadoPago = PagoEstados.Pendiente,
                        Resultado = "Pago pendiente de revision."
                    };
                }
                else if (intento < PaymentSimulator.MaxIntentos)
                {
                    var ordenActual = await _unitOfWork.Ordenes.GetByIdAsync(orden.OrdenId, cancellationToken);
                    if (ordenActual is not null)
                    {
                        ordenActual.Estado = OrdenEstados.Pendiente;
                        _unitOfWork.Ordenes.Update(ordenActual);
                        await _unitOfWork.SaveChangesAsync(cancellationToken);
                    }
                }
            }

            if (!pagoExitoso)
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return new CompraResultDto
                {
                    OrdenId = orden.OrdenId,
                    EstadoOrden = OrdenEstados.Rechazada,
                    EstadoPago = ultimoPago?.Estado,
                    Resultado = "Pago rechazado despues de todos los reintentos."
                };
            }

            await _inventarioService.DescontarCoreAsync(orden.OrdenId, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return new CompraResultDto
            {
                OrdenId = orden.OrdenId,
                EstadoOrden = OrdenEstados.Confirmada,
                EstadoPago = PagoEstados.Autorizado,
                Resultado = $"Compra exitosa. Orden #{orden.OrdenId} confirmada."
            };
        }
        catch (InsufficientStockException ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw new InsufficientStockException(
                "Stock insuficiente (posible compra simultanea). La transaccion fue revertida. " + ex.Message);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
