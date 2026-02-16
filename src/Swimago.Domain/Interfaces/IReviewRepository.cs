using Swimago.Domain.Entities;

namespace Swimago.Domain.Interfaces;

public interface IReviewRepository : IRepository<Review>
{
    Task<IEnumerable<Review>> GetByListingIdAsync(Guid listingId, CancellationToken cancellationToken = default);
    Task<Review?> GetByReservationIdAsync(Guid reservationId, CancellationToken cancellationToken = default);
    Task<bool> HasUserReviewedReservationAsync(Guid reservationId, CancellationToken cancellationToken = default);
}
