namespace Swimago.API.Models;

public record ErrorResponse(
    int StatusCode,
    string Message,
    string? Details = null,
    IDictionary<string, string[]>? ValidationErrors = null
);
