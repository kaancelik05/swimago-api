using Swimago.Domain.Entities;

namespace Swimago.Domain.Interfaces;

public interface IDailyPricingRepository : IRepository<DailyPricing>
{
    Task<IReadOnlyCollection<DailyPricing>> GetByListingAndDateRangeAsync(
        Guid listingId,
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default);
}
