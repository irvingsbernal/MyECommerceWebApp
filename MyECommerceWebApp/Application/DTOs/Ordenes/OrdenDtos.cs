namespace MyECommerceWebApp.Application.DTOs.Ordenes;

public class OrdenDetalleDto
{
    public int OrdenDetalleId { get; set; }

    public int ProductoId { get; set; }

    public string Producto { get; set; } = string.Empty;

    public int Cantidad { get; set; }

    public decimal PrecioUnitario { get; set; }

    public decimal Subtotal { get; set; }

    public int StockActual { get; set; }
}

public class PagoDto
{
    public int PagoId { get; set; }

    public decimal Monto { get; set; }

    public string Estado { get; set; } = string.Empty;

    public string MetodoPago { get; set; } = string.Empty;

    public string? Referencia { get; set; }

    public int Intentos { get; set; }

    public DateTime? FechaPago { get; set; }

    public DateTime FechaRegistro { get; set; }

    public string? MensajeError { get; set; }
}

public class OrdenEstadoDto
{
    public int OrdenId { get; set; }

    public DateTime FechaOrden { get; set; }

    public string EstadoOrden { get; set; } = string.Empty;

    public decimal Total { get; set; }

    public int ClienteId { get; set; }

    public string ClienteNombre { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public int? PagoId { get; set; }

    public string? EstadoPago { get; set; }

    public string? MetodoPago { get; set; }

    public int? IntentosPago { get; set; }

    public DateTime? FechaPago { get; set; }

    public string? MensajeError { get; set; }

    public int TotalProductos { get; set; }

    public IReadOnlyList<OrdenDetalleDto> Detalles { get; set; } = [];

    public IReadOnlyList<PagoDto> Pagos { get; set; } = [];
}

public class CompraResultDto
{
    public int OrdenId { get; set; }

    public string EstadoOrden { get; set; } = string.Empty;

    public string? EstadoPago { get; set; }

    public string Resultado { get; set; } = string.Empty;
}
