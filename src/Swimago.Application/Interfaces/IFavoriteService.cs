using Swimago.Application.DTOs.Favorites;

namespace Swimago.Application.Interfaces;

public interface IFavoriteService
{
    Task<FavoriteListResponse> GetFavoritesAsync(Guid userId, Domain.Enums.VenueType? type, CancellationToken cancellationToken = default);
    Task<FavoriteItemDto> AddFavoriteAsync(Guid userId, AddFavoriteRequest request, CancellationToken cancellationToken = default);
    Task RemoveFavoriteAsync(Guid userId, Guid venueId, CancellationToken cancellationToken = default);
}
