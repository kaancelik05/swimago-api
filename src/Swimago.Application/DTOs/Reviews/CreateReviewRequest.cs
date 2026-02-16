namespace Swimago.Application.DTOs.Reviews;

public record CreateReviewRequest(
    Guid ListingId,
    int Rating,
    string Comment
);
