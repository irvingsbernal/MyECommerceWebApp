namespace MyECommerceWebApp.Domain.Entities;

public class Pago
{
    public int PagoId { get; set; }

    public int OrdenId { get; set; }

    public decimal Monto { get; set; }

    public string Estado { get; set; } = string.Empty;

    public string MetodoPago { get; set; } = string.Empty;

    public string? Referencia { get; set; }

    public int Intentos { get; set; }

    public DateTime? FechaPago { get; set; }

    public DateTime FechaRegistro { get; set; }

    public string? MensajeError { get; set; }

    public Orden Orden { get; set; } = null!;
}
