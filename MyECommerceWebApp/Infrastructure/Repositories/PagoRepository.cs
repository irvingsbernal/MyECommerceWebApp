using Microsoft.EntityFrameworkCore;
using MyECommerceWebApp.Domain.Entities;
using MyECommerceWebApp.Domain.Interfaces;
using MyECommerceWebApp.Infrastructure.Persistence;

namespace MyECommerceWebApp.Infrastructure.Repositories;

public class PagoRepository : Repository<Pago>, IPagoRepository
{
    public PagoRepository(AppDbContext context)
        : base(context)
    {
    }

    public async Task<IReadOnlyList<Pago>> GetByOrdenIdAsync(int ordenId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Where(pago => pago.OrdenId == ordenId)
            .OrderBy(pago => pago.PagoId)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetMaxIntentosAsync(int ordenId, CancellationToken cancellationToken = default)
    {
        var max = await DbSet
            .Where(pago => pago.OrdenId == ordenId)
            .Select(pago => (int?)pago.Intentos)
            .MaxAsync(cancellationToken);

        return max ?? 0;
    }

    public Task<Pago?> GetLatestByOrdenIdAsync(int ordenId, CancellationToken cancellationToken = default)
    {
        return DbSet
            .Where(pago => pago.OrdenId == ordenId)
            .OrderByDescending(pago => pago.PagoId)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
