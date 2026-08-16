using Microsoft.EntityFrameworkCore;
using MyECommerceWebApp.Domain.Entities;

namespace MyECommerceWebApp.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Cliente> Clientes => Set<Cliente>();

    public DbSet<Producto> Productos => Set<Producto>();

    public DbSet<Orden> Ordenes => Set<Orden>();

    public DbSet<OrdenDetalle> OrdenDetalles => Set<OrdenDetalle>();

    public DbSet<Pago> Pagos => Set<Pago>();

    public DbSet<LogEvento> Logs => Set<LogEvento>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
