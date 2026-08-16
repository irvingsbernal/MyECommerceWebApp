using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using MyECommerceWebApp.Application.DTOs.Auth;
using MyECommerceWebApp.Application.DTOs.Ordenes;
using MyECommerceWebApp.Application.DTOs.Productos;
using MyECommerceWebApp.Domain.Constants;

namespace MyECommerceWebApp.Tests;

public class CompraFlowTests : IClassFixture<ApiFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly ApiFactory _factory;

    public CompraFlowTests(ApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CompraAutorizada_ConfirmaOrdenYDescuentaStock()
    {
        var client = CreateClient();
        var auth = await IdentificarAsync(client, "juan.perez@email.com");
        Authorize(client, auth.Token);

        var mouse = await FindProductoAsync(client, "Mouse inalambrico");
        var stockInicial = mouse.Stock;

        var compra = await client.PostAsJsonAsync("/api/compras", new
        {
            clienteId = auth.ClienteId,
            metodoPago = "Tarjeta de credito",
            referencia = "VISA-4532",
            lineas = new[] { new { productoId = mouse.ProductoId, cantidad = 2 } }
        });

        compra.EnsureSuccessStatusCode();
        var result = await compra.Content.ReadFromJsonAsync<CompraResultDto>(JsonOptions);
        Assert.NotNull(result);
        Assert.Equal(OrdenEstados.Confirmada, result.EstadoOrden);
        Assert.Equal(PagoEstados.Autorizado, result.EstadoPago);

        var estado = await client.GetFromJsonAsync<OrdenEstadoDto>($"/api/ordenes/{result.OrdenId}", JsonOptions);
        Assert.Equal(OrdenEstados.Confirmada, estado!.EstadoOrden);

        var mouseActualizado = await FindProductoAsync(client, "Mouse inalambrico");
        Assert.Equal(stockInicial - 2, mouseActualizado.Stock);
    }

    [Fact]
    public async Task Referencia0000_RechazaPago()
    {
        var client = CreateClient();
        var auth = await IdentificarAsync(client, "juan.perez@email.com");
        Authorize(client, auth.Token);

        var teclado = await FindProductoAsync(client, "Teclado mecanico");

        var compra = await client.PostAsJsonAsync("/api/compras", new
        {
            clienteId = auth.ClienteId,
            metodoPago = "Tarjeta de debito",
            referencia = "CARD-0000",
            lineas = new[] { new { productoId = teclado.ProductoId, cantidad = 1 } }
        });

        compra.EnsureSuccessStatusCode();
        var result = await compra.Content.ReadFromJsonAsync<CompraResultDto>(JsonOptions);
        Assert.NotNull(result);
        Assert.Equal(OrdenEstados.Rechazada, result.EstadoOrden);
        Assert.Contains("rechazado", result.Resultado, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task DosComprasParalelas_ConStockUno_SoloUnaConfirma()
    {
        var clientA = CreateClient();
        var clientB = CreateClient();

        var authA = await IdentificarAsync(clientA, "juan.perez@email.com");
        var authB = await IdentificarAsync(clientB, "maria.lopez@email.com");
        Authorize(clientA, authA.Token);
        Authorize(clientB, authB.Token);

        var limitado = await FindProductoAsync(clientA, "Edicion limitada");
        Assert.Equal(1, limitado.Stock);

        var payloadA = new
        {
            clienteId = authA.ClienteId,
            metodoPago = "Tarjeta de credito",
            referencia = "VISA-1111",
            lineas = new[] { new { productoId = limitado.ProductoId, cantidad = 1 } }
        };
        var payloadB = new
        {
            clienteId = authB.ClienteId,
            metodoPago = "Tarjeta de credito",
            referencia = "VISA-2222",
            lineas = new[] { new { productoId = limitado.ProductoId, cantidad = 1 } }
        };

        var taskA = clientA.PostAsJsonAsync("/api/compras", payloadA);
        var taskB = clientB.PostAsJsonAsync("/api/compras", payloadB);
        var responses = await Task.WhenAll(taskA, taskB);
        var success = responses.Count(response => response.IsSuccessStatusCode);
        var conflict = responses.Count(response => response.StatusCode == HttpStatusCode.Conflict);

        Assert.Equal(1, success);
        Assert.Equal(1, conflict);

        var stockFinal = await FindProductoAsync(clientA, "Edicion limitada");
        Assert.Equal(0, stockFinal.Stock);
    }

    private HttpClient CreateClient()
    {
        return _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("http://localhost")
        });
    }

    private static async Task<AuthResponse> IdentificarAsync(HttpClient client, string email)
    {
        var response = await client.PostAsJsonAsync("/api/auth/identificar", new { email });
        response.EnsureSuccessStatusCode();
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);
        Assert.NotNull(auth);
        return auth;
    }

    private static void Authorize(HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private static async Task<ProductoDto> FindProductoAsync(HttpClient client, string nombre)
    {
        var productos = await client.GetFromJsonAsync<List<ProductoDto>>("/api/productos", JsonOptions);
        var producto = productos!.Single(item => item.Nombre == nombre);
        return producto;
    }
}
