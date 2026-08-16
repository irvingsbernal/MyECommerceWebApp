using MyECommerceWebApp.Domain.Entities;

namespace MyECommerceWebApp.Domain.Interfaces;

public interface IProductoRepository : IRepository<Producto>
{
    Task<IReadOnlyList<Producto>> GetActivosAsync(CancellationToken cancellationToken = default);

    Task<int> TryDecrementStockAsync(int productoId, int cantidad, CancellationToken cancellationToken = default);
}
