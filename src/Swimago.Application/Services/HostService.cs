using Microsoft.Extensions.Logging;
using Swimago.Application.DTOs.Host;
using Swimago.Application.Interfaces;
using Swimago.Domain.Enums;
using Swimago.Domain.Interfaces;

namespace Swimago.Application.Services;

public class HostService : IHostService
{
    private readonly IListingRepository _listingRepository;
    private readonly IReservationRepository _reservationRepository;
    private readonly ILogger<HostService> _logger;

    public HostService(
        IListingRepository listingRepository,
        IReservationRepository reservationRepository,
        ILogger<HostService> logger)
    {
        _listingRepository = listingRepository;
        _reservationRepository = reservationRepository;
        _logger = logger;
    }

    public async Task<HostDashboardResponse> GetDashboardAsync(Guid hostId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting dashboard for host {HostId}", hostId);

        var listings = await _listingRepository.GetByHostIdAsync(hostId, cancellationToken);
        var listingIds = listings.Select(l => l.Id).ToHashSet();
        var reservations = await _reservationRepository.GetByListingIdsAsync(listingIds, cancellationToken);

        var pendingCount = reservations.Count(r => r.Status == ReservationStatus.Pending);
        var activeListingCount = listings.Count(l => l.Status == ListingStatus.Active);
        var totalReservations = reservations.Count();
        
        var stats = new HostStatsDto(
            TotalListings: listings.Count(),
            ActiveListings: activeListingCount,
            PendingListings: listings.Count(l => l.Status == ListingStatus.Pending),
            TotalReservations: totalReservations,
            PendingReservations: pendingCount,
            TodayReservations: 0, 
            AverageRating: 4.8m,
            TotalReviews: 0
        );

        var earnings = new HostEarningsDto(
            TotalEarnings: reservations.Where(r => r.Status == ReservationStatus.Completed).Sum(r => r.TotalPrice),
            ThisMonthEarnings: 0,
            LastMonthEarnings: 0,
            PendingPayouts: 0,
            Currency: "USD"
        );

        return new HostDashboardResponse(stats, new List<RecentReservationDto>(), new List<RecentReviewDto>(), earnings);
    }

    public async Task<HostListingListResponse> GetMyListingsAsync(Guid hostId, CancellationToken cancellationToken = default)
    {
        var listings = await _listingRepository.GetByHostIdAsync(hostId, cancellationToken);
        
        var items = listings.Select(l => new HostListingItemDto(
            Id: l.Id,
            Name: GetTitle(l.Title),
            Slug: l.Slug,
            VenueType: MapVenueType(l.Type),
            ImageUrl: null,
            Status: l.Status,
            BasePricePerDay: l.BasePricePerDay,
            Currency: l.PriceCurrency,
            Rating: l.Rating,
            ReviewCount: l.ReviewCount,
            ReservationCount: 0,
            TotalEarnings: 0,
            CreatedAt: l.CreatedAt
        )).ToList();

        return new HostListingListResponse(
            Listings: items,
            TotalCount: items.Count,
            ActiveCount: items.Count(i => i.Status == ListingStatus.Active),
            PendingCount: items.Count(i => i.Status == ListingStatus.Pending),
            InactiveCount: items.Count(i => i.Status == ListingStatus.Inactive)
        );
    }

    public async Task<HostListingItemDto> GetListingAsync(Guid hostId, Guid listingId, CancellationToken cancellationToken = default)
    {
        var listing = await _listingRepository.GetByIdAsync(listingId, cancellationToken);
        if (listing == null || listing.HostId != hostId)
            throw new KeyNotFoundException("İlan bulunamadı");

        return new HostListingItemDto(
            Id: listing.Id,
            Name: GetTitle(listing.Title),
            Slug: listing.Slug,
            VenueType: MapVenueType(listing.Type),
            ImageUrl: null,
            Status: listing.Status,
            BasePricePerDay: listing.BasePricePerDay,
            Currency: listing.PriceCurrency,
            Rating: listing.Rating,
            ReviewCount: listing.ReviewCount,
            ReservationCount: 0,
            TotalEarnings: 0,
            CreatedAt: listing.CreatedAt
        );
    }

    public async Task UpdateListingAsync(Guid hostId, Guid listingId, UpdateListingRequest request, CancellationToken cancellationToken = default)
    {
        var listing = await _listingRepository.GetByIdAsync(listingId, cancellationToken);
        if (listing == null || listing.HostId != hostId)
            throw new KeyNotFoundException("İlan bulunamadı");

        if (listing.Title == null) listing.Title = new Dictionary<string, string>();
        listing.Title["tr"] = request.Name;

        if (listing.Description == null) listing.Description = new Dictionary<string, string>();
        listing.Description["tr"] = request.Description;

        if (request.Capacity.HasValue)
            listing.MaxGuestCount = request.Capacity.Value;
            
        listing.BasePricePerDay = request.Price;
        
        await _listingRepository.UpdateAsync(listing, cancellationToken);
    }

    public async Task UpdatePricingAsync(Guid hostId, Guid listingId, UpdatePricingRequest request, CancellationToken cancellationToken = default)
    {
         var listing = await _listingRepository.GetByIdAsync(listingId, cancellationToken);
        if (listing == null || listing.HostId != hostId)
            throw new KeyNotFoundException("İlan bulunamadı");
            
        if (request.BasePricePerDay.HasValue)
            listing.BasePricePerDay = request.BasePricePerDay.Value;

        await _listingRepository.UpdateAsync(listing, cancellationToken);
    }

    public async Task DeleteListingAsync(Guid hostId, Guid listingId, CancellationToken cancellationToken = default)
    {
        var listing = await _listingRepository.GetByIdAsync(listingId, cancellationToken);
        if (listing == null || listing.HostId != hostId)
            throw new KeyNotFoundException("İlan bulunamadı");

        listing.Status = ListingStatus.Inactive;
        await _listingRepository.UpdateAsync(listing, cancellationToken);
    }

    public async Task<HostReservationListResponse> GetReservationsAsync(Guid hostId, ReservationStatus? status, CancellationToken cancellationToken = default)
    {
        var listings = await _listingRepository.GetByHostIdAsync(hostId, cancellationToken);
        var listingIds = listings.Select(l => l.Id).ToHashSet();
        var reservations = await _reservationRepository.GetByListingIdsAsync(listingIds, cancellationToken);

        if (status.HasValue)
            reservations = reservations.Where(r => r.Status == status.Value).ToList();

        var items = reservations.Select(r => new HostReservationItemDto(
            Id: r.Id,
            ConfirmationNumber: r.Id.ToString()[..8].ToUpper(),
            ListingId: r.ListingId,
            ListingName: GetTitle(r.Listing?.Title),
            GuestId: r.GuestId,
            GuestName: GetGuestName(r),
            GuestAvatar: r.Guest?.Profile?.Avatar,
            GuestPhone: r.Guest?.Profile?.PhoneNumber,
            StartTime: r.StartTime,
            EndTime: r.EndTime,
            GuestCount: r.GuestCount,
            TotalPrice: r.TotalPrice,
            Currency: "USD",
            Status: r.Status,
            SpecialRequests: GetTitle(r.SpecialRequests),
            CreatedAt: r.CreatedAt
        )).ToList();

        var counts = new HostReservationCountsDto(
            Total: reservations.Count(),
            Pending: reservations.Where(r => r.Status == ReservationStatus.Pending).Count(),
            Confirmed: reservations.Where(r => r.Status == ReservationStatus.Confirmed).Count(),
            CheckedIn: reservations.Where(r => r.Status == ReservationStatus.InProgress).Count(),
            Completed: reservations.Where(r => r.Status == ReservationStatus.Completed).Count(),
            Cancelled: reservations.Where(r => r.Status == ReservationStatus.Cancelled).Count()
        );

        return new HostReservationListResponse(items, counts, items.Count);
    }

    public async Task UpdateReservationStatusAsync(Guid hostId, Guid reservationId, UpdateReservationStatusRequest request, CancellationToken cancellationToken = default)
    {
        var reservation = await _reservationRepository.GetByIdAsync(reservationId, cancellationToken);
        if (reservation == null)
            throw new KeyNotFoundException("Rezervasyon bulunamadı");

        var listing = await _listingRepository.GetByIdAsync(reservation.ListingId, cancellationToken);
        if (listing == null || listing.HostId != hostId)
            throw new UnauthorizedAccessException("Bu işlem için yetkiniz yok");

        reservation.Status = request.Status;
        await _reservationRepository.UpdateAsync(reservation, cancellationToken);
    }

    public async Task<HostCalendarResponse> GetCalendarAsync(Guid hostId, Guid listingId, DateTime start, DateTime end, CancellationToken cancellationToken = default)
    {
         var listing = await _listingRepository.GetByIdAsync(listingId, cancellationToken);
         if (listing == null || listing.HostId != hostId)
            throw new KeyNotFoundException("İlan bulunamadı");
            
        return new HostCalendarResponse(listing.Id, GetTitle(listing.Title), new List<CalendarDayDto>());
    }

    public async Task UpdateCalendarAsync(Guid hostId, UpdateCalendarRequest request, CancellationToken cancellationToken = default)
    {
        // Mock implementation
        await Task.CompletedTask;
    }

    public async Task<HostAnalyticsResponse> GetAnalyticsAsync(Guid hostId, DateTime start, DateTime end, CancellationToken cancellationToken = default)
    {
         return await Task.FromResult(new HostAnalyticsResponse(
             new AnalyticsPeriodDto(0, 0, 0, 0, 0),
             new AnalyticsPeriodDto(0, 0, 0, 0, 0),
             new List<DailyAnalyticsDto>(),
             new List<ListingAnalyticsDto>()
         ));
    }

    private string GetTitle(Dictionary<string, string>? titleDict)
    {
        if (titleDict == null || !titleDict.Any()) return "";
        if (titleDict.ContainsKey("tr")) return titleDict["tr"];
        if (titleDict.ContainsKey("en")) return titleDict["en"];
        return titleDict.Values.First();
    }
    
    private string GetGuestName(Swimago.Domain.Entities.Reservation reservation)
    {
        if (reservation.Guest?.Profile == null) return "Guest";
        var first = GetTitle(reservation.Guest.Profile.FirstName);
        var last = GetTitle(reservation.Guest.Profile.LastName);
        return $"{first} {last}".Trim();
    }

    private VenueType MapVenueType(ListingType type)
    {
        return type switch
        {
            ListingType.Beach => VenueType.Beach,
            ListingType.Pool => VenueType.Pool,
            ListingType.Yacht => VenueType.Yacht,
            ListingType.DayTrip => VenueType.DayTrip,
            _ => VenueType.Beach
        };
    }
}
