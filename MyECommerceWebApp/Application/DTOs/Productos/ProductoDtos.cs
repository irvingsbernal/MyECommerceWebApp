using System.ComponentModel.DataAnnotations;

namespace MyECommerceWebApp.Application.DTOs.Productos;

public class ProductoDto
{
    public int ProductoId { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public decimal Precio { get; set; }

    public int Stock { get; set; }

    public bool Activo { get; set; }
}

public class CreateProductoRequest
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Descripcion { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal Precio { get; set; }

    [Range(0, int.MaxValue)]
    public int Stock { get; set; }

    public bool Activo { get; set; } = true;
}

public class UpdateProductoRequest : CreateProductoRequest;
