using Microsoft.EntityFrameworkCore;
using Swimago.Domain.Entities;
using Swimago.Domain.Interfaces;
using Swimago.Infrastructure.Data;

namespace Swimago.Infrastructure.Repositories;

public class PaymentMethodRepository : Repository<PaymentMethod>, IPaymentMethodRepository
{
    public PaymentMethodRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<PaymentMethod>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(pm => pm.UserId == userId)
            .OrderByDescending(pm => pm.IsDefault)
            .ThenByDescending(pm => pm.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<PaymentMethod?> GetDefaultByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(pm => pm.UserId == userId && pm.IsDefault, cancellationToken);
    }

    public async Task SetDefaultAsync(Guid userId, Guid paymentMethodId, CancellationToken cancellationToken = default)
    {
        var paymentMethods = await _dbSet.Where(pm => pm.UserId == userId).ToListAsync(cancellationToken);
        foreach (var pm in paymentMethods)
        {
            pm.IsDefault = pm.Id == paymentMethodId;
        }
    }
}
