using Swimago.Domain.Enums;

namespace Swimago.Application.DTOs.PaymentMethods;

/// <summary>
/// Payment method response
/// </summary>
public record PaymentMethodResponse(
    Guid Id,
    PaymentBrand Brand,
    string Last4,
    int ExpiryMonth,
    int ExpiryYear,
    string? CardholderName,
    bool IsDefault,
    DateTime CreatedAt
);

/// <summary>
/// Add payment method request
/// </summary>
public record AddPaymentMethodRequest(
    string CardNumber,
    int ExpiryMonth,
    int ExpiryYear,
    string Cvv,
    string CardholderName,
    bool SetAsDefault = false
);

/// <summary>
/// Payment method list response
/// </summary>
public record PaymentMethodListResponse(
    IEnumerable<PaymentMethodResponse> PaymentMethods,
    Guid? DefaultPaymentMethodId
);
