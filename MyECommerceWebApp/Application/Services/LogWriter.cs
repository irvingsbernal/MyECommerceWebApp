using MyECommerceWebApp.Domain.Constants;
using MyECommerceWebApp.Domain.Entities;
using MyECommerceWebApp.Domain.Interfaces;

namespace MyECommerceWebApp.Application.Services;

internal static class LogWriter
{
    public static LogEvento Create(
        string tabla,
        string operacion,
        string? registroId,
        string? mensaje,
        string usuario = "api")
    {
        return new LogEvento
        {
            TablaAfectada = tabla,
            Operacion = operacion,
            RegistroId = registroId,
            MensajeLog = mensaje,
            Usuario = usuario,
            FechaEvento = DateTime.UtcNow
        };
    }

    public static Task AddAsync(
        IUnitOfWork unitOfWork,
        string tabla,
        string operacion,
        string? registroId,
        string? mensaje,
        CancellationToken cancellationToken,
        string usuario = "api")
    {
        return unitOfWork.Logs.AddAsync(Create(tabla, operacion, registroId, mensaje, usuario), cancellationToken);
    }

    public static Task AddErrorAsync(
        IUnitOfWork unitOfWork,
        string tabla,
        string? registroId,
        string mensaje,
        CancellationToken cancellationToken)
    {
        return AddAsync(unitOfWork, tabla, LogOperaciones.Error, registroId, mensaje, cancellationToken);
    }
}
