using Swimago.Domain.Entities;

namespace Swimago.Domain.Interfaces;

public interface IPaymentMethodRepository : IRepository<PaymentMethod>
{
    Task<IEnumerable<PaymentMethod>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<PaymentMethod?> GetDefaultByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task SetDefaultAsync(Guid userId, Guid paymentMethodId, CancellationToken cancellationToken = default);
}
