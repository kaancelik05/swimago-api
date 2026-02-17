using Swimago.Domain.Entities;

namespace Swimago.Domain.Interfaces;

public interface IHostListingMetadataRepository : IRepository<HostListingMetadata>
{
    Task<HostListingMetadata?> GetByListingIdAsync(Guid listingId, CancellationToken cancellationToken = default);
    Task<IReadOnlyDictionary<Guid, HostListingMetadata>> GetByListingIdsAsync(IEnumerable<Guid> listingIds, CancellationToken cancellationToken = default);
}
