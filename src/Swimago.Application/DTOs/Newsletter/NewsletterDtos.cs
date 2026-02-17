namespace Swimago.Application.DTOs.Newsletter;

/// <summary>
/// Newsletter subscribe request
/// </summary>
public record NewsletterSubscribeRequest(
    string Email,
    string? Name,
    string? Language
);

/// <summary>
/// Newsletter subscribe response
/// </summary>
public record NewsletterSubscribeResponse(
    bool Success,
    string? Message,
    string? Email,
    bool IsSubscribed
);
