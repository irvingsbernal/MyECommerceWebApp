using MyECommerceWebApp.Application.DTOs.Productos;

namespace MyECommerceWebApp.Application.Interfaces;

public interface IProductoService
{
    Task<IReadOnlyList<ProductoDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<ProductoDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<ProductoDto> CreateAsync(CreateProductoRequest request, CancellationToken cancellationToken = default);

    Task<ProductoDto?> UpdateAsync(int id, UpdateProductoRequest request, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
