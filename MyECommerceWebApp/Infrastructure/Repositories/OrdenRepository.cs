using Microsoft.EntityFrameworkCore;
using MyECommerceWebApp.Domain.Entities;
using MyECommerceWebApp.Domain.Interfaces;
using MyECommerceWebApp.Infrastructure.Persistence;

namespace MyECommerceWebApp.Infrastructure.Repositories;

public class OrdenRepository : Repository<Orden>, IOrdenRepository
{
    public OrdenRepository(AppDbContext context)
        : base(context)
    {
    }

    public Task<Orden?> GetWithDetailsAsync(int ordenId, CancellationToken cancellationToken = default)
    {
        return DbSet
            .Include(orden => orden.Cliente)
            .Include(orden => orden.Detalles)
                .ThenInclude(detalle => detalle.Producto)
            .Include(orden => orden.Pagos)
            .FirstOrDefaultAsync(orden => orden.OrdenId == ordenId, cancellationToken);
    }

    public async Task<IReadOnlyList<Orden>> GetByEstadoAsync(string estado, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Include(orden => orden.Cliente)
            .Include(orden => orden.Detalles)
                .ThenInclude(detalle => detalle.Producto)
            .Include(orden => orden.Pagos)
            .Where(orden => orden.Estado == estado)
            .OrderByDescending(orden => orden.FechaOrden)
            .ToListAsync(cancellationToken);
    }
}
