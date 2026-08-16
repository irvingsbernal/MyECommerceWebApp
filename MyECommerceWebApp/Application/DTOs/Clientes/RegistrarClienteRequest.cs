using System.ComponentModel.DataAnnotations;

namespace MyECommerceWebApp.Application.DTOs.Clientes;

public class RegistrarClienteRequest
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Apellido { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;

    [StringLength(20)]
    public string? Telefono { get; set; }

    [Required]
    [StringLength(300, MinimumLength = 1)]
    public string Direccion { get; set; } = string.Empty;
}
