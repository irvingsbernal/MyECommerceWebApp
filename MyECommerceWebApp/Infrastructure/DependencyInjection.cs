using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using MyECommerceWebApp.Application.Interfaces;
using MyECommerceWebApp.Domain.Interfaces;
using MyECommerceWebApp.Infrastructure.Auth;
using MyECommerceWebApp.Infrastructure.Persistence;
using MyECommerceWebApp.Infrastructure.Repositories;

namespace MyECommerceWebApp.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ITokenService, JwtTokenService>();

        var jwtKey = configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key no configurada.");
        var issuer = configuration["Jwt:Issuer"] ?? "MyECommerceWebApp";
        var audience = configuration["Jwt:Audience"] ?? "MyECommerceWebApp";

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("Cliente", policy => policy.RequireRole("cliente"));
            options.AddPolicy("Admin", policy => policy.RequireRole("admin"));
        });

        return services;
    }
}
