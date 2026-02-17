using Microsoft.EntityFrameworkCore;
using Swimago.Domain.Entities;
using Swimago.Domain.Interfaces;
using Swimago.Infrastructure.Data;

namespace Swimago.Infrastructure.Repositories;

public class DailyPricingRepository : Repository<DailyPricing>, IDailyPricingRepository
{
    public DailyPricingRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyCollection<DailyPricing>> GetByListingAndDateRangeAsync(
        Guid listingId,
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(x => x.ListingId == listingId && x.Date >= startDate && x.Date <= endDate)
            .OrderBy(x => x.Date)
            .ToListAsync(cancellationToken);
    }
}
