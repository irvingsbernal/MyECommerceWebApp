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

    public OrdenService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
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
