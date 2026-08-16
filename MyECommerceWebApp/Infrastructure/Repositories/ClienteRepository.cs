using Microsoft.EntityFrameworkCore;
using MyECommerceWebApp.Domain.Entities;
using MyECommerceWebApp.Domain.Interfaces;
using MyECommerceWebApp.Infrastructure.Persistence;

namespace MyECommerceWebApp.Infrastructure.Repositories;

public class ClienteRepository : Repository<Cliente>, IClienteRepository
{
    public ClienteRepository(AppDbContext context)
        : base(context)
    {
    }

    public Task<Cliente?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return DbSet.FirstOrDefaultAsync(cliente => cliente.Email == email, cancellationToken);
    }

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return DbSet.AnyAsync(cliente => cliente.Email == email, cancellationToken);
    }
}
