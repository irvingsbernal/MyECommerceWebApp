using System.ComponentModel.DataAnnotations;

namespace MyECommerceWebApp.Application.DTOs.Ordenes;

public class LineaOrdenRequest
{
    [Range(1, int.MaxValue)]
    public int ProductoId { get; set; }

    [Range(1, int.MaxValue)]
    public int Cantidad { get; set; }
}

public class CrearOrdenRequest
{
    [Range(1, int.MaxValue)]
    public int ClienteId { get; set; }

    [Required]
    [MinLength(1)]
    public List<LineaOrdenRequest> Lineas { get; set; } = [];

    [StringLength(500)]
    public string? Observaciones { get; set; }
}

public class ProcesarPagoRequest
{
    [Required]
    [StringLength(50, MinimumLength = 1)]
    public string MetodoPago { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Referencia { get; set; }

    [StringLength(20)]
    public string? ForzarEstado { get; set; }
}

public class ProcesarCompraRequest
{
    [Range(1, int.MaxValue)]
    public int ClienteId { get; set; }

    [Required]
    [MinLength(1)]
    public List<LineaOrdenRequest> Lineas { get; set; } = [];

    [Required]
    [StringLength(50, MinimumLength = 1)]
    public string MetodoPago { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Referencia { get; set; }

    [StringLength(500)]
    public string? Observaciones { get; set; }
}
