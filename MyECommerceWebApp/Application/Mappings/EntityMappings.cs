using MyECommerceWebApp.Application.DTOs.Auth;
using MyECommerceWebApp.Application.DTOs.Clientes;
using MyECommerceWebApp.Application.DTOs.Logs;
using MyECommerceWebApp.Application.DTOs.Ordenes;
using MyECommerceWebApp.Application.DTOs.Productos;
using MyECommerceWebApp.Domain.Entities;

namespace MyECommerceWebApp.Application.Mappings;

public static class EntityMappings
{
    public static ClienteDto ToDto(this Cliente cliente) => new()
    {
        ClienteId = cliente.ClienteId,
        Nombre = cliente.Nombre,
        Apellido = cliente.Apellido,
        Email = cliente.Email,
        Telefono = cliente.Telefono,
        Direccion = cliente.Direccion,
        FechaRegistro = cliente.FechaRegistro,
        Activo = cliente.Activo
    };

    public static ProductoDto ToDto(this Producto producto) => new()
    {
        ProductoId = producto.ProductoId,
        Nombre = producto.Nombre,
        Descripcion = producto.Descripcion,
        Precio = producto.Precio,
        Stock = producto.Stock,
        Activo = producto.Activo
    };

    public static PagoDto ToDto(this Pago pago) => new()
    {
        PagoId = pago.PagoId,
        Monto = pago.Monto,
        Estado = pago.Estado,
        MetodoPago = pago.MetodoPago,
        Referencia = pago.Referencia,
        Intentos = pago.Intentos,
        FechaPago = pago.FechaPago,
        FechaRegistro = pago.FechaRegistro,
        MensajeError = pago.MensajeError
    };

    public static LogEventoDto ToDto(this LogEvento log) => new()
    {
        LogId = log.LogId,
        TablaAfectada = log.TablaAfectada,
        Operacion = log.Operacion,
        RegistroId = log.RegistroId,
        MensajeLog = log.MensajeLog,
        Usuario = log.Usuario,
        FechaEvento = log.FechaEvento
    };

    public static OrdenEstadoDto ToEstadoDto(this Orden orden)
    {
        var ultimoPago = orden.Pagos.OrderByDescending(pago => pago.PagoId).FirstOrDefault();

        return new OrdenEstadoDto
        {
            OrdenId = orden.OrdenId,
            FechaOrden = orden.FechaOrden,
            EstadoOrden = orden.Estado,
            Total = orden.Total,
            ClienteId = orden.ClienteId,
            ClienteNombre = $"{orden.Cliente.Nombre} {orden.Cliente.Apellido}",
            Email = orden.Cliente.Email,
            PagoId = ultimoPago?.PagoId,
            EstadoPago = ultimoPago?.Estado,
            MetodoPago = ultimoPago?.MetodoPago,
            IntentosPago = ultimoPago?.Intentos,
            FechaPago = ultimoPago?.FechaPago,
            MensajeError = ultimoPago?.MensajeError,
            TotalProductos = orden.Detalles.Count,
            Detalles = orden.Detalles.Select(detalle => new OrdenDetalleDto
            {
                OrdenDetalleId = detalle.OrdenDetalleId,
                ProductoId = detalle.ProductoId,
                Producto = detalle.Producto.Nombre,
                Cantidad = detalle.Cantidad,
                PrecioUnitario = detalle.PrecioUnitario,
                Subtotal = detalle.Cantidad * detalle.PrecioUnitario,
                StockActual = detalle.Producto.Stock
            }).ToList(),
            Pagos = orden.Pagos.OrderBy(pago => pago.PagoId).Select(pago => pago.ToDto()).ToList()
        };
    }

    public static AuthResponse ToAuthResponse(this Cliente cliente, string token, string role) => new()
    {
        Token = token,
        Role = role,
        ClienteId = cliente.ClienteId,
        Email = cliente.Email,
        NombreCompleto = cliente.Nombre
    };
}
