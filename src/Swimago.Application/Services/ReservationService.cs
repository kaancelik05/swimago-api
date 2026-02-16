using AutoMapper;
using Swimago.Application.DTOs.Reservations;
using Swimago.Application.Interfaces;
using Swimago.Domain.Entities;
using Swimago.Domain.Enums;
using Swimago.Domain.Interfaces;

namespace Swimago.Application.Services;

public class ReservationService : IReservationService
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IListingRepository _listingRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPricingService _pricingService;
    private readonly IMapper _mapper;

    public ReservationService(
        IReservationRepository reservationRepository,
        IListingRepository listingRepository,
        IUnitOfWork unitOfWork,
        IPricingService pricingService,
        IMapper mapper)
    {
        _reservationRepository = reservationRepository;
        _listingRepository = listingRepository;
        _unitOfWork = unitOfWork;
        _pricingService = pricingService;
        _mapper = mapper;
    }

    public async Task<ReservationResponse> CreateReservationAsync(Guid guestId, CreateReservationRequest request, CancellationToken cancellationToken = default)
    {
        var listing = await _listingRepository.GetByIdAsync(request.ListingId, cancellationToken);
        if (listing == null)
            throw new KeyNotFoundException("Listing not found");

        if (!listing.IsActive || listing.Status != ListingStatus.Active)
            throw new InvalidOperationException("This listing is currently not accepting reservations.");

        if (request.GuestCount > listing.MaxGuestCount)
            throw new InvalidOperationException($"Maximum guest count for this listing is {listing.MaxGuestCount}.");

        // Availability check
        var isOverlap = await _reservationRepository.HasOverlappingReservationsAsync(
            request.ListingId, 
            request.StartTime, 
            request.EndTime, 
            null, 
            cancellationToken);

        if (isOverlap)
            throw new InvalidOperationException("The selected dates/times are already booked.");

        // Calculate price
        var totalPrice = await _pricingService.CalculateTotalPriceAsync(
            listing, 
            request.StartTime, 
            request.EndTime, 
            request.GuestCount, 
            cancellationToken);

        // Determine VenueType from ListingType
        var venueType = listing.Type switch
        {
            ListingType.Beach => VenueType.Beach,
            ListingType.Pool => VenueType.Pool,
            ListingType.Yacht => VenueType.Yacht,
            ListingType.DayTrip => VenueType.DayTrip,
            _ => VenueType.Beach
        };

        // Create reservation
        var reservation = new Reservation
        {
            Id = Guid.NewGuid(),
            ListingId = request.ListingId,
            GuestId = guestId,
            VenueType = venueType,
            BookingType = request.BookingType,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            GuestCount = request.GuestCount,
            TotalPrice = totalPrice,
            FinalPrice = totalPrice, // Discounts could be applied here
            Currency = "USD",
            Status = ReservationStatus.Pending,
            ConfirmationNumber = GenerateConfirmationNumber(),
            CreatedAt = DateTime.UtcNow,
            SpecialRequests = !string.IsNullOrWhiteSpace(request.SpecialRequests) 
                ? new Dictionary<string, string> { { "tr", request.SpecialRequests } } 
                : null
        };

        // Create initial pending payment
        reservation.Payment = new ReservationPayment
        {
            Id = Guid.NewGuid(),
            Amount = totalPrice,
            Currency = "USD",
            Status = PaymentStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await _reservationRepository.AddAsync(reservation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ReservationResponse>(reservation);
    }

    public async Task<ReservationResponse?> GetReservationByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var reservation = await _reservationRepository.GetWithDetailsAsync(id, cancellationToken);
        return _mapper.Map<ReservationResponse>(reservation);
    }

    public async Task<IEnumerable<ReservationResponse>> GetUserReservationsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var reservations = await _reservationRepository.GetByGuestIdAsync(userId, cancellationToken);
        return _mapper.Map<IEnumerable<ReservationResponse>>(reservations);
    }

    public async Task<bool> CancelReservationAsync(Guid reservationId, Guid userId, CancellationToken cancellationToken = default)
    {
        var reservation = await _reservationRepository.GetByIdAsync(reservationId, cancellationToken);
        if (reservation == null)
            throw new KeyNotFoundException("Reservation not found");

        if (reservation.GuestId != userId)
            throw new UnauthorizedAccessException("You are not authorized to cancel this reservation.");

        if (reservation.Status == ReservationStatus.Cancelled || reservation.Status == ReservationStatus.Completed)
            throw new InvalidOperationException($"Reservation cannot be cancelled because it is already {reservation.Status}.");

        reservation.Status = ReservationStatus.Cancelled;
        reservation.CancelledAt = DateTime.UtcNow;
        await _reservationRepository.UpdateAsync(reservation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> CheckAvailabilityAsync(Guid listingId, DateTime startTime, DateTime endTime, CancellationToken cancellationToken = default)
    {
        return !await _reservationRepository.HasOverlappingReservationsAsync(listingId, startTime, endTime, null, cancellationToken);
    }

    private static string GenerateConfirmationNumber()
    {
        return $"SW{DateTime.UtcNow:yyyyMMdd}{Guid.NewGuid().ToString("N")[..6].ToUpper()}";
    }
}
