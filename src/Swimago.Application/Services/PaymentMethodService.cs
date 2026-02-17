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
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PaymentMethodService> _logger;

    public PaymentMethodService(
        IPaymentMethodRepository paymentMethodRepository,
        IUnitOfWork unitOfWork,
        ILogger<PaymentMethodService> logger)
    {
        _paymentMethodRepository = paymentMethodRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<PaymentMethodListResponse> GetPaymentMethodsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching payment methods for user {UserId}", userId);

        var paymentMethods = await _paymentMethodRepository.GetByUserIdAsync(userId, cancellationToken);

        var items = paymentMethods.Select(MapToResponse).ToList();
        return new PaymentMethodListResponse(items);
    }

    public async Task<PaymentMethodResponse> AddPaymentMethodAsync(Guid userId, CreatePaymentMethodRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Adding payment method for user {UserId}", userId);

        if (string.IsNullOrWhiteSpace(request.Provider))
            throw new ArgumentException("Provider zorunludur");

        if (string.IsNullOrWhiteSpace(request.PaymentMethodToken))
            throw new ArgumentException("paymentMethodToken zorunludur");

        var existingMethods = (await _paymentMethodRepository.GetByUserIdAsync(userId, cancellationToken)).ToList();
        var isDefault = !existingMethods.Any() || request.SetAsDefault;

        if (isDefault)
        {
            await _paymentMethodRepository.SetDefaultAsync(userId, Guid.Empty, cancellationToken);
        }

        var (expiryMonth, expiryYear) = ParseExpiry(request.ExpiryDate);

        var paymentMethod = new PaymentMethod
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = request.Provider.Trim().ToLowerInvariant(),
            Brand = ParseBrand(request.Brand),
            Last4 = NormalizeLastFour(request.LastFour),
            ExpiryMonth = expiryMonth,
            ExpiryYear = expiryYear,
            IsDefault = isDefault,
            CreatedAt = DateTime.UtcNow,
            ProviderToken = request.PaymentMethodToken
        };

        await _paymentMethodRepository.AddAsync(paymentMethod, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToResponse(paymentMethod);
    }

    public async Task<PaymentMethodResponse> UpdatePaymentMethodAsync(Guid userId, Guid id, UpdatePaymentMethodRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating payment method {Id} for user {UserId}", id, userId);

        var paymentMethod = await _paymentMethodRepository.GetByIdAsync(id, cancellationToken);

        if (paymentMethod == null || paymentMethod.UserId != userId)
            throw new KeyNotFoundException("Ödeme yöntemi bulunamadı");

        if (!string.IsNullOrWhiteSpace(request.Brand))
        {
            paymentMethod.Brand = ParseBrand(request.Brand);
        }

        if (!string.IsNullOrWhiteSpace(request.LastFour))
        {
            paymentMethod.Last4 = NormalizeLastFour(request.LastFour);
        }

        if (!string.IsNullOrWhiteSpace(request.ExpiryDate))
        {
            var (month, year) = ParseExpiry(request.ExpiryDate);
            paymentMethod.ExpiryMonth = month;
            paymentMethod.ExpiryYear = year;
        }

        await _paymentMethodRepository.UpdateAsync(paymentMethod, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToResponse(paymentMethod);
    }

    public async Task DeletePaymentMethodAsync(Guid userId, Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting payment method {Id} for user {UserId}", id, userId);

        var paymentMethod = await _paymentMethodRepository.GetByIdAsync(id, cancellationToken);

        if (paymentMethod == null || paymentMethod.UserId != userId)
            throw new KeyNotFoundException("Ödeme yöntemi bulunamadı");

        await _paymentMethodRepository.DeleteAsync(paymentMethod, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task SetDefaultPaymentMethodAsync(Guid userId, Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Setting default payment method {Id} for user {UserId}", id, userId);

        var paymentMethod = await _paymentMethodRepository.GetByIdAsync(id, cancellationToken);

        if (paymentMethod == null || paymentMethod.UserId != userId)
            throw new KeyNotFoundException("Ödeme yöntemi bulunamadı");

        await _paymentMethodRepository.SetDefaultAsync(userId, id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static PaymentMethodResponse MapToResponse(PaymentMethod paymentMethod)
    {
        return new PaymentMethodResponse(
            Id: paymentMethod.Id,
            Brand: paymentMethod.Brand.ToString().ToLowerInvariant(),
            LastFour: paymentMethod.Last4,
            ExpiryDate: $"{paymentMethod.ExpiryMonth:00}/{paymentMethod.ExpiryYear}",
            IsDefault: paymentMethod.IsDefault
        );
    }

    private static string NormalizeLastFour(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "0000";
        }

        var digits = new string(value.Where(char.IsDigit).ToArray());
        if (digits.Length >= 4)
        {
            return digits[^4..];
        }

        return digits.PadLeft(4, '0');
    }

    private static PaymentBrand ParseBrand(string? brand)
    {
        if (string.IsNullOrWhiteSpace(brand))
        {
            return PaymentBrand.Visa;
        }

        return brand.Trim().ToLowerInvariant() switch
        {
            "visa" => PaymentBrand.Visa,
            "mastercard" => PaymentBrand.Mastercard,
            "amex" or "americanexpress" => PaymentBrand.Amex,
            _ => PaymentBrand.Visa
        };
    }

    private static (int month, int year) ParseExpiry(string? expiryDate)
    {
        if (string.IsNullOrWhiteSpace(expiryDate))
        {
            var now = DateTime.UtcNow;
            return (now.Month, now.Year + 2);
        }

        var parts = expiryDate.Split('/', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2 || !int.TryParse(parts[0], out var month) || !int.TryParse(parts[1], out var year))
        {
            throw new ArgumentException("expiryDate MM/YYYY formatında olmalıdır");
        }

        if (month < 1 || month > 12)
        {
            throw new ArgumentException("Geçersiz son kullanma ayı");
        }

        if (year < DateTime.UtcNow.Year)
        {
            throw new ArgumentException("Kartınızın süresi dolmuş");
        }

        return (month, year);
    }
}
