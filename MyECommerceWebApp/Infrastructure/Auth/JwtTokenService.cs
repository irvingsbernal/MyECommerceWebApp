using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MyECommerceWebApp.Application.Interfaces;

namespace MyECommerceWebApp.Infrastructure.Auth;

public class JwtTokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string CreateClienteToken(int clienteId, string email, string nombreCompleto)
    {
        return CreateToken(
        [
            new Claim(JwtRegisteredClaimNames.Sub, clienteId.ToString()),
            new Claim(ClaimTypes.NameIdentifier, clienteId.ToString()),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Name, nombreCompleto),
            new Claim(ClaimTypes.Role, "cliente"),
            new Claim("clienteId", clienteId.ToString())
        ]);
    }

    public string CreateAdminToken()
    {
        var email = _configuration["DemoAdmin:Email"] ?? "admin@ecommerce.local";
        return CreateToken(
        [
            new Claim(JwtRegisteredClaimNames.Sub, "admin"),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Name, "Administrador demo"),
            new Claim(ClaimTypes.Role, "admin")
        ]);
    }

    private string CreateToken(IEnumerable<Claim> claims)
    {
        var key = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key no configurada.");
        var issuer = _configuration["Jwt:Issuer"] ?? "MyECommerceWebApp";
        var audience = _configuration["Jwt:Audience"] ?? "MyECommerceWebApp";
        var minutes = int.TryParse(_configuration["Jwt:ExpirationMinutes"], out var parsed) ? parsed : 120;

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: DateTime.UtcNow.AddMinutes(minutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
