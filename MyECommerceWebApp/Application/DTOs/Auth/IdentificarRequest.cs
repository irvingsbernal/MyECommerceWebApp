using System.ComponentModel.DataAnnotations;

namespace MyECommerceWebApp.Application.DTOs.Auth;

public class IdentificarRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}
