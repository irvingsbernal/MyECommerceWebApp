using MyECommerceWebApp.Domain.Constants;

namespace MyECommerceWebApp.Application.Payments;

public static class PaymentSimulator
{
    public const int MaxIntentos = 3;

    public const decimal UmbralRevision = 10000m;

    public static string Simular(decimal monto, string? referencia, string? forzarEstado = null)
    {
        if (!string.IsNullOrWhiteSpace(forzarEstado))
        {
            return forzarEstado;
        }

        if (referencia is not null && referencia.EndsWith("0000", StringComparison.Ordinal))
        {
            return PagoEstados.Rechazado;
        }

        if (monto > UmbralRevision)
        {
            return PagoEstados.Pendiente;
        }

        return PagoEstados.Autorizado;
    }
}
