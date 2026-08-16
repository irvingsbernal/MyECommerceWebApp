using MyECommerceWebApp.Application.DTOs.Logs;
using MyECommerceWebApp.Application.Exceptions;
using MyECommerceWebApp.Application.Interfaces;
using MyECommerceWebApp.Application.Mappings;
using MyECommerceWebApp.Domain.Constants;
using MyECommerceWebApp.Domain.Interfaces;

namespace MyECommerceWebApp.Application.Services;

public class LogService : ILogService
{
    private static readonly string[] OperacionesValidas =
    [
        LogOperaciones.Insert,
        LogOperaciones.Update,
        LogOperaciones.Delete,
        LogOperaciones.Error,
        LogOperaciones.Pago,
        LogOperaciones.Inventario
    ];

    private readonly IUnitOfWork _unitOfWork;

    public LogService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<LogEventoDto>> GetRecentAsync(
        int take = 100,
        string? operacion = null,
        CancellationToken cancellationToken = default)
    {
        var logs = await _unitOfWork.Logs.GetRecentAsync(take, operacion, cancellationToken);
        return logs.Select(log => log.ToDto()).ToList();
    }

    public async Task<LogEventoDto> RegistrarAsync(RegistrarLogRequest request, CancellationToken cancellationToken = default)
    {
        if (!OperacionesValidas.Contains(request.Operacion))
        {
            throw new BusinessRuleException("Operacion de bitacora no valida.");
        }

        var log = LogWriter.Create(request.TablaAfectada, request.Operacion, request.RegistroId, request.MensajeLog);
        await _unitOfWork.Logs.AddAsync(log, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return log.ToDto();
    }
}
