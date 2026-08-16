using System.ComponentModel.DataAnnotations;

namespace MyECommerceWebApp.Application.DTOs.Auth;

public class AdminLoginRequest
{
    [Required]
    public string DemoKey { get; set; } = string.Empty;
}
