using MyECommerceWebApp.Domain.Entities;

namespace MyECommerceWebApp.Domain.Interfaces;

public interface IPagoRepository : IRepository<Pago>
{
    Task<IReadOnlyList<Pago>> GetByOrdenIdAsync(int ordenId, CancellationToken cancellationToken = default);

    Task<int> GetMaxIntentosAsync(int ordenId, CancellationToken cancellationToken = default);

    Task<Pago?> GetLatestByOrdenIdAsync(int ordenId, CancellationToken cancellationToken = default);
}
