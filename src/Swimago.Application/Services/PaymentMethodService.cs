using Microsoft.Extensions.Logging;
using Swimago.Application.DTOs.PaymentMethods;
using Swimago.Application.Interfaces;
using Swimago.Domain.Entities;
using Swimago.Domain.Enums;
using Swimago.Domain.Interfaces;

namespace Swimago.Application.Services;

public class PaymentMethodService : IPaymentMethodService
{
    private readonly IPaymentMethodRepository _paymentMethodRepository;
    private readonly ILogger<PaymentMethodService> _logger;

    public PaymentMethodService(
        IPaymentMethodRepository paymentMethodRepository,
        ILogger<PaymentMethodService> logger)
    {
        _paymentMethodRepository = paymentMethodRepository;
        _logger = logger;
    }

    public async Task<PaymentMethodListResponse> GetPaymentMethodsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching payment methods for user {UserId}", userId);

        var paymentMethods = await _paymentMethodRepository.GetByUserIdAsync(userId, cancellationToken);
        var defaultMethod = paymentMethods.FirstOrDefault(pm => pm.IsDefault);

        var items = paymentMethods.Select(pm => new PaymentMethodResponse(
            Id: pm.Id,
            Brand: pm.Brand, // Direct assignment (Enum to Enum)
            Last4: pm.Last4,
            ExpiryMonth: pm.ExpiryMonth,
            ExpiryYear: pm.ExpiryYear,
            CardholderName: null, 
            IsDefault: pm.IsDefault,
            CreatedAt: pm.CreatedAt
        )).ToList();

        return new PaymentMethodListResponse(items, defaultMethod?.Id);
    }

    public async Task<PaymentMethodResponse> AddPaymentMethodAsync(Guid userId, AddPaymentMethodRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Adding payment method for user {UserId}", userId);

        ValidateCard(request);

        var cleanCardNumber = request.CardNumber.Replace(" ", "");
        var brand = DetectCardBrand(cleanCardNumber);

        // Check if this should be default
        var existingMethods = await _paymentMethodRepository.GetByUserIdAsync(userId, cancellationToken);
        var isDefault = !existingMethods.Any() || request.SetAsDefault;

        // If setting as default, unset current default
        if (isDefault && existingMethods.Any(m => m.IsDefault))
        {
            await _paymentMethodRepository.SetDefaultAsync(userId, Guid.Empty, cancellationToken);
        }

        var paymentMethod = new PaymentMethod
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = "card",
            Brand = brand, // Store Enum directly
            Last4 = cleanCardNumber[^4..],
            ExpiryMonth = request.ExpiryMonth,
            ExpiryYear = request.ExpiryYear,
            IsDefault = isDefault,
            CreatedAt = DateTime.UtcNow,
            ProviderToken = null 
        };

        await _paymentMethodRepository.AddAsync(paymentMethod, cancellationToken);

        return new PaymentMethodResponse(
            Id: paymentMethod.Id,
            Brand: paymentMethod.Brand,
            Last4: paymentMethod.Last4,
            ExpiryMonth: paymentMethod.ExpiryMonth,
            ExpiryYear: paymentMethod.ExpiryYear,
            CardholderName: request.CardholderName,
            IsDefault: paymentMethod.IsDefault,
            CreatedAt: paymentMethod.CreatedAt
        );
    }

    public async Task DeletePaymentMethodAsync(Guid userId, Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting payment method {Id} for user {UserId}", id, userId);

        var paymentMethod = await _paymentMethodRepository.GetByIdAsync(id, cancellationToken);

        if (paymentMethod == null || paymentMethod.UserId != userId)
            throw new KeyNotFoundException("Ödeme yöntemi bulunamadı");

        await _paymentMethodRepository.DeleteAsync(paymentMethod, cancellationToken);
    }

    public async Task SetDefaultPaymentMethodAsync(Guid userId, Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Setting default payment method {Id} for user {UserId}", id, userId);

        var paymentMethod = await _paymentMethodRepository.GetByIdAsync(id, cancellationToken);

        if (paymentMethod == null || paymentMethod.UserId != userId)
            throw new KeyNotFoundException("Ödeme yöntemi bulunamadı");

        await _paymentMethodRepository.SetDefaultAsync(userId, id, cancellationToken);
    }

    private void ValidateCard(AddPaymentMethodRequest request)
    {
        if (string.IsNullOrEmpty(request.CardNumber) || request.CardNumber.Replace(" ", "").Length < 13)
            throw new ArgumentException("Geçersiz kart numarası");

        if (request.ExpiryMonth < 1 || request.ExpiryMonth > 12)
            throw new ArgumentException("Geçersiz son kullanma ayı");

        if (request.ExpiryYear < DateTime.UtcNow.Year)
            throw new ArgumentException("Kartınızın süresi dolmuş");

        if (string.IsNullOrEmpty(request.Cvv) || request.Cvv.Length < 3)
            throw new ArgumentException("Geçersiz CVV");
    }

    private PaymentBrand DetectCardBrand(string cardNumber)
    {
        if (cardNumber.StartsWith("4")) return PaymentBrand.Visa;
        if (cardNumber.StartsWith("5")) return PaymentBrand.Mastercard;
        if (cardNumber.StartsWith("34") || cardNumber.StartsWith("37")) return PaymentBrand.Amex;
        return PaymentBrand.Visa; // Default
    }
}
