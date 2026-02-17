using Swimago.Application.DTOs.Host;
using Swimago.Application.Interfaces;
using Swimago.Domain.Entities;
using Swimago.Domain.Enums;
using Swimago.Domain.Interfaces;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Swimago.Application.Services;

public class HostService : IHostService
{
    private const string FallbackImageUrl = "https://images.unsplash.com/photo-1507525428034-b723cf961d3e?auto=format&fit=crop&w=1200&q=80";

    private readonly IListingRepository _listingRepository;
    private readonly IReservationRepository _reservationRepository;
    private readonly IHostListingMetadataRepository _hostListingMetadataRepository;
    private readonly IHostBusinessSettingsRepository _hostBusinessSettingsRepository;
    private readonly IDailyPricingRepository _dailyPricingRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public HostService(
        IListingRepository listingRepository,
        IReservationRepository reservationRepository,
        IHostListingMetadataRepository hostListingMetadataRepository,
        IHostBusinessSettingsRepository hostBusinessSettingsRepository,
        IDailyPricingRepository dailyPricingRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _listingRepository = listingRepository;
        _reservationRepository = reservationRepository;
        _hostListingMetadataRepository = hostListingMetadataRepository;
        _hostBusinessSettingsRepository = hostBusinessSettingsRepository;
        _dailyPricingRepository = dailyPricingRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<HostListingsResponse> GetListingsAsync(
        Guid hostId,
        string? status,
        string? type,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var safePage = Math.Max(1, page);
        var safePageSize = Math.Clamp(pageSize, 1, 100);

        var listings = (await _listingRepository.GetByHostIdAsync(hostId, cancellationToken)).ToList();

        if (!string.IsNullOrWhiteSpace(status) && !string.Equals(status, "all", StringComparison.OrdinalIgnoreCase))
        {
            var parsedStatus = ParseListingStatus(status);
            listings = listings.Where(x => x.Status == parsedStatus).ToList();
        }

        if (!string.IsNullOrWhiteSpace(type) && !string.Equals(type, "all", StringComparison.OrdinalIgnoreCase))
        {
            var parsedType = ParseListingType(type);
            listings = listings.Where(x => x.Type == parsedType).ToList();
        }

        var listingIds = listings.Select(x => x.Id).ToList();
        var metadataByListing = await _hostListingMetadataRepository.GetByListingIdsAsync(listingIds, cancellationToken);
        var reservations = listingIds.Count == 0
            ? new List<Reservation>()
            : (await _reservationRepository.GetByListingIdsAsync(listingIds, cancellationToken)).ToList();
        var reservationsByListing = reservations.GroupBy(x => x.ListingId).ToDictionary(x => x.Key, x => x.ToList());

        var totalCount = listings.Count;
        var pagedListings = listings
            .OrderByDescending(x => x.CreatedAt)
            .Skip((safePage - 1) * safePageSize)
            .Take(safePageSize)
            .Select(x =>
            {
                metadataByListing.TryGetValue(x.Id, out var metadata);
                return MapListing(x, metadata, reservationsByListing);
            })
            .ToList();

        return new HostListingsResponse(pagedListings, totalCount, safePage, safePageSize);
    }

    public async Task<HostListingDto> GetListingAsync(Guid hostId, Guid listingId, CancellationToken cancellationToken = default)
    {
        var listing = await _listingRepository.GetWithDetailsAsync(listingId, cancellationToken);
        if (listing == null)
        {
            throw new KeyNotFoundException("İlan bulunamadı");
        }

        if (listing.HostId != hostId)
        {
            throw new UnauthorizedAccessException("Bu ilana erişim yetkiniz yok");
        }

        var metadata = await _hostListingMetadataRepository.GetByListingIdAsync(listing.Id, cancellationToken);
        var reservations = (await _reservationRepository.GetByListingIdsAsync([listing.Id], cancellationToken)).ToList();
        var reservationsByListing = reservations.GroupBy(x => x.ListingId).ToDictionary(x => x.Key, x => x.ToList());
        return MapListing(listing, metadata, reservationsByListing);
    }

    public async Task<HostListingDto> CreateListingAsync(Guid hostId, UpsertHostListingRequest request, CancellationToken cancellationToken = default)
    {
        ValidateListingRequest(request);

        var now = DateTime.UtcNow;
        var listing = new Listing
        {
            Id = Guid.NewGuid(),
            HostId = hostId,
            Type = ParseListingType(request.Type),
            Status = ParseListingStatus(request.Status),
            IsActive = ParseListingStatus(request.Status) == ListingStatus.Active,
            IsFeatured = false,
            Slug = await GenerateUniqueSlugAsync(request.Name, cancellationToken),
            Title = new Dictionary<string, string> { ["tr"] = request.Name.Trim() },
            Description = new Dictionary<string, string> { ["tr"] = request.Name.Trim() },
            Address = new Dictionary<string, string> { ["tr"] = request.City.Trim() },
            City = request.City.Trim(),
            Country = "Turkey",
            Latitude = 0,
            Longitude = 0,
            MaxGuestCount = Math.Max(1, request.Capacity),
            BasePricePerHour = request.BasePrice,
            BasePricePerDay = request.BasePrice,
            PriceCurrency = NormalizeCurrency(request.Currency),
            CreatedAt = now,
            Rating = 0,
            ReviewCount = 0,
            SpotCount = 0,
            IsSuperhost = false
        };

        if (!string.IsNullOrWhiteSpace(request.ImageUrl))
        {
            listing.Images.Add(new ListingImage
            {
                Id = Guid.NewGuid(),
                ListingId = listing.Id,
                Url = request.ImageUrl.Trim(),
                IsCover = true,
                DisplayOrder = 0
            });
        }

        var metadata = BuildListingMetadata(listing.Id, request, now);

        await _listingRepository.AddAsync(listing, cancellationToken);
        await _hostListingMetadataRepository.AddAsync(metadata, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetListingAsync(hostId, listing.Id, cancellationToken);
    }

    public async Task<HostListingDto> UpdateListingAsync(Guid hostId, Guid listingId, UpsertHostListingRequest request, CancellationToken cancellationToken = default)
    {
        ValidateListingRequest(request);

        var listing = await _listingRepository.GetWithDetailsAsync(listingId, cancellationToken);
        if (listing == null)
        {
            throw new KeyNotFoundException("İlan bulunamadı");
        }

        if (listing.HostId != hostId)
        {
            throw new UnauthorizedAccessException("Bu ilana erişim yetkiniz yok");
        }

        listing.Type = ParseListingType(request.Type);
        listing.Status = ParseListingStatus(request.Status);
        listing.IsActive = listing.Status == ListingStatus.Active;
        listing.Title["tr"] = request.Name.Trim();
        listing.Description["tr"] = request.Name.Trim();
        listing.Address["tr"] = request.City.Trim();
        listing.City = request.City.Trim();
        listing.MaxGuestCount = Math.Max(1, request.Capacity);
        listing.BasePricePerHour = request.BasePrice;
        listing.BasePricePerDay = request.BasePrice;
        listing.PriceCurrency = NormalizeCurrency(request.Currency);
        listing.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(request.ImageUrl))
        {
            var coverImage = listing.Images
                .OrderByDescending(x => x.IsCover)
                .ThenBy(x => x.DisplayOrder)
                .FirstOrDefault();

            if (coverImage == null)
            {
                listing.Images.Add(new ListingImage
                {
                    Id = Guid.NewGuid(),
                    ListingId = listing.Id,
                    Url = request.ImageUrl.Trim(),
                    IsCover = true,
                    DisplayOrder = 0
                });
            }
            else
            {
                coverImage.Url = request.ImageUrl.Trim();
                coverImage.IsCover = true;
            }
        }

        var metadata = await _hostListingMetadataRepository.GetByListingIdAsync(listingId, cancellationToken);
        if (metadata == null)
        {
            metadata = BuildListingMetadata(listing.Id, request, DateTime.UtcNow);
            await _hostListingMetadataRepository.AddAsync(metadata, cancellationToken);
        }
        else
        {
            metadata.Highlights = SanitizeHighlights(request.Highlights);
            metadata.SeatingAreas = SanitizeSeatingAreas(request.SeatingAreas);
            metadata.AvailabilityNotes = NormalizeOptionalText(request.AvailabilityNotes);
            metadata.UpdatedAt = DateTime.UtcNow;
            await _hostListingMetadataRepository.UpdateAsync(metadata, cancellationToken);
        }

        await _listingRepository.UpdateAsync(listing, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetListingAsync(hostId, listing.Id, cancellationToken);
    }

    public async Task UpdateListingStatusAsync(Guid hostId, Guid listingId, UpdateHostListingStatusRequest request, CancellationToken cancellationToken = default)
    {
        var listing = await _listingRepository.GetByIdAsync(listingId, cancellationToken);
        if (listing == null)
        {
            throw new KeyNotFoundException("İlan bulunamadı");
        }

        if (listing.HostId != hostId)
        {
            throw new UnauthorizedAccessException("Bu ilana erişim yetkiniz yok");
        }

        listing.Status = ParseListingStatus(request.Status);
        listing.IsActive = listing.Status == ListingStatus.Active;
        listing.UpdatedAt = DateTime.UtcNow;

        await _listingRepository.UpdateAsync(listing, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync(Guid hostId, CancellationToken cancellationToken = default)
    {
        var listings = (await _listingRepository.GetByHostIdAsync(hostId, cancellationToken)).ToList();
        var listingIds = listings.Select(x => x.Id).ToList();
        var reservations = listingIds.Count == 0
            ? new List<Reservation>()
            : (await _reservationRepository.GetByListingIdsAsync(listingIds, cancellationToken)).ToList();

        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var monthEnd = monthStart.AddMonths(1);

        var totalRevenue = reservations
            .Where(x => IsRevenueStatus(x.Status))
            .Sum(x => x.TotalPrice);

        var monthlyRevenue = reservations
            .Where(x => IsRevenueStatus(x.Status))
            .Where(x => x.StartTime >= monthStart && x.StartTime < monthEnd)
            .Sum(x => x.TotalPrice);

        return new DashboardStatsDto(
            TotalListings: listings.Count,
            ActiveListings: listings.Count(x => x.Status == ListingStatus.Active),
            PendingReservations: reservations.Count(x => x.Status == ReservationStatus.Pending),
            UpcomingReservations: reservations.Count(x =>
                x.StartTime.Date >= DateTime.UtcNow.Date &&
                x.Status != ReservationStatus.Cancelled &&
                x.Status != ReservationStatus.Rejected),
            TotalRevenue: totalRevenue,
            MonthlyRevenue: monthlyRevenue
        );
    }

    public async Task<IReadOnlyCollection<HostReservationDto>> GetRecentReservationsAsync(Guid hostId, int limit, CancellationToken cancellationToken = default)
    {
        var safeLimit = Math.Clamp(limit, 1, 50);
        var reservations = await GetHostReservationsAsync(hostId, cancellationToken);

        return reservations
            .OrderByDescending(x => x.CreatedAt)
            .Take(safeLimit)
            .Select(MapReservation)
            .ToList();
    }

    public async Task<IReadOnlyCollection<HostInsightDto>> GetInsightsAsync(Guid hostId, CancellationToken cancellationToken = default)
    {
        var reservations = await GetHostReservationsAsync(hostId, cancellationToken);
        var total = reservations.Count;
        var pendingCount = reservations.Count(x => x.Status == ReservationStatus.Pending);
        var offlineCount = reservations.Count(x => x.Source == ReservationSource.Phone || x.Source == ReservationSource.WalkIn);
        var cancelledCount = reservations.Count(x => x.Status == ReservationStatus.Cancelled || x.Status == ReservationStatus.Rejected);

        var offlineRatio = total == 0 ? 0m : decimal.Round((offlineCount * 100m) / total, 2);
        var cancellationRate = total == 0 ? 0m : decimal.Round((cancelledCount * 100m) / total, 2);

        return
        [
            new HostInsightDto(
                Id: "waitlist",
                TitleKey: "host.insights.waitlist.title",
                DescriptionKey: "host.insights.waitlist.description",
                DescriptionParams: new Dictionary<string, object?> { ["pendingCount"] = pendingCount },
                Level: pendingCount >= 10 ? "warning" : "info"),
            new HostInsightDto(
                Id: "offline-demand",
                TitleKey: "host.insights.offlineDemand.title",
                DescriptionKey: "host.insights.offlineDemand.description",
                DescriptionParams: new Dictionary<string, object?> { ["ratio"] = offlineRatio },
                Level: offlineRatio >= 40 ? "success" : "info"),
            new HostInsightDto(
                Id: "churn",
                TitleKey: "host.insights.churn.title",
                DescriptionKey: "host.insights.churn.description",
                DescriptionParams: new Dictionary<string, object?> { ["cancellationRate"] = cancellationRate },
                Level: cancellationRate > 12 ? "warning" : "info")
        ];
    }

    public async Task<HostReservationsResponse> GetReservationsAsync(
        Guid hostId,
        string? status,
        string? source,
        string? listingId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var safePage = Math.Max(1, page);
        var safePageSize = Math.Clamp(pageSize, 1, 100);

        var listings = (await _listingRepository.GetByHostIdAsync(hostId, cancellationToken)).ToList();
        var listingIds = listings.Select(x => x.Id).ToHashSet();

        if (!string.IsNullOrWhiteSpace(listingId) && !string.Equals(listingId, "all", StringComparison.OrdinalIgnoreCase))
        {
            if (!Guid.TryParse(listingId, out var parsedListingId))
            {
                throw new ArgumentException("listingId geçersiz");
            }

            if (!listingIds.Contains(parsedListingId))
            {
                throw new UnauthorizedAccessException("Bu ilana erişim yetkiniz yok");
            }

            listingIds = [parsedListingId];
        }

        var reservations = listingIds.Count == 0
            ? new List<Reservation>()
            : (await _reservationRepository.GetByListingIdsAsync(listingIds, cancellationToken)).ToList();

        if (!string.IsNullOrWhiteSpace(status) && !string.Equals(status, "all", StringComparison.OrdinalIgnoreCase))
        {
            var parsedStatus = ParseReservationStatus(status);
            reservations = reservations.Where(x => x.Status == parsedStatus).ToList();
        }

        if (!string.IsNullOrWhiteSpace(source) && !string.Equals(source, "all", StringComparison.OrdinalIgnoreCase))
        {
            var parsedSource = ParseReservationSource(source, allowOnline: true);
            reservations = reservations.Where(x => x.Source == parsedSource).ToList();
        }

        reservations = reservations
            .OrderByDescending(x => x.StartTime)
            .ThenByDescending(x => x.CreatedAt)
            .ToList();

        var totalCount = reservations.Count;
        var items = reservations
            .Skip((safePage - 1) * safePageSize)
            .Take(safePageSize)
            .Select(MapReservation)
            .ToList();

        return new HostReservationsResponse(items, totalCount, safePage, safePageSize);
    }

    public async Task UpdateReservationStatusAsync(Guid hostId, Guid reservationId, UpdateHostReservationStatusRequest request, CancellationToken cancellationToken = default)
    {
        var reservation = await _reservationRepository.GetByIdAsync(reservationId, cancellationToken);
        if (reservation == null)
        {
            throw new KeyNotFoundException("Rezervasyon bulunamadı");
        }

        var listing = await _listingRepository.GetByIdAsync(reservation.ListingId, cancellationToken);
        if (listing == null)
        {
            throw new KeyNotFoundException("İlan bulunamadı");
        }

        if (listing.HostId != hostId)
        {
            throw new UnauthorizedAccessException("Bu rezervasyona erişim yetkiniz yok");
        }

        var parsedStatus = ParseReservationStatus(request.Status);
        reservation.Status = parsedStatus;

        if (parsedStatus == ReservationStatus.Confirmed)
        {
            reservation.ConfirmedAt = DateTime.UtcNow;
        }

        if (parsedStatus == ReservationStatus.Cancelled || parsedStatus == ReservationStatus.Rejected)
        {
            reservation.CancelledAt = DateTime.UtcNow;
        }

        await _reservationRepository.UpdateAsync(reservation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<HostReservationDto> CreateManualReservationAsync(Guid hostId, CreateManualReservationRequest request, CancellationToken cancellationToken = default)
    {
        var listing = await _listingRepository.GetByIdAsync(request.ListingId, cancellationToken);
        if (listing == null)
        {
            throw new KeyNotFoundException("İlan bulunamadı");
        }

        if (listing.HostId != hostId)
        {
            throw new UnauthorizedAccessException("Bu ilana rezervasyon oluşturma yetkiniz yok");
        }

        if (request.Guests <= 0)
        {
            throw new ArgumentException("guests en az 1 olmalı");
        }

        if (request.Guests > listing.MaxGuestCount)
        {
            throw new ArgumentException($"guests en fazla {listing.MaxGuestCount} olabilir");
        }

        if (request.TotalAmount < 0)
        {
            throw new ArgumentException("totalAmount negatif olamaz");
        }

        var source = ParseReservationSource(request.Source, allowOnline: false);
        var startTime = ParseReservationDateTime(request.Date, request.Time);
        var endTime = startTime.AddHours(2);

        var settings = await _hostBusinessSettingsRepository.GetByHostIdAsync(hostId, cancellationToken);
        var status = settings?.AutoConfirmReservations == true
            ? ReservationStatus.Confirmed
            : ReservationStatus.Pending;

        var guest = await GetOrCreateManualGuestAsync(request.GuestName, request.GuestPhone, cancellationToken);

        var reservation = new Reservation
        {
            Id = Guid.NewGuid(),
            ListingId = listing.Id,
            GuestId = guest.Id,
            VenueType = MapVenueType(listing.Type),
            BookingType = BookingType.Daily,
            StartTime = startTime,
            EndTime = endTime,
            GuestCount = request.Guests,
            UnitPrice = request.TotalAmount,
            UnitCount = 1,
            TotalPrice = request.TotalAmount,
            FinalPrice = request.TotalAmount,
            Currency = listing.PriceCurrency,
            Status = status,
            Source = source,
            ConfirmationNumber = GenerateConfirmationNumber(),
            CreatedAt = DateTime.UtcNow,
            SpecialRequests = string.IsNullOrWhiteSpace(request.SpecialRequests)
                ? null
                : new Dictionary<string, string> { ["tr"] = request.SpecialRequests.Trim() }
        };

        await _reservationRepository.AddAsync(reservation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var created = await _reservationRepository.GetWithDetailsAsync(reservation.Id, cancellationToken) ?? reservation;
        return MapReservation(created);
    }

    public async Task<IReadOnlyCollection<CalendarDayDto>> GetCalendarAsync(
        Guid hostId,
        Guid listingId,
        int month,
        int year,
        CancellationToken cancellationToken = default)
    {
        if (month < 1 || month > 12)
        {
            throw new ArgumentException("month 1-12 aralığında olmalı");
        }

        if (year < 2000 || year > 2100)
        {
            throw new ArgumentException("year geçersiz");
        }

        var listing = await _listingRepository.GetByIdAsync(listingId, cancellationToken);
        if (listing == null)
        {
            throw new KeyNotFoundException("İlan bulunamadı");
        }

        if (listing.HostId != hostId)
        {
            throw new UnauthorizedAccessException("Bu ilana erişim yetkiniz yok");
        }

        var startDate = new DateOnly(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        var dailyOverrides = await _dailyPricingRepository.GetByListingAndDateRangeAsync(listingId, startDate, endDate, cancellationToken);
        var overrideMap = dailyOverrides.ToDictionary(x => x.Date, x => x);

        var reservations = (await _reservationRepository.GetByListingIdsAsync([listingId], cancellationToken))
            .Where(x => x.StartTime.Date >= startDate.ToDateTime(TimeOnly.MinValue).Date)
            .Where(x => x.StartTime.Date <= endDate.ToDateTime(TimeOnly.MaxValue).Date)
            .Where(x => x.Status != ReservationStatus.Cancelled && x.Status != ReservationStatus.Rejected)
            .ToList();

        var reservationCountByDay = reservations
            .GroupBy(x => DateOnly.FromDateTime(x.StartTime))
            .ToDictionary(x => x.Key, x => x.Count());
        var guestCountByDay = reservations
            .GroupBy(x => DateOnly.FromDateTime(x.StartTime))
            .ToDictionary(x => x.Key, x => x.Sum(r => r.GuestCount));

        var days = new List<CalendarDayDto>();
        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            overrideMap.TryGetValue(date, out var dayOverride);
            reservationCountByDay.TryGetValue(date, out var reservationCount);
            guestCountByDay.TryGetValue(date, out var guestCount);

            var isAvailable = dayOverride?.IsAvailable ?? guestCount < Math.Max(1, listing.MaxGuestCount);
            var customPrice = dayOverride?.Price;

            days.Add(new CalendarDayDto(
                Date: date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                IsAvailable: isAvailable,
                ReservationCount: reservationCount,
                CustomPrice: customPrice
            ));
        }

        return days;
    }

    public async Task UpdateCalendarAsync(Guid hostId, UpdateCalendarRequest request, CancellationToken cancellationToken = default)
    {
        var listing = await _listingRepository.GetByIdAsync(request.ListingId, cancellationToken);
        if (listing == null)
        {
            throw new KeyNotFoundException("İlan bulunamadı");
        }

        if (listing.HostId != hostId)
        {
            throw new UnauthorizedAccessException("Bu ilana erişim yetkiniz yok");
        }

        if (request.Updates.Count == 0)
        {
            return;
        }

        var parsedUpdates = request.Updates
            .Select(x => new
            {
                Date = ParseDateOnly(x.Date),
                x.IsAvailable,
                x.CustomPrice
            })
            .ToList();

        var minDate = parsedUpdates.Min(x => x.Date);
        var maxDate = parsedUpdates.Max(x => x.Date);

        var existing = await _dailyPricingRepository.GetByListingAndDateRangeAsync(request.ListingId, minDate, maxDate, cancellationToken);
        var existingByDate = existing.ToDictionary(x => x.Date, x => x);

        foreach (var update in parsedUpdates)
        {
            if (existingByDate.TryGetValue(update.Date, out var existingDay))
            {
                existingDay.IsAvailable = update.IsAvailable;
                existingDay.Price = update.CustomPrice ?? listing.BasePricePerDay;
                await _dailyPricingRepository.UpdateAsync(existingDay, cancellationToken);
                continue;
            }

            await _dailyPricingRepository.AddAsync(new DailyPricing
            {
                Id = Guid.NewGuid(),
                ListingId = request.ListingId,
                Date = update.Date,
                Price = update.CustomPrice ?? listing.BasePricePerDay,
                IsAvailable = update.IsAvailable
            }, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<HostAnalyticsDto> GetAnalyticsAsync(
        Guid hostId,
        string period,
        string? listingId,
        CancellationToken cancellationToken = default)
    {
        var listings = (await _listingRepository.GetByHostIdAsync(hostId, cancellationToken)).ToList();
        if (listings.Count == 0)
        {
            return EmptyAnalytics();
        }

        if (!TryGetPeriodRange(period, out var currentStart, out var currentEnd, out var previousStart, out var previousEnd))
        {
            throw new ArgumentException("period week|month|year olmalı");
        }

        var selectedListings = listings;
        if (!string.IsNullOrWhiteSpace(listingId) && !string.Equals(listingId, "all", StringComparison.OrdinalIgnoreCase))
        {
            if (!Guid.TryParse(listingId, out var parsedListingId))
            {
                throw new ArgumentException("listingId geçersiz");
            }

            selectedListings = listings.Where(x => x.Id == parsedListingId).ToList();
            if (selectedListings.Count == 0)
            {
                throw new UnauthorizedAccessException("Bu ilana erişim yetkiniz yok");
            }
        }

        var selectedListingIds = selectedListings.Select(x => x.Id).ToList();
        var reservations = (await _reservationRepository.GetByListingIdsAsync(selectedListingIds, cancellationToken)).ToList();

        var currentPeriodReservations = reservations
            .Where(x => x.StartTime >= currentStart && x.StartTime < currentEnd)
            .ToList();
        var previousPeriodReservations = reservations
            .Where(x => x.StartTime >= previousStart && x.StartTime < previousEnd)
            .ToList();

        var currentRevenueReservations = currentPeriodReservations.Where(x => IsRevenueStatus(x.Status)).ToList();
        var previousRevenueReservations = previousPeriodReservations.Where(x => IsRevenueStatus(x.Status)).ToList();

        var totalRevenue = currentRevenueReservations.Sum(x => x.TotalPrice);
        var previousRevenue = previousRevenueReservations.Sum(x => x.TotalPrice);

        var totalReservations = currentRevenueReservations.Count;
        var previousReservations = previousRevenueReservations.Count;

        var totalCurrent = currentPeriodReservations.Count;
        var cancelledCurrent = currentPeriodReservations.Count(x => x.Status == ReservationStatus.Cancelled || x.Status == ReservationStatus.Rejected);
        var noShowCurrent = currentPeriodReservations.Count(x => x.Status == ReservationStatus.NoShow);

        var cancellationRate = totalCurrent == 0 ? 0m : decimal.Round((cancelledCurrent * 100m) / totalCurrent, 2);
        var noShowRate = totalCurrent == 0 ? 0m : decimal.Round((noShowCurrent * 100m) / totalCurrent, 2);

        var averageRating = selectedListings.Count == 0 ? 0m : decimal.Round(selectedListings.Average(x => x.Rating), 2);
        var reviewCount = selectedListings.Sum(x => x.ReviewCount);

        var occupancyRate = ComputeOccupancyRate(currentPeriodReservations, selectedListings, currentStart, currentEnd);

        var revenueSeries = BuildRevenueSeries(currentRevenueReservations, period, currentStart, currentEnd);
        var topListings = BuildTopListings(currentRevenueReservations, selectedListings, currentStart, currentEnd);

        var sourceBreakdown = new List<SourceBreakdownDto>
        {
            new("online", currentPeriodReservations.Count(x => x.Source == ReservationSource.Online)),
            new("phone", currentPeriodReservations.Count(x => x.Source == ReservationSource.Phone)),
            new("walk-in", currentPeriodReservations.Count(x => x.Source == ReservationSource.WalkIn))
        };

        return new HostAnalyticsDto(
            TotalRevenue: totalRevenue,
            RevenueTrendPercent: ComputeTrendPercent(totalRevenue, previousRevenue),
            TotalReservations: totalReservations,
            ReservationTrendPercent: ComputeTrendPercent(totalReservations, previousReservations),
            AverageRating: averageRating,
            ReviewCount: reviewCount,
            OccupancyRate: occupancyRate,
            RevenueSeries: revenueSeries,
            TopListings: topListings,
            SourceBreakdown: sourceBreakdown,
            NoShowRate: noShowRate,
            CancellationRate: cancellationRate
        );
    }

    public async Task<BusinessSettingsDto> GetBusinessSettingsAsync(Guid hostId, CancellationToken cancellationToken = default)
    {
        var settings = await _hostBusinessSettingsRepository.GetByHostIdAsync(hostId, cancellationToken);
        return settings == null ? DefaultBusinessSettings() : MapBusinessSettings(settings);
    }

    public async Task UpdateBusinessSettingsAsync(Guid hostId, BusinessSettingsDto request, CancellationToken cancellationToken = default)
    {
        if (request.MinimumNoticeHours < 0)
        {
            throw new ArgumentException("minimumNoticeHours 0 veya büyük olmalı");
        }

        if (request.CancellationWindowHours < 0)
        {
            throw new ArgumentException("cancellationWindowHours 0 veya büyük olmalı");
        }

        var settings = await _hostBusinessSettingsRepository.GetByHostIdAsync(hostId, cancellationToken);
        var now = DateTime.UtcNow;

        if (settings == null)
        {
            settings = new HostBusinessSettings
            {
                Id = Guid.NewGuid(),
                HostId = hostId,
                CreatedAt = now
            };

            await _hostBusinessSettingsRepository.AddAsync(settings, cancellationToken);
        }

        settings.AutoConfirmReservations = request.AutoConfirmReservations;
        settings.AllowSameDayBookings = request.AllowSameDayBookings;
        settings.MinimumNoticeHours = request.MinimumNoticeHours;
        settings.CancellationWindowHours = request.CancellationWindowHours;
        settings.DynamicPricingEnabled = request.DynamicPricingEnabled;
        settings.SmartOverbookingProtection = request.SmartOverbookingProtection;
        settings.WhatsappNotifications = request.WhatsappNotifications;
        settings.EmailNotifications = request.EmailNotifications;
        settings.UpdatedAt = now;

        await _hostBusinessSettingsRepository.UpdateAsync(settings, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<List<Reservation>> GetHostReservationsAsync(Guid hostId, CancellationToken cancellationToken)
    {
        var listings = (await _listingRepository.GetByHostIdAsync(hostId, cancellationToken)).ToList();
        var listingIds = listings.Select(x => x.Id).ToList();
        return listingIds.Count == 0
            ? new List<Reservation>()
            : (await _reservationRepository.GetByListingIdsAsync(listingIds, cancellationToken)).ToList();
    }

    private HostListingDto MapListing(
        Listing listing,
        HostListingMetadata? metadata,
        IReadOnlyDictionary<Guid, List<Reservation>> reservationsByListing)
    {
        reservationsByListing.TryGetValue(listing.Id, out var reservations);
        reservations ??= [];

        var reservationCount = reservations.Count(x => x.Status != ReservationStatus.Rejected);
        var revenue = reservations.Where(x => IsRevenueStatus(x.Status)).Sum(x => x.TotalPrice);

        return new HostListingDto(
            Id: listing.Id,
            Name: GetLocalizedText(listing.Title),
            Slug: listing.Slug,
            Type: MapListingType(listing.Type),
            City: listing.City,
            ImageUrl: GetListingImage(listing),
            Status: MapListingStatus(listing.Status),
            Rating: listing.Rating,
            ReviewCount: listing.ReviewCount,
            ReservationCount: reservationCount,
            Revenue: revenue,
            BasePrice: listing.BasePricePerDay,
            Currency: listing.PriceCurrency,
            Capacity: listing.MaxGuestCount,
            SeatingAreas: (metadata?.SeatingAreas ?? [])
                .Select(x => new HostSeatingAreaDto(x.Id, x.Name, x.Capacity, x.PriceMultiplier, x.IsVip, x.MinSpend))
                .ToList(),
            Highlights: metadata?.Highlights ?? [],
            AvailabilityNotes: metadata?.AvailabilityNotes
        );
    }

    private HostReservationDto MapReservation(Reservation reservation)
    {
        var listingName = reservation.Listing == null ? string.Empty : GetLocalizedText(reservation.Listing.Title);
        var guestName = GetGuestName(reservation);
        var guestPhone = reservation.Guest?.Profile?.PhoneNumber ?? "-";

        return new HostReservationDto(
            Id: reservation.Id,
            ListingId: reservation.ListingId,
            ListingName: listingName,
            GuestName: guestName,
            GuestPhone: guestPhone,
            Date: reservation.StartTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            Time: reservation.StartTime.ToString("HH:mm", CultureInfo.InvariantCulture),
            Guests: reservation.GuestCount,
            TotalAmount: reservation.TotalPrice,
            Status: MapReservationStatus(reservation.Status),
            Source: MapReservationSource(reservation.Source),
            SpecialRequests: GetLocalizedText(reservation.SpecialRequests),
            CreatedAt: reservation.CreatedAt.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture)
        );
    }

    private async Task<User> GetOrCreateManualGuestAsync(string guestName, string guestPhone, CancellationToken cancellationToken)
    {
        var normalizedPhone = NormalizePhone(guestPhone);
        var email = $"manual+{normalizedPhone}@guest.swimago.local";

        var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
        if (user != null)
        {
            EnsureGuestProfile(user, guestName, guestPhone);
            await _userRepository.UpdateAsync(user, cancellationToken);
            return user;
        }

        user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = HashValue(Guid.NewGuid().ToString("N")),
            Role = Role.Customer,
            Status = UserStatus.Active,
            IsEmailVerified = false,
            CreatedAt = DateTime.UtcNow
        };

        EnsureGuestProfile(user, guestName, guestPhone);
        await _userRepository.AddAsync(user, cancellationToken);
        return user;
    }

    private static void EnsureGuestProfile(User user, string guestName, string guestPhone)
    {
        var parts = guestName
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();
        var firstName = parts.FirstOrDefault() ?? "Guest";
        var lastName = parts.Count > 1 ? string.Join(' ', parts.Skip(1)) : "Guest";

        if (user.Profile == null)
        {
            user.Profile = new UserProfile
            {
                Id = Guid.NewGuid(),
                UserId = user.Id
            };
        }

        user.Profile.FirstName["tr"] = firstName;
        user.Profile.LastName["tr"] = lastName;
        user.Profile.PhoneNumber = guestPhone.Trim();
    }

    private static string HashValue(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes);
    }

    private static BusinessSettingsDto DefaultBusinessSettings()
    {
        return new BusinessSettingsDto(
            AutoConfirmReservations: false,
            AllowSameDayBookings: true,
            MinimumNoticeHours: 2,
            CancellationWindowHours: 24,
            DynamicPricingEnabled: false,
            SmartOverbookingProtection: true,
            WhatsappNotifications: false,
            EmailNotifications: true
        );
    }

    private static BusinessSettingsDto MapBusinessSettings(HostBusinessSettings settings)
    {
        return new BusinessSettingsDto(
            AutoConfirmReservations: settings.AutoConfirmReservations,
            AllowSameDayBookings: settings.AllowSameDayBookings,
            MinimumNoticeHours: settings.MinimumNoticeHours,
            CancellationWindowHours: settings.CancellationWindowHours,
            DynamicPricingEnabled: settings.DynamicPricingEnabled,
            SmartOverbookingProtection: settings.SmartOverbookingProtection,
            WhatsappNotifications: settings.WhatsappNotifications,
            EmailNotifications: settings.EmailNotifications
        );
    }

    private HostListingMetadata BuildListingMetadata(Guid listingId, UpsertHostListingRequest request, DateTime now)
    {
        return new HostListingMetadata
        {
            Id = Guid.NewGuid(),
            ListingId = listingId,
            Highlights = SanitizeHighlights(request.Highlights),
            SeatingAreas = SanitizeSeatingAreas(request.SeatingAreas),
            AvailabilityNotes = NormalizeOptionalText(request.AvailabilityNotes),
            CreatedAt = now
        };
    }

    private static List<string> SanitizeHighlights(IReadOnlyCollection<string>? highlights)
    {
        return (highlights ?? [])
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static List<HostSeatingArea> SanitizeSeatingAreas(IReadOnlyCollection<HostSeatingAreaDto>? seatingAreas)
    {
        var usedIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var result = new List<HostSeatingArea>();

        foreach (var area in seatingAreas ?? [])
        {
            var id = string.IsNullOrWhiteSpace(area.Id)
                ? $"seat_{Guid.NewGuid():N}"[..13]
                : area.Id.Trim();
            if (usedIds.Contains(id))
            {
                id = $"{id}_{result.Count + 1}";
            }

            usedIds.Add(id);

            result.Add(new HostSeatingArea
            {
                Id = id,
                Name = string.IsNullOrWhiteSpace(area.Name) ? "Area" : area.Name.Trim(),
                Capacity = Math.Max(1, area.Capacity),
                PriceMultiplier = area.PriceMultiplier <= 0 ? 1 : area.PriceMultiplier,
                IsVip = area.IsVip,
                MinSpend = area.MinSpend is > 0 ? area.MinSpend : null
            });
        }

        return result;
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private async Task<string> GenerateUniqueSlugAsync(string name, CancellationToken cancellationToken)
    {
        var baseSlug = GenerateSlug(name);
        var slug = baseSlug;
        var suffix = 2;

        while (await _listingRepository.AnyAsync(x => x.Slug == slug, cancellationToken))
        {
            slug = $"{baseSlug}-{suffix}";
            suffix++;
        }

        return slug;
    }

    private static string GenerateSlug(string value)
    {
        var lower = value.ToLowerInvariant().Trim();
        var normalized = Regex.Replace(lower, @"[^a-z0-9]+", "-");
        var slug = normalized.Trim('-');
        return string.IsNullOrWhiteSpace(slug) ? "listing" : slug;
    }

    private static string NormalizeCurrency(string currency)
    {
        var normalized = (currency ?? string.Empty).Trim().ToUpperInvariant();
        return normalized switch
        {
            "USD" => "USD",
            "EUR" => "EUR",
            "TRY" => "TRY",
            "GBP" => "GBP",
            _ => throw new ArgumentException("currency USD|EUR|TRY|GBP olmalı")
        };
    }

    private static void ValidateListingRequest(UpsertHostListingRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ArgumentException("name zorunludur");
        }

        if (string.IsNullOrWhiteSpace(request.City))
        {
            throw new ArgumentException("city zorunludur");
        }

        if (request.BasePrice < 0)
        {
            throw new ArgumentException("basePrice negatif olamaz");
        }

        if (request.Capacity <= 0)
        {
            throw new ArgumentException("capacity en az 1 olmalıdır");
        }
    }

    private static string GetListingImage(Listing listing)
    {
        return listing.Images
            .OrderByDescending(x => x.IsCover)
            .ThenBy(x => x.DisplayOrder)
            .Select(x => x.Url)
            .FirstOrDefault() ?? FallbackImageUrl;
    }

    private static string GetLocalizedText(Dictionary<string, string>? values)
    {
        if (values == null || values.Count == 0)
        {
            return string.Empty;
        }

        if (values.TryGetValue("tr", out var tr))
        {
            return tr;
        }

        if (values.TryGetValue("en", out var en))
        {
            return en;
        }

        return values.Values.First();
    }

    private static string GetGuestName(Reservation reservation)
    {
        var profile = reservation.Guest?.Profile;
        if (profile == null)
        {
            return "Guest";
        }

        var first = GetLocalizedText(profile.FirstName);
        var last = GetLocalizedText(profile.LastName);
        var fullName = $"{first} {last}".Trim();
        return string.IsNullOrWhiteSpace(fullName) ? "Guest" : fullName;
    }

    private static ListingType ParseListingType(string? value)
    {
        return value?.Trim().ToLowerInvariant() switch
        {
            "beach" => ListingType.Beach,
            "pool" => ListingType.Pool,
            "yacht" => ListingType.Yacht,
            "day-trip" => ListingType.DayTrip,
            _ => throw new ArgumentException("type beach|pool|yacht|day-trip olmalı")
        };
    }

    private static string MapListingType(ListingType value)
    {
        return value switch
        {
            ListingType.Beach => "beach",
            ListingType.Pool => "pool",
            ListingType.Yacht => "yacht",
            ListingType.DayTrip => "day-trip",
            _ => "beach"
        };
    }

    private static ListingStatus ParseListingStatus(string? value)
    {
        return value?.Trim().ToLowerInvariant() switch
        {
            "active" => ListingStatus.Active,
            "pending" => ListingStatus.Pending,
            "inactive" => ListingStatus.Inactive,
            "rejected" => ListingStatus.Rejected,
            _ => throw new ArgumentException("status active|pending|inactive|rejected olmalı")
        };
    }

    private static string MapListingStatus(ListingStatus value)
    {
        return value switch
        {
            ListingStatus.Active => "active",
            ListingStatus.Pending => "pending",
            ListingStatus.Inactive => "inactive",
            ListingStatus.Rejected => "rejected",
            _ => "pending"
        };
    }

    private static ReservationStatus ParseReservationStatus(string? value)
    {
        return value?.Trim().ToLowerInvariant() switch
        {
            "pending" => ReservationStatus.Pending,
            "confirmed" => ReservationStatus.Confirmed,
            "completed" => ReservationStatus.Completed,
            "cancelled" => ReservationStatus.Cancelled,
            "rejected" => ReservationStatus.Rejected,
            _ => throw new ArgumentException("status pending|confirmed|completed|cancelled|rejected olmalı")
        };
    }

    private static string MapReservationStatus(ReservationStatus value)
    {
        return value switch
        {
            ReservationStatus.Pending => "pending",
            ReservationStatus.Confirmed => "confirmed",
            ReservationStatus.InProgress => "confirmed",
            ReservationStatus.Completed => "completed",
            ReservationStatus.Cancelled => "cancelled",
            ReservationStatus.Rejected => "rejected",
            ReservationStatus.NoShow => "cancelled",
            _ => "pending"
        };
    }

    private static ReservationSource ParseReservationSource(string? value, bool allowOnline)
    {
        var parsed = value?.Trim().ToLowerInvariant() switch
        {
            "online" => ReservationSource.Online,
            "phone" => ReservationSource.Phone,
            "walk-in" => ReservationSource.WalkIn,
            _ => throw new ArgumentException("source online|phone|walk-in olmalı")
        };

        if (!allowOnline && parsed == ReservationSource.Online)
        {
            throw new ArgumentException("manual reservation için source sadece phone|walk-in olabilir");
        }

        return parsed;
    }

    private static string MapReservationSource(ReservationSource value)
    {
        return value switch
        {
            ReservationSource.Online => "online",
            ReservationSource.Phone => "phone",
            ReservationSource.WalkIn => "walk-in",
            _ => "online"
        };
    }

    private static VenueType MapVenueType(ListingType type)
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

    private static bool IsRevenueStatus(ReservationStatus status)
    {
        return status == ReservationStatus.Confirmed || status == ReservationStatus.Completed;
    }

    private static string NormalizePhone(string value)
    {
        var digits = new string(value.Where(char.IsDigit).ToArray());
        return string.IsNullOrWhiteSpace(digits) ? Guid.NewGuid().ToString("N")[..10] : digits;
    }

    private static string GenerateConfirmationNumber()
    {
        return $"SW{DateTime.UtcNow:yyyyMMdd}{Guid.NewGuid():N}"[..16].ToUpperInvariant();
    }

    private static DateTime ParseReservationDateTime(string date, string time)
    {
        if (!DateOnly.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
        {
            throw new ArgumentException("date YYYY-MM-DD formatında olmalı");
        }

        if (!TimeOnly.TryParseExact(time, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedTime))
        {
            throw new ArgumentException("time HH:mm formatında olmalı");
        }

        return DateTime.SpecifyKind(parsedDate.ToDateTime(parsedTime), DateTimeKind.Utc);
    }

    private static DateOnly ParseDateOnly(string value)
    {
        if (!DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
        {
            throw new ArgumentException("date YYYY-MM-DD formatında olmalı");
        }

        return parsed;
    }

    private static bool TryGetPeriodRange(
        string period,
        out DateTime currentStart,
        out DateTime currentEnd,
        out DateTime previousStart,
        out DateTime previousEnd)
    {
        var now = DateTime.UtcNow;

        switch (period.Trim().ToLowerInvariant())
        {
            case "week":
            {
                var diff = ((int)now.DayOfWeek + 6) % 7;
                currentStart = now.Date.AddDays(-diff);
                currentEnd = currentStart.AddDays(7);
                previousStart = currentStart.AddDays(-7);
                previousEnd = currentStart;
                return true;
            }
            case "month":
            {
                currentStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                currentEnd = currentStart.AddMonths(1);
                previousStart = currentStart.AddMonths(-1);
                previousEnd = currentStart;
                return true;
            }
            case "year":
            {
                currentStart = new DateTime(now.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                currentEnd = currentStart.AddYears(1);
                previousStart = currentStart.AddYears(-1);
                previousEnd = currentStart;
                return true;
            }
            default:
                currentStart = default;
                currentEnd = default;
                previousStart = default;
                previousEnd = default;
                return false;
        }
    }

    private static decimal ComputeTrendPercent(decimal current, decimal previous)
    {
        if (previous == 0)
        {
            return current == 0 ? 0 : 100;
        }

        return decimal.Round(((current - previous) / previous) * 100m, 2);
    }

    private static decimal ComputeTrendPercent(int current, int previous)
    {
        if (previous == 0)
        {
            return current == 0 ? 0 : 100;
        }

        return decimal.Round(((current - previous) * 100m) / previous, 2);
    }

    private static decimal ComputeOccupancyRate(
        IReadOnlyCollection<Reservation> reservations,
        IReadOnlyCollection<Listing> listings,
        DateTime periodStart,
        DateTime periodEnd)
    {
        var dayCount = Math.Max(1, (periodEnd - periodStart).Days);
        var totalCapacityPerDay = listings.Sum(x => Math.Max(1, x.MaxGuestCount));
        var capacity = dayCount * totalCapacityPerDay;
        if (capacity <= 0)
        {
            return 0;
        }

        var guests = reservations
            .Where(x => x.Status != ReservationStatus.Cancelled && x.Status != ReservationStatus.Rejected)
            .Sum(x => x.GuestCount);

        return decimal.Round((guests * 100m) / capacity, 2);
    }

    private static IReadOnlyCollection<RevenuePointDto> BuildRevenueSeries(
        IReadOnlyCollection<Reservation> revenueReservations,
        string period,
        DateTime periodStart,
        DateTime periodEnd)
    {
        var items = new List<RevenuePointDto>();
        var key = period.Trim().ToLowerInvariant();

        if (key == "year")
        {
            for (var month = 1; month <= 12; month++)
            {
                var start = new DateTime(periodStart.Year, month, 1, 0, 0, 0, DateTimeKind.Utc);
                var end = start.AddMonths(1);
                var amount = revenueReservations
                    .Where(x => x.StartTime >= start && x.StartTime < end)
                    .Sum(x => x.TotalPrice);
                items.Add(new RevenuePointDto(start.ToString("MMM", CultureInfo.InvariantCulture), amount));
            }

            return items;
        }

        var cursor = periodStart.Date;
        while (cursor < periodEnd.Date)
        {
            var start = cursor;
            var end = cursor.AddDays(1);
            var amount = revenueReservations
                .Where(x => x.StartTime >= start && x.StartTime < end)
                .Sum(x => x.TotalPrice);
            items.Add(new RevenuePointDto(cursor.ToString("dd MMM", CultureInfo.InvariantCulture), amount));
            cursor = cursor.AddDays(1);
        }

        return items;
    }

    private static IReadOnlyCollection<TopListingMetricDto> BuildTopListings(
        IReadOnlyCollection<Reservation> revenueReservations,
        IReadOnlyCollection<Listing> listings,
        DateTime periodStart,
        DateTime periodEnd)
    {
        var listingMap = listings.ToDictionary(x => x.Id, x => x);
        var dayCount = Math.Max(1, (periodEnd - periodStart).Days);

        return revenueReservations
            .GroupBy(x => x.ListingId)
            .Select(group =>
            {
                listingMap.TryGetValue(group.Key, out var listing);
                var name = listing == null ? string.Empty : GetLocalizedText(listing.Title);
                var bookings = group.Count();
                var revenue = group.Sum(x => x.TotalPrice);
                var guests = group.Sum(x => x.GuestCount);
                var maxCapacity = listing == null ? 1 : Math.Max(1, listing.MaxGuestCount * dayCount);
                var occupancy = decimal.Round((guests * 100m) / maxCapacity, 2);

                return new TopListingMetricDto(group.Key, name, revenue, bookings, occupancy);
            })
            .OrderByDescending(x => x.Revenue)
            .Take(5)
            .ToList();
    }

    private static HostAnalyticsDto EmptyAnalytics()
    {
        return new HostAnalyticsDto(
            TotalRevenue: 0,
            RevenueTrendPercent: 0,
            TotalReservations: 0,
            ReservationTrendPercent: 0,
            AverageRating: 0,
            ReviewCount: 0,
            OccupancyRate: 0,
            RevenueSeries: [],
            TopListings: [],
            SourceBreakdown:
            [
                new SourceBreakdownDto("online", 0),
                new SourceBreakdownDto("phone", 0),
                new SourceBreakdownDto("walk-in", 0)
            ],
            NoShowRate: 0,
            CancellationRate: 0
        );
    }
}
