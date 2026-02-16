using Swimago.Application.DTOs.Host;
using Swimago.Domain.Enums;

namespace Swimago.Application.Interfaces;

public interface IHostService
{
    Task<HostDashboardResponse> GetDashboardAsync(Guid hostId, CancellationToken cancellationToken = default);
    Task<HostListingListResponse> GetMyListingsAsync(Guid hostId, CancellationToken cancellationToken = default);
    Task<HostListingItemDto> GetListingAsync(Guid hostId, Guid listingId, CancellationToken cancellationToken = default);
    Task UpdateListingAsync(Guid hostId, Guid listingId, UpdateListingRequest request, CancellationToken cancellationToken = default);
    Task UpdatePricingAsync(Guid hostId, Guid listingId, UpdatePricingRequest request, CancellationToken cancellationToken = default);
    Task DeleteListingAsync(Guid hostId, Guid listingId, CancellationToken cancellationToken = default);
    Task<HostReservationListResponse> GetReservationsAsync(Guid hostId, ReservationStatus? status, CancellationToken cancellationToken = default);
    Task UpdateReservationStatusAsync(Guid hostId, Guid reservationId, UpdateReservationStatusRequest request, CancellationToken cancellationToken = default);
    Task<HostCalendarResponse> GetCalendarAsync(Guid hostId, Guid listingId, DateTime start, DateTime end, CancellationToken cancellationToken = default);
    Task UpdateCalendarAsync(Guid hostId, UpdateCalendarRequest request, CancellationToken cancellationToken = default);
    Task<HostAnalyticsResponse> GetAnalyticsAsync(Guid hostId, DateTime start, DateTime end, CancellationToken cancellationToken = default);
}
