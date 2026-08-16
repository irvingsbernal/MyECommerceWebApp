namespace MyECommerceWebApp.Application.DTOs.Auth;

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public int? ClienteId { get; set; }

    public string Email { get; set; } = string.Empty;

    public string NombreCompleto { get; set; } = string.Empty;
}
