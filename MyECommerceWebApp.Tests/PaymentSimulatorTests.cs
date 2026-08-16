using MyECommerceWebApp.Application.Payments;
using MyECommerceWebApp.Domain.Constants;

namespace MyECommerceWebApp.Tests;

public class PaymentSimulatorTests
{
    [Fact]
    public void ReferenciaTerminadaEn0000_Rechaza()
    {
        var estado = PaymentSimulator.Simular(100m, "CARD-0000");
        Assert.Equal(PagoEstados.Rechazado, estado);
    }

    [Fact]
    public void MontoMayorA10000_QuedaPendiente()
    {
        var estado = PaymentSimulator.Simular(12500m, "VISA-4532");
        Assert.Equal(PagoEstados.Pendiente, estado);
    }

    [Fact]
    public void PagoNormal_Autoriza()
    {
        var estado = PaymentSimulator.Simular(1299.99m, "VISA-4532");
        Assert.Equal(PagoEstados.Autorizado, estado);
    }

    [Fact]
    public void ForzarEstado_TienePrioridad()
    {
        var estado = PaymentSimulator.Simular(100m, "CARD-0000", PagoEstados.Autorizado);
        Assert.Equal(PagoEstados.Autorizado, estado);
    }

    [Fact]
    public void MaxIntentos_EsTres()
    {
        Assert.Equal(3, PaymentSimulator.MaxIntentos);
    }
}
