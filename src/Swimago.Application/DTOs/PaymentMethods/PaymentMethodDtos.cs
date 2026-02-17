namespace Swimago.Application.DTOs.PaymentMethods;

/// <summary>
/// Payment method response used by customer app.
/// </summary>
public record PaymentMethodResponse(
    Guid Id,
    string Brand,
    string LastFour,
    string ExpiryDate,
    bool IsDefault
);

/// <summary>
/// Add payment method request (tokenized).
/// </summary>
public record CreatePaymentMethodRequest(
    string Provider,
    string PaymentMethodToken,
    bool SetAsDefault = false,
    string? Brand = null,
    string? LastFour = null,
    string? ExpiryDate = null
);

/// <summary>
/// Patch payment method request.
/// </summary>
public record UpdatePaymentMethodRequest(
    string? Brand,
    string? LastFour,
    string? ExpiryDate
);

/// <summary>
/// Payment method list response.
/// </summary>
public record PaymentMethodListResponse(
    IEnumerable<PaymentMethodResponse> Items
);
