using Swimago.Application.DTOs.Reviews;

namespace Swimago.Application.Interfaces;

public interface IReviewService
{
    Task<ReviewResponse> CreateReviewAsync(Guid guestId, CreateReviewRequest request, CancellationToken cancellationToken = default);
    Task<ReviewResponse?> GetReviewByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<ReviewResponse>> GetListingReviewsAsync(Guid listingId, CancellationToken cancellationToken = default);
    Task<ReviewResponse> AddHostResponseAsync(Guid reviewId, Guid hostId, AddHostResponseRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteReviewAsync(Guid reviewId, Guid userId, CancellationToken cancellationToken = default);
}
