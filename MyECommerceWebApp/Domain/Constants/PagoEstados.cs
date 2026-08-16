namespace MyECommerceWebApp.Domain.Constants;

public static class PagoEstados
{
    public const string Autorizado = "autorizado";
    public const string Rechazado = "rechazado";
    public const string Pendiente = "pendiente";

    public static readonly string[] Todos = [Autorizado, Rechazado, Pendiente];
}
