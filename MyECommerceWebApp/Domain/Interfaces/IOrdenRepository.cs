using MyECommerceWebApp.Domain.Entities;

namespace MyECommerceWebApp.Domain.Interfaces;

public interface IOrdenRepository : IRepository<Orden>
{
    Task<Orden?> GetWithDetailsAsync(int ordenId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Orden>> GetByEstadoAsync(string estado, CancellationToken cancellationToken = default);
}
