using MyECommerceWebApp.Application.DTOs.Ordenes;
using MyECommerceWebApp.Application.Exceptions;
using MyECommerceWebApp.Application.Interfaces;
using MyECommerceWebApp.Application.Mappings;
using MyECommerceWebApp.Domain.Constants;
using MyECommerceWebApp.Domain.Entities;
using MyECommerceWebApp.Domain.Interfaces;

namespace MyECommerceWebApp.Application.Services;

public class OrdenService : IOrdenService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly InventarioService _inventarioService;

    public OrdenService(IUnitOfWork unitOfWork, InventarioService inventarioService)
    {
        _unitOfWork = unitOfWork;
        _inventarioService = inventarioService;
    }

    public async Task<OrdenEstadoDto> CrearAsync(CrearOrdenRequest request, CancellationToken cancellationToken = default)
    {
        var orden = await CrearInternoAsync(request.ClienteId, request.Lineas, request.Observaciones, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var created = await _unitOfWork.Ordenes.GetWithDetailsAsync(orden.OrdenId, cancellationToken);
        return created!.ToEstadoDto();
    }

    public async Task<OrdenEstadoDto> GetEstadoAsync(int ordenId, CancellationToken cancellationToken = default)
    {
        var orden = await _unitOfWork.Ordenes.GetWithDetailsAsync(ordenId, cancellationToken);
        if (orden is null)
        {
            throw new NotFoundException("Orden no encontrada.");
        }

        return orden.ToEstadoDto();
    }

    public async Task<IReadOnlyList<OrdenEstadoDto>> ListarPorEstadoAsync(
        string estado,
        CancellationToken cancellationToken = default)
    {
        var ordenes = await _unitOfWork.Ordenes.GetByEstadoAsync(estado, cancellationToken);
        return ordenes.Select(orden => orden.ToEstadoDto()).ToList();
    }

    public async Task<OrdenEstadoDto> AutorizarPendienteAsync(int ordenId, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var orden = await _unitOfWork.Ordenes.GetWithDetailsAsync(ordenId, cancellationToken);
            if (orden is null)
            {
                throw new NotFoundException("Orden no encontrada.");
            }

            if (orden.Estado != OrdenEstados.Pendiente)
            {
                throw new BusinessRuleException("Solo se pueden autorizar ordenes pendientes.");
            }

            var ultimoPago = orden.Pagos.OrderByDescending(pago => pago.PagoId).FirstOrDefault();
            if (ultimoPago is null)
            {
                await _unitOfWork.Pagos.AddAsync(new Pago
                {
                    OrdenId = orden.OrdenId,
                    Monto = orden.Total,
                    Estado = PagoEstados.Autorizado,
                    MetodoPago = "Autorizacion admin",
                    Intentos = 1,
                    FechaRegistro = DateTime.UtcNow,
                    FechaPago = DateTime.UtcNow
                }, cancellationToken);
            }
            else if (ultimoPago.Estado == PagoEstados.Pendiente)
            {
                ultimoPago.Estado = PagoEstados.Autorizado;
                ultimoPago.FechaPago = DateTime.UtcNow;
                ultimoPago.MensajeError = null;
                _unitOfWork.Pagos.Update(ultimoPago);
            }
            else
            {
                throw new BusinessRuleException("Solo se pueden autorizar ordenes con pago pendiente de revision.");
            }

            orden.Estado = OrdenEstados.Confirmada;
            _unitOfWork.Ordenes.Update(orden);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await LogWriter.AddAsync(
                _unitOfWork,
                "Pagos",
                LogOperaciones.Pago,
                orden.OrdenId.ToString(),
                "Pago autorizado por administrador.",
                cancellationToken);

            await _inventarioService.DescontarCoreAsync(orden.OrdenId, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            var actualizada = await _unitOfWork.Ordenes.GetWithDetailsAsync(orden.OrdenId, cancellationToken);
            return actualizada!.ToEstadoDto();
        }
        catch (InsufficientStockException)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    internal async Task<Orden> CrearInternoAsync(
        int clienteId,
        IReadOnlyList<LineaOrdenRequest> lineas,
        string? observaciones,
        CancellationToken cancellationToken)
    {
        var cliente = await _unitOfWork.Clientes.GetByIdAsync(clienteId, cancellationToken);
        if (cliente is null || !cliente.Activo)
        {
            throw new BusinessRuleException("Cliente no existe o esta inactivo.");
        }

        if (lineas.Count == 0)
        {
            throw new BusinessRuleException("La orden debe tener al menos un producto.");
        }

        var productos = new List<Producto>();
        foreach (var linea in lineas.OrderBy(item => item.ProductoId))
        {
            var producto = await _unitOfWork.Productos.GetByIdAsync(linea.ProductoId, cancellationToken);
            if (producto is null || !producto.Activo)
            {
                throw new BusinessRuleException("Uno o mas productos no existen o estan inactivos.");
            }

            if (linea.Cantidad > producto.Stock)
            {
                throw new InsufficientStockException($"Stock insuficiente para el producto '{producto.Nombre}'.");
            }

            productos.Add(producto);
        }

        var total = lineas.Sum(linea =>
        {
            var producto = productos.First(item => item.ProductoId == linea.ProductoId);
            return linea.Cantidad * producto.Precio;
        });

        var orden = new Orden
        {
            ClienteId = clienteId,
            FechaOrden = DateTime.UtcNow,
            Estado = OrdenEstados.Pendiente,
            Total = total,
            Observaciones = observaciones
        };

        foreach (var linea in lineas)
        {
            var producto = productos.First(item => item.ProductoId == linea.ProductoId);
            orden.Detalles.Add(new OrdenDetalle
            {
                ProductoId = producto.ProductoId,
                Cantidad = linea.Cantidad,
                PrecioUnitario = producto.Precio
            });
        }

        await _unitOfWork.Ordenes.AddAsync(orden, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await LogWriter.AddAsync(
            _unitOfWork,
            "Ordenes",
            LogOperaciones.Insert,
            orden.OrdenId.ToString(),
            $"Orden creada. Total: {orden.Total}",
            cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return orden;
    }
}
