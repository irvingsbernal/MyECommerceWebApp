namespace MyECommerceWebApp.Application.DTOs.Clientes;

public class ClienteDto
{
    public int ClienteId { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Apellido { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? Telefono { get; set; }

    public string Direccion { get; set; } = string.Empty;

    public DateTime FechaRegistro { get; set; }

    public bool Activo { get; set; }
}
