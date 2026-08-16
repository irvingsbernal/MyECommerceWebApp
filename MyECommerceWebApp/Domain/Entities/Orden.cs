namespace MyECommerceWebApp.Domain.Entities;

public class Orden
{
    public int OrdenId { get; set; }

    public int ClienteId { get; set; }

    public DateTime FechaOrden { get; set; }

    public string Estado { get; set; } = string.Empty;

    public decimal Total { get; set; }

    public string? Observaciones { get; set; }

    public Cliente Cliente { get; set; } = null!;

    public ICollection<OrdenDetalle> Detalles { get; set; } = [];

    public ICollection<Pago> Pagos { get; set; } = [];
}
