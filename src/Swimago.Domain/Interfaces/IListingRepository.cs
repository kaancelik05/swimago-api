using Swimago.Domain.Entities;
using Swimago.Domain.Enums;

namespace Swimago.Domain.Interfaces;

public interface IListingRepository : IRepository<Listing>
{
    Task<IEnumerable<Listing>> GetActiveListingsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Listing>> GetByHostIdAsync(Guid hostId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Listing>> GetByTypeAsync(ListingType type, CancellationToken cancellationToken = default);
    Task<Listing?> GetWithDetailsAsync(Guid listingId, CancellationToken cancellationToken = default);
    Task<Listing?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<IEnumerable<Listing>> SearchNearbyAsync(decimal latitude, decimal longitude, decimal radiusKm, ListingType? type = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<Listing>> GetByStatusAsync(ListingStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<Listing>> GetFeaturedAsync(int limit = 10, CancellationToken cancellationToken = default);
}
