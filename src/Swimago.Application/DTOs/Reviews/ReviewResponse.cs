using Swimago.Application.DTOs.Common;

namespace Swimago.Application.DTOs.Reviews;

public record ReviewResponse(
    int Id,
    int ListingId,
    int GuestId,
    string GuestName,
    int Rating,
    MultiLanguageDto Comment,
    DateTime CreatedAt,
    MultiLanguageDto? HostResponse = null,
    DateTime? HostResponseAt = null
);
