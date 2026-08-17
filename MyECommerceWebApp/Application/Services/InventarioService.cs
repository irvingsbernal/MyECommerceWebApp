using MyECommerceWebApp.Application.Exceptions;
using MyECommerceWebApp.Application.Interfaces;
using MyECommerceWebApp.Domain.Constants;
using MyECommerceWebApp.Domain.Interfaces;

namespace MyECommerceWebApp.Application.Services;

public class InventarioService : IInventarioService
{
    private readonly IUnitOfWork _unitOfWork;

    public InventarioService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task ActualizarPorOrdenAsync(int ordenId, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            await DescontarCoreAsync(ordenId, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (InsufficientStockException ex)
        {
            await transaction.RollbackAsync(cancellationToken);

            var orden = await _unitOfWork.Ordenes.GetByIdAsync(ordenId, cancellationToken);
            if (orden is not null && orden.Estado == OrdenEstados.Confirmada)
            {
                orden.Estado = OrdenEstados.Rechazada;
                _unitOfWork.Ordenes.Update(orden);
                await LogWriter.AddErrorAsync(_unitOfWork, "Productos", ordenId.ToString(), ex.Message, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            throw;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    internal async Task DescontarCoreAsync(int ordenId, CancellationToken cancellationToken)
    {
        var orden = await _unitOfWork.Ordenes.GetWithDetailsAsync(ordenId, cancellationToken);
        if (orden is null)
        {
            throw new NotFoundException("La orden no existe.");
        }

        if (orden.Estado != OrdenEstados.Confirmada)
        {
            throw new BusinessRuleException("Solo se puede actualizar inventario de ordenes confirmadas.");
        }

        var yaActualizado = await _unitOfWork.Logs.ExistsAsync(
            LogOperaciones.Inventario,
            ordenId.ToString(),
            cancellationToken);

        if (yaActualizado)
        {
            throw new BusinessRuleException("El inventario de esta orden ya fue actualizado.");
        }

        foreach (var detalle in orden.Detalles.OrderBy(item => item.ProductoId))
        {
            var affected = await _unitOfWork.Productos.TryDecrementStockAsync(
                detalle.ProductoId,
                detalle.Cantidad,
                cancellationToken);

            if (affected == 0)
            {
                throw new InsufficientStockException(
                    $"Stock insuficiente al confirmar la venta del producto '{detalle.Producto.Nombre}'.");
            }
        }

        await LogWriter.AddAsync(
            _unitOfWork,
            "Productos",
            LogOperaciones.Inventario,
            ordenId.ToString(),
            "Inventario actualizado por venta confirmada.",
            cancellationToken);
    }
}
