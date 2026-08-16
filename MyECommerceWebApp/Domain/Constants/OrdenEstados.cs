namespace MyECommerceWebApp.Domain.Constants;

public static class OrdenEstados
{
    public const string Pendiente = "pendiente";
    public const string Confirmada = "confirmada";
    public const string Cancelada = "cancelada";
    public const string Rechazada = "rechazada";

    public static readonly string[] Todos = [Pendiente, Confirmada, Cancelada, Rechazada];
}
