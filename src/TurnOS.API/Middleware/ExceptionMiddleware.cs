using System.Net;
using System.Text.Json;

namespace TurnOS.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message) = exception switch
        {
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, exception.Message),
            InvalidOperationException => (HttpStatusCode.BadRequest, exception.Message),
            KeyNotFoundException => (HttpStatusCode.NotFound, exception.Message),
            _ => (HttpStatusCode.InternalServerError, "Ha ocurrido un error interno.")
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var result = JsonSerializer.Serialize(new { error = message });
        return context.Response.WriteAsync(result);
    }
}
