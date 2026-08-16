using MyECommerceWebApp.Domain.Entities;

namespace MyECommerceWebApp.Domain.Interfaces;

public interface ILogRepository
{
    Task AddAsync(LogEvento entity, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LogEvento>> GetRecentAsync(
        int take = 100,
        string? operacion = null,
        CancellationToken cancellationToken = default);
}
