namespace MyECommerceWebApp.Domain.Entities;

public class Producto
{
    public int ProductoId { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public decimal Precio { get; set; }

    public int Stock { get; set; }

    public bool Activo { get; set; } = true;

    public ICollection<OrdenDetalle> OrdenDetalles { get; set; } = [];
}
