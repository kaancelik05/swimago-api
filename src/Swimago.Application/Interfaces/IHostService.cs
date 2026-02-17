using Swimago.Application.DTOs.Host;

namespace Swimago.Application.Interfaces;

public interface IHostService
{
    Task<HostListingsResponse> GetListingsAsync(
        Guid hostId,
        string? status,
        string? type,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<HostListingDto> GetListingAsync(Guid hostId, Guid listingId, CancellationToken cancellationToken = default);

    Task<HostListingDto> CreateListingAsync(Guid hostId, UpsertHostListingRequest request, CancellationToken cancellationToken = default);

    Task<HostListingDto> UpdateListingAsync(Guid hostId, Guid listingId, UpsertHostListingRequest request, CancellationToken cancellationToken = default);

    Task UpdateListingStatusAsync(Guid hostId, Guid listingId, UpdateHostListingStatusRequest request, CancellationToken cancellationToken = default);

    Task<DashboardStatsDto> GetDashboardStatsAsync(Guid hostId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<HostReservationDto>> GetRecentReservationsAsync(Guid hostId, int limit, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<HostInsightDto>> GetInsightsAsync(Guid hostId, CancellationToken cancellationToken = default);

    Task<HostReservationsResponse> GetReservationsAsync(
        Guid hostId,
        string? status,
        string? source,
        string? listingId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task UpdateReservationStatusAsync(Guid hostId, Guid reservationId, UpdateHostReservationStatusRequest request, CancellationToken cancellationToken = default);

    Task<HostReservationDto> CreateManualReservationAsync(Guid hostId, CreateManualReservationRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<CalendarDayDto>> GetCalendarAsync(Guid hostId, Guid listingId, int month, int year, CancellationToken cancellationToken = default);

    Task UpdateCalendarAsync(Guid hostId, UpdateCalendarRequest request, CancellationToken cancellationToken = default);

    Task<HostAnalyticsDto> GetAnalyticsAsync(Guid hostId, string period, string? listingId, CancellationToken cancellationToken = default);

    Task<BusinessSettingsDto> GetBusinessSettingsAsync(Guid hostId, CancellationToken cancellationToken = default);

    Task UpdateBusinessSettingsAsync(Guid hostId, BusinessSettingsDto request, CancellationToken cancellationToken = default);
}
