namespace MyECommerceWebApp.Domain.Entities;

public class OrdenDetalle
{
    public int OrdenDetalleId { get; set; }

    public int OrdenId { get; set; }

    public int ProductoId { get; set; }

    public int Cantidad { get; set; }

    public decimal PrecioUnitario { get; set; }

    public decimal Subtotal { get; private set; }

    public Orden Orden { get; set; } = null!;

    public Producto Producto { get; set; } = null!;
}
