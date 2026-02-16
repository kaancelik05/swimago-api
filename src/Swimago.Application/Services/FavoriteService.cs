using Microsoft.Extensions.Logging;
using Swimago.Application.DTOs.Favorites;
using Swimago.Application.Interfaces;
using Swimago.Domain.Entities;
using Swimago.Domain.Enums;
using Swimago.Domain.Interfaces;

namespace Swimago.Application.Services;

public class FavoriteService : IFavoriteService
{
    private readonly IFavoriteRepository _favoriteRepository;
    private readonly IListingRepository _listingRepository;
    private readonly ILogger<FavoriteService> _logger;

    public FavoriteService(
        IFavoriteRepository favoriteRepository,
        IListingRepository listingRepository,
        ILogger<FavoriteService> logger)
    {
        _favoriteRepository = favoriteRepository;
        _listingRepository = listingRepository;
        _logger = logger;
    }

    public async Task<FavoriteListResponse> GetFavoritesAsync(Guid userId, VenueType? type, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching favorites for user {UserId}", userId);

        var favorites = await _favoriteRepository.GetByUserIdAsync(userId, cancellationToken);

        if (type.HasValue)
        {
            favorites = favorites.Where(f => f.VenueType == type.Value);
        }

        var favoriteItems = new List<FavoriteItemDto>();

        foreach (var favorite in favorites)
        {
            var listing = await _listingRepository.GetByIdAsync(favorite.VenueId, cancellationToken);
            
            favoriteItems.Add(new FavoriteItemDto(
                Id: favorite.Id,
                VenueId: favorite.VenueId,
                VenueType: favorite.VenueType,
                VenueName: listing?.Title.GetValueOrDefault("tr") ?? "Bilinmeyen mekan",
                VenueSlug: listing?.Slug,
                VenueImageUrl: listing?.Images.FirstOrDefault(i => i.IsCover)?.Url ?? listing?.Images.FirstOrDefault()?.Url,
                VenueCity: listing?.City,
                VenuePrice: listing?.BasePricePerDay,
                Currency: listing?.PriceCurrency ?? "TRY",
                VenueRating: listing?.Rating,
                VenueReviewCount: listing?.ReviewCount,
                AddedAt: favorite.CreatedAt
            ));
        }

        return new FavoriteListResponse(favoriteItems, favoriteItems.Count);
    }

    public async Task<FavoriteItemDto> AddFavoriteAsync(Guid userId, AddFavoriteRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Adding favorite for user {UserId}: venue {VenueId}", userId, request.VenueId);

        var listing = await _listingRepository.GetByIdAsync(request.VenueId, cancellationToken);
        if (listing == null)
            throw new KeyNotFoundException("Mekan bulunamadı");

        var existingFavorite = await _favoriteRepository.GetByUserAndVenueAsync(userId, request.VenueId, cancellationToken);
        if (existingFavorite != null)
            throw new InvalidOperationException("Bu mekan zaten favorilerinizde");

        var favorite = new Favorite
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            VenueId = request.VenueId,
            VenueType = request.VenueType,
            CreatedAt = DateTime.UtcNow
        };

        await _favoriteRepository.AddAsync(favorite, cancellationToken);

        return new FavoriteItemDto(
            Id: favorite.Id,
            VenueId: favorite.VenueId,
            VenueType: favorite.VenueType,
            VenueName: listing.Title.GetValueOrDefault("tr") ?? "",
            VenueSlug: listing.Slug,
            VenueImageUrl: listing.Images.FirstOrDefault(i => i.IsCover)?.Url ?? listing.Images.FirstOrDefault()?.Url,
            VenueCity: listing.City,
            VenuePrice: listing.BasePricePerDay,
            Currency: listing.PriceCurrency,
            VenueRating: listing.Rating,
            VenueReviewCount: listing.ReviewCount,
            AddedAt: favorite.CreatedAt
        );
    }

    public async Task RemoveFavoriteAsync(Guid userId, Guid venueId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Removing favorite for user {UserId}: venue {VenueId}", userId, venueId);

        var favorite = await _favoriteRepository.GetByUserAndVenueAsync(userId, venueId, cancellationToken);
        if (favorite == null)
            throw new KeyNotFoundException("Favori bulunamadı");

        await _favoriteRepository.DeleteAsync(favorite, cancellationToken);
    }
}
