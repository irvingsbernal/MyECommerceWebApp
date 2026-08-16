namespace MyECommerceWebApp.Domain.Entities;

public class LogEvento
{
    public long LogId { get; set; }

    public string TablaAfectada { get; set; } = string.Empty;

    public string Operacion { get; set; } = string.Empty;

    public string? RegistroId { get; set; }

    public string? MensajeLog { get; set; }

    public string Usuario { get; set; } = "api";

    public DateTime FechaEvento { get; set; }
}
