namespace MyECommerceWebApp.Application.Exceptions;

public class InsufficientStockException : BusinessRuleException
{
    public InsufficientStockException(string message = "Stock insuficiente para uno o mas productos.")
        : base(message)
    {
    }
}
