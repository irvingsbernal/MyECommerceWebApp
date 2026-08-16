using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using MyECommerceWebApp.Application.Exceptions;

namespace MyECommerceWebApp.Middleware;

public class ExceptionHandlingMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await WriteProblemAsync(context, exception);
        }
    }

    private async Task WriteProblemAsync(HttpContext context, Exception exception)
    {
        var (status, title) = exception switch
        {
            NotFoundException => (HttpStatusCode.NotFound, "Recurso no encontrado"),
            InsufficientStockException => (HttpStatusCode.Conflict, "Stock insuficiente"),
            PaymentRejectedException => (HttpStatusCode.UnprocessableEntity, "Pago rechazado"),
            UnauthorizedOperationException => (HttpStatusCode.Unauthorized, "No autorizado"),
            BusinessRuleException => (HttpStatusCode.UnprocessableEntity, "Regla de negocio"),
            FluentValidation.ValidationException => (HttpStatusCode.BadRequest, "Solicitud invalida"),
            _ => (HttpStatusCode.InternalServerError, "Error interno")
        };

        if ((int)status >= 500)
        {
            _logger.LogError(exception, "Error no controlado");
        }
        else
        {
            _logger.LogWarning(exception, "{Title}: {Message}", title, exception.Message);
        }

        var problem = new ProblemDetails
        {
            Status = (int)status,
            Title = title,
            Detail = exception.Message,
            Instance = context.Request.Path
        };

        context.Response.StatusCode = problem.Status.Value;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(problem, JsonOptions));
    }
}
