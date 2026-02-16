using Swimago.Application.DTOs.PaymentMethods;

namespace Swimago.Application.Interfaces;

public interface IPaymentMethodService
{
    Task<PaymentMethodListResponse> GetPaymentMethodsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<PaymentMethodResponse> AddPaymentMethodAsync(Guid userId, AddPaymentMethodRequest request, CancellationToken cancellationToken = default);
    Task DeletePaymentMethodAsync(Guid userId, Guid id, CancellationToken cancellationToken = default);
    Task SetDefaultPaymentMethodAsync(Guid userId, Guid id, CancellationToken cancellationToken = default);
}
