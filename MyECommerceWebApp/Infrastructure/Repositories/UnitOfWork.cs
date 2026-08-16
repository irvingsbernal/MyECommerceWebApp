using MyECommerceWebApp.Domain.Entities;
using MyECommerceWebApp.Domain.Interfaces;
using MyECommerceWebApp.Infrastructure.Persistence;

namespace MyECommerceWebApp.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
        Clientes = new ClienteRepository(context);
        Productos = new ProductoRepository(context);
        Ordenes = new OrdenRepository(context);
        OrdenDetalles = new Repository<OrdenDetalle>(context);
        Pagos = new PagoRepository(context);
        Logs = new LogRepository(context);
    }

    public IClienteRepository Clientes { get; }

    public IProductoRepository Productos { get; }

    public IOrdenRepository Ordenes { get; }

    public IRepository<OrdenDetalle> OrdenDetalles { get; }

    public IPagoRepository Pagos { get; }

    public ILogRepository Logs { get; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IAppTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        return new EfTransaction(transaction);
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}
