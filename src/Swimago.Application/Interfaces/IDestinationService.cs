using Swimago.Application.DTOs.Destinations;

namespace Swimago.Application.Interfaces;

public interface IDestinationService
{
    Task<DestinationListResponse> GetAllDestinationsAsync(
        bool? featured,
        string? type = null,
        string? search = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);
    Task<DestinationDetailResponse> GetDestinationBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<DestinationPageDetailResponse> GetDestinationPageBySlugAsync(string slug, CancellationToken cancellationToken = default);
}
