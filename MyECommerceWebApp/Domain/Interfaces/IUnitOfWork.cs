namespace MyECommerceWebApp.Domain.Interfaces;

public interface IUnitOfWork : IAsyncDisposable
{
    IClienteRepository Clientes { get; }

    IProductoRepository Productos { get; }

    IOrdenRepository Ordenes { get; }

    IRepository<Entities.OrdenDetalle> OrdenDetalles { get; }

    IPagoRepository Pagos { get; }

    ILogRepository Logs { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    Task<IAppTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}
