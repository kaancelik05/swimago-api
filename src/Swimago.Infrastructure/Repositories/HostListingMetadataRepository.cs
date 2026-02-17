using Microsoft.EntityFrameworkCore;
using Swimago.Domain.Entities;
using Swimago.Domain.Interfaces;
using Swimago.Infrastructure.Data;

namespace Swimago.Infrastructure.Repositories;

public class HostListingMetadataRepository : Repository<HostListingMetadata>, IHostListingMetadataRepository
{
    public HostListingMetadataRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<HostListingMetadata?> GetByListingIdAsync(Guid listingId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(x => x.ListingId == listingId, cancellationToken);
    }

    public async Task<IReadOnlyDictionary<Guid, HostListingMetadata>> GetByListingIdsAsync(IEnumerable<Guid> listingIds, CancellationToken cancellationToken = default)
    {
        var ids = listingIds.Distinct().ToList();
        if (ids.Count == 0)
        {
            return new Dictionary<Guid, HostListingMetadata>();
        }

        var items = await _dbSet
            .Where(x => ids.Contains(x.ListingId))
            .ToListAsync(cancellationToken);

        return items.ToDictionary(x => x.ListingId, x => x);
    }
}
