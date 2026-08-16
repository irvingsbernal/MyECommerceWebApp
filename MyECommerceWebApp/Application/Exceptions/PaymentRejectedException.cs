namespace MyECommerceWebApp.Application.Exceptions;

public class PaymentRejectedException : BusinessRuleException
{
    public PaymentRejectedException(string message = "El pago fue rechazado.")
        : base(message)
    {
    }
}
