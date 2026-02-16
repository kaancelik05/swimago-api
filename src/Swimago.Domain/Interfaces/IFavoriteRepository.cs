using Swimago.Domain.Entities;

namespace Swimago.Domain.Interfaces;

public interface IFavoriteRepository : IRepository<Favorite>
{
    Task<IEnumerable<Favorite>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Favorite?> GetByUserAndVenueAsync(Guid userId, Guid venueId, CancellationToken cancellationToken = default);
    Task<bool> IsFavoriteAsync(Guid userId, Guid venueId, CancellationToken cancellationToken = default);
}
