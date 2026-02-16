using AutoMapper;
using Swimago.Application.DTOs.Reviews;
using Swimago.Application.Interfaces;
using Swimago.Domain.Entities;
using Swimago.Domain.Enums;
using Swimago.Domain.Interfaces;

namespace Swimago.Application.Services;

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IReservationRepository _reservationRepository;
    private readonly IListingRepository _listingRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ReviewService(
        IReviewRepository reviewRepository,
        IReservationRepository reservationRepository,
        IListingRepository listingRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _reviewRepository = reviewRepository;
        _reservationRepository = reservationRepository;
        _listingRepository = listingRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ReviewResponse> CreateReviewAsync(Guid guestId, CreateReviewRequest request, CancellationToken cancellationToken = default)
    {
        // Verify listing exists
        var listing = await _listingRepository.GetByIdAsync(request.ListingId, cancellationToken);
        if (listing == null)
            throw new KeyNotFoundException("Listing bulunamadı");

        // Find completed reservation for this user and listing
        var reservation = await _reservationRepository
            .FirstOrDefaultAsync(r => 
                r.GuestId == guestId && 
                r.ListingId == request.ListingId && 
                r.Status == ReservationStatus.Completed,
            cancellationToken);

        if (reservation == null)
            throw new InvalidOperationException("Bu liste için tamamlanmış rezervasyonunuz bulunmamaktadır. Sadece kaldığınız yerler için yorum yapabilirsiniz.");

        // Check if already reviewed
        var existingReview = await _reviewRepository.HasUserReviewedReservationAsync(reservation.Id, cancellationToken);
        if (existingReview)
            throw new InvalidOperationException("Bu rezervasyon için zaten yorum yapmışsınız.");

        // Create review
        var review = new Review
        {
            Id = Guid.NewGuid(),
            ReservationId = reservation.Id,
            ListingId = request.ListingId,
            GuestId = guestId,
            Rating = request.Rating,
            Text = request.Comment,
            CreatedAt = DateTime.UtcNow,
            IsVerified = true // Verified because they had a reservation
        };

        await _reviewRepository.AddAsync(review, cancellationToken);

        // Update listing rating
        await UpdateListingRatingAsync(request.ListingId, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetReviewByIdAsync(review.Id, cancellationToken) 
            ?? throw new InvalidOperationException("Yorum oluşturulduktan sonra alınamadı");
    }

    public async Task<ReviewResponse?> GetReviewByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var review = await _reviewRepository.GetByIdAsync(id, cancellationToken);
        return review == null ? null : _mapper.Map<ReviewResponse>(review);
    }

    public async Task<IEnumerable<ReviewResponse>> GetListingReviewsAsync(Guid listingId, CancellationToken cancellationToken = default)
    {
        var reviews = await _reviewRepository.GetByListingIdAsync(listingId, cancellationToken);
        return _mapper.Map<IEnumerable<ReviewResponse>>(reviews);
    }

    public async Task<ReviewResponse> AddHostResponseAsync(Guid reviewId, Guid hostId, AddHostResponseRequest request, CancellationToken cancellationToken = default)
    {
        var review = await _reviewRepository.GetByIdAsync(reviewId, cancellationToken);
        if (review == null)
            throw new KeyNotFoundException("Yorum bulunamadı");

        var listing = await _listingRepository.GetByIdAsync(review.ListingId, cancellationToken);
        if (listing == null || listing.HostId != hostId)
            throw new UnauthorizedAccessException("Bu yoruma yanıt verme yetkiniz yok");

        if (review.HostResponseText != null)
            throw new InvalidOperationException("Bu yoruma zaten yanıt verilmiş");

        review.HostResponseText = request.Response;
        review.HostResponseDate = DateTime.UtcNow;

        await _reviewRepository.UpdateAsync(review, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetReviewByIdAsync(reviewId, cancellationToken)
            ?? throw new InvalidOperationException("Yanıt ekledikten sonra yorum alınamadı");
    }

    public async Task<bool> DeleteReviewAsync(Guid reviewId, Guid userId, CancellationToken cancellationToken = default)
    {
        var review = await _reviewRepository.GetByIdAsync(reviewId, cancellationToken);
        if (review == null)
            throw new KeyNotFoundException("Yorum bulunamadı");

        if (review.GuestId != userId)
            throw new UnauthorizedAccessException("Bu yorumu silme yetkiniz yok");

        await _reviewRepository.DeleteAsync(review, cancellationToken);
        
        // Update listing rating after deletion
        await UpdateListingRatingAsync(review.ListingId, cancellationToken);
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    private async Task UpdateListingRatingAsync(Guid listingId, CancellationToken cancellationToken)
    {
        var listing = await _listingRepository.GetByIdAsync(listingId, cancellationToken);
        if (listing == null) return;

        var reviews = await _reviewRepository.GetByListingIdAsync(listingId, cancellationToken);
        var reviewList = reviews.ToList();

        if (reviewList.Any())
        {
            listing.Rating = (decimal)reviewList.Average(r => r.Rating);
            listing.ReviewCount = reviewList.Count;
        }
        else
        {
            listing.Rating = 0;
            listing.ReviewCount = 0;
        }

        await _listingRepository.UpdateAsync(listing, cancellationToken);
    }
}
