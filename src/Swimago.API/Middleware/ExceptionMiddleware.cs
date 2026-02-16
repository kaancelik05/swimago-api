using FluentValidation;
using Swimago.API.Models;
using System.Net;
using System.Text.Json;

namespace Swimago.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var statusCode = (int)HttpStatusCode.InternalServerError;
        string message = "An unexpected error occurred.";
        string? details = _env.IsDevelopment() ? exception.StackTrace : null;
        IDictionary<string, string[]>? validationErrors = null;

        switch (exception)
        {
            case ValidationException valEx:
                statusCode = (int)HttpStatusCode.BadRequest;
                message = "Validation failed.";
                validationErrors = valEx.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(x => x.ErrorMessage).ToArray());
                break;

            case UnauthorizedAccessException:
                statusCode = (int)HttpStatusCode.Unauthorized;
                message = "Unauthorized access.";
                break;

            case KeyNotFoundException:
                statusCode = (int)HttpStatusCode.NotFound;
                message = "Resource not found.";
                break;

            case InvalidOperationException invEx:
                statusCode = (int)HttpStatusCode.BadRequest;
                message = invEx.Message;
                break;
        }

        context.Response.StatusCode = statusCode;

        var response = new ErrorResponse(statusCode, message, details, validationErrors);
        
        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var json = JsonSerializer.Serialize(response, jsonOptions);

        await context.Response.WriteAsync(json);
    }
}
