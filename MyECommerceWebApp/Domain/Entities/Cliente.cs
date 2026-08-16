namespace MyECommerceWebApp.Domain.Entities;

public class Cliente
{
    public int ClienteId { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Apellido { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? Telefono { get; set; }

    public string Direccion { get; set; } = string.Empty;

    public DateTime FechaRegistro { get; set; }

    public bool Activo { get; set; } = true;

    public ICollection<Orden> Ordenes { get; set; } = [];
}
