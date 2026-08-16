using Microsoft.EntityFrameworkCore;
using MyECommerceWebApp.Domain.Entities;
using MyECommerceWebApp.Domain.Interfaces;
using MyECommerceWebApp.Infrastructure.Persistence;

namespace MyECommerceWebApp.Infrastructure.Repositories;

public class ProductoRepository : Repository<Producto>, IProductoRepository
{
    public ProductoRepository(AppDbContext context)
        : base(context)
    {
    }

    public async Task<IReadOnlyList<Producto>> GetActivosAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Where(producto => producto.Activo)
            .OrderBy(producto => producto.Nombre)
            .ToListAsync(cancellationToken);
    }

    public Task<int> TryDecrementStockAsync(int productoId, int cantidad, CancellationToken cancellationToken = default)
    {
        return DbSet
            .Where(producto => producto.ProductoId == productoId && producto.Activo && producto.Stock >= cantidad)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(producto => producto.Stock, producto => producto.Stock - cantidad),
                cancellationToken);
    }
}
