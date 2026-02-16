using Swimago.Application.DTOs.BoatTours;

namespace Swimago.Application.Interfaces;

public interface IBoatTourService
{
    Task<BoatTourListResponse> GetAllBoatToursAsync(string? city, string? type, decimal? minPrice, decimal? maxPrice, CancellationToken cancellationToken = default);
    Task<YachtTourDetailResponse> GetYachtTourBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<DayTripDetailResponse> GetDayTripBySlugAsync(string slug, CancellationToken cancellationToken = default);
}
