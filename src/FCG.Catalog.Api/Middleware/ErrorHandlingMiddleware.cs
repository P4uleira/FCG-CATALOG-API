using System.Net;
using System.Text.Json;

namespace FCG.Catalog.Api.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ErrorHandlingMiddleware(
        RequestDelegate next,
        ILogger<ErrorHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = exception switch
        {
            ArgumentException => HttpStatusCode.BadRequest,
            KeyNotFoundException => HttpStatusCode.NotFound,
            UnauthorizedAccessException => HttpStatusCode.Unauthorized,
            InvalidOperationException => HttpStatusCode.Conflict,
            _ => HttpStatusCode.InternalServerError
        };

        _logger.LogError(
            exception,
            "Erro ao processar requisição. Path: {Path}, Method: {Method}, StatusCode: {StatusCode}",
            context.Request.Path,
            context.Request.Method,
            (int)statusCode
        );

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            statusCode = (int)statusCode,
            message = exception.Message,
            detail = _environment.IsDevelopment() ? exception.StackTrace : null
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}