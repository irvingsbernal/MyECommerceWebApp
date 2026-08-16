using MyECommerceWebApp.Application.DTOs.Productos;
using MyECommerceWebApp.Application.Interfaces;
using MyECommerceWebApp.Application.Mappings;
using MyECommerceWebApp.Domain.Entities;
using MyECommerceWebApp.Domain.Interfaces;

namespace MyECommerceWebApp.Application.Services;

public class ProductoService : IProductoService
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductoService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<ProductoDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var productos = await _unitOfWork.Productos.GetActivosAsync(cancellationToken);
        return productos.Select(producto => producto.ToDto()).ToList();
    }

    public async Task<ProductoDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var producto = await _unitOfWork.Productos.GetByIdAsync(id, cancellationToken);
        return producto?.ToDto();
    }

    public async Task<ProductoDto> CreateAsync(CreateProductoRequest request, CancellationToken cancellationToken = default)
    {
        var producto = new Producto
        {
            Nombre = request.Nombre.Trim(),
            Descripcion = request.Descripcion,
            Precio = request.Precio,
            Stock = request.Stock,
            Activo = request.Activo
        };

        await _unitOfWork.Productos.AddAsync(producto, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return producto.ToDto();
    }

    public async Task<ProductoDto?> UpdateAsync(
        int id,
        UpdateProductoRequest request,
        CancellationToken cancellationToken = default)
    {
        var producto = await _unitOfWork.Productos.GetByIdAsync(id, cancellationToken);
        if (producto is null)
        {
            return null;
        }

        producto.Nombre = request.Nombre.Trim();
        producto.Descripcion = request.Descripcion;
        producto.Precio = request.Precio;
        producto.Stock = request.Stock;
        producto.Activo = request.Activo;
        _unitOfWork.Productos.Update(producto);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return producto.ToDto();
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var producto = await _unitOfWork.Productos.GetByIdAsync(id, cancellationToken);
        if (producto is null)
        {
            return false;
        }

        producto.Activo = false;
        _unitOfWork.Productos.Update(producto);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
