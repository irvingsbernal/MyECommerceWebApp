using System.ComponentModel.DataAnnotations;

namespace MyECommerceWebApp.Application.DTOs.Logs;

public class LogEventoDto
{
    public long LogId { get; set; }

    public string TablaAfectada { get; set; } = string.Empty;

    public string Operacion { get; set; } = string.Empty;

    public string? RegistroId { get; set; }

    public string? MensajeLog { get; set; }

    public string Usuario { get; set; } = string.Empty;

    public DateTime FechaEvento { get; set; }
}

public class RegistrarLogRequest
{
    [Required]
    [StringLength(100)]
    public string TablaAfectada { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string Operacion { get; set; } = string.Empty;

    [StringLength(50)]
    public string? RegistroId { get; set; }

    public string? MensajeLog { get; set; }
}
