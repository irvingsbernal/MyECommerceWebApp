using MyECommerceWebApp.Application.DTOs.Ordenes;

namespace MyECommerceWebApp.Application.Interfaces;

public interface IOrdenService
{
    Task<OrdenEstadoDto> CrearAsync(CrearOrdenRequest request, CancellationToken cancellationToken = default);

    Task<OrdenEstadoDto> GetEstadoAsync(int ordenId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OrdenEstadoDto>> ListarPorEstadoAsync(string estado, CancellationToken cancellationToken = default);

    Task<OrdenEstadoDto> AutorizarPendienteAsync(int ordenId, CancellationToken cancellationToken = default);
}

public interface IPagoService
{
    Task<PagoDto> ProcesarAsync(int ordenId, ProcesarPagoRequest request, CancellationToken cancellationToken = default);

    Task<PagoDto> ReintentarAsync(int ordenId, ProcesarPagoRequest request, CancellationToken cancellationToken = default);
}

public interface IInventarioService
{
    Task ActualizarPorOrdenAsync(int ordenId, CancellationToken cancellationToken = default);
}

public interface ICompraService
{
    Task<CompraResultDto> ProcesarAsync(ProcesarCompraRequest request, CancellationToken cancellationToken = default);
}
