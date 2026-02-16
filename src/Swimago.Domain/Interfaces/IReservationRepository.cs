using Swimago.Domain.Entities;
using Swimago.Domain.Enums;

namespace Swimago.Domain.Interfaces;

public interface IReservationRepository : IRepository<Reservation>
{
    Task<IEnumerable<Reservation>> GetByGuestIdAsync(Guid guestId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Reservation>> GetByGuestIdAndStatusAsync(Guid guestId, ReservationStatus? status = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<Reservation>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Reservation>> GetByListingIdsAsync(IEnumerable<Guid> listingIds, CancellationToken cancellationToken = default);
    Task<IEnumerable<Reservation>> GetByVenueIdAsync(Guid venueId, CancellationToken cancellationToken = default);
    Task<Reservation?> GetWithDetailsAsync(Guid reservationId, CancellationToken cancellationToken = default);
    Task<bool> HasOverlappingReservationsAsync(Guid listingId, DateTime startTime, DateTime endTime, Guid? excludeReservationId = null, CancellationToken cancellationToken = default);
    Task<IDictionary<ReservationStatus, int>> GetStatusCountsAsync(Guid guestId, CancellationToken cancellationToken = default);
}
