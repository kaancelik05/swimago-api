using Swimago.Application.DTOs.Reservations;

namespace Swimago.Application.Interfaces;

public interface IReservationService
{
    Task<ReservationResponse> CreateReservationAsync(Guid guestId, CreateReservationRequest request, CancellationToken cancellationToken = default);
    Task<ReservationResponse?> GetReservationByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<ReservationResponse>> GetUserReservationsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> CancelReservationAsync(Guid reservationId, Guid userId, CancellationToken cancellationToken = default);
    Task<bool> CheckAvailabilityAsync(Guid listingId, DateTime startTime, DateTime endTime, CancellationToken cancellationToken = default);
}
