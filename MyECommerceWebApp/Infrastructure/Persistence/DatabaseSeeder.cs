using Microsoft.EntityFrameworkCore;
using MyECommerceWebApp.Domain.Entities;

namespace MyECommerceWebApp.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(AppDbContext context, CancellationToken cancellationToken = default)
    {
        if (!await context.Productos.AnyAsync(cancellationToken))
        {
            context.Productos.AddRange(
                new Producto
                {
                    Nombre = "Laptop Pro 15\"",
                    Descripcion = "Laptop 16GB RAM, 512GB SSD",
                    Precio = 1299.99m,
                    Stock = 10,
                    Activo = true
                },
                new Producto
                {
                    Nombre = "Mouse inalambrico",
                    Descripcion = "Mouse ergonomico",
                    Precio = 29.99m,
                    Stock = 50,
                    Activo = true
                },
                new Producto
                {
                    Nombre = "Teclado mecanico",
                    Descripcion = "Teclado RGB",
                    Precio = 89.99m,
                    Stock = 30,
                    Activo = true
                },
                new Producto
                {
                    Nombre = "Servidor Enterprise",
                    Descripcion = "Servidor para revision de pago (monto > 10000)",
                    Precio = 12500.00m,
                    Stock = 5,
                    Activo = true
                },
                new Producto
                {
                    Nombre = "Edicion limitada",
                    Descripcion = "Unidad unica para probar carrera de stock",
                    Precio = 49.99m,
                    Stock = 1,
                    Activo = true
                });
        }

        if (!await context.Clientes.AnyAsync(cancellationToken))
        {
            context.Clientes.AddRange(
                new Cliente
                {
                    Nombre = "Juan",
                    Apellido = "Perez",
                    Email = "juan.perez@email.com",
                    Telefono = "555-1234",
                    Direccion = "Av. Reforma 123, CDMX",
                    FechaRegistro = DateTime.UtcNow,
                    Activo = true
                },
                new Cliente
                {
                    Nombre = "Maria",
                    Apellido = "Lopez",
                    Email = "maria.lopez@email.com",
                    Telefono = "555-5678",
                    Direccion = "Calle Juarez 45, Guadalajara",
                    FechaRegistro = DateTime.UtcNow,
                    Activo = true
                });
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
