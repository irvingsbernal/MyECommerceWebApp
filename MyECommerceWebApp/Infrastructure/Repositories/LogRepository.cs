using Microsoft.EntityFrameworkCore;
using MyECommerceWebApp.Domain.Entities;
using MyECommerceWebApp.Domain.Interfaces;
using MyECommerceWebApp.Infrastructure.Persistence;

namespace MyECommerceWebApp.Infrastructure.Repositories;

public class LogRepository : ILogRepository
{
    private readonly DbSet<LogEvento> _dbSet;

    public LogRepository(AppDbContext context)
    {
        _dbSet = context.Set<LogEvento>();
    }

    public async Task AddAsync(LogEvento entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
    }

    public async Task<IReadOnlyList<LogEvento>> GetRecentAsync(
        int take = 100,
        string? operacion = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(operacion))
        {
            query = query.Where(log => log.Operacion == operacion);
        }

        return await query
            .OrderByDescending(log => log.FechaEvento)
            .ThenByDescending(log => log.LogId)
            .Take(take)
            .ToListAsync(cancellationToken);
    }
}
