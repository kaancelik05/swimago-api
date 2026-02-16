using Swimago.Application.DTOs.Destinations;

namespace Swimago.Application.Interfaces;

public interface IDestinationService
{
    Task<DestinationListResponse> GetAllDestinationsAsync(bool? featured, CancellationToken cancellationToken = default);
    Task<DestinationDetailResponse> GetDestinationBySlugAsync(string slug, CancellationToken cancellationToken = default);
}
