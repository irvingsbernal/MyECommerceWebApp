using MyECommerceWebApp.Application.DTOs.Logs;

namespace MyECommerceWebApp.Application.Interfaces;

public interface ILogService
{
    Task<IReadOnlyList<LogEventoDto>> GetRecentAsync(
        int take = 100,
        string? operacion = null,
        CancellationToken cancellationToken = default);

    Task<LogEventoDto> RegistrarAsync(RegistrarLogRequest request, CancellationToken cancellationToken = default);
}
