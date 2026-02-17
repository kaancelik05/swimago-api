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
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<FavoriteService> _logger;

    public FavoriteService(
        IFavoriteRepository favoriteRepository,
        IListingRepository listingRepository,
        IUnitOfWork unitOfWork,
        ILogger<FavoriteService> logger)
    {
        _favoriteRepository = favoriteRepository;
        _listingRepository = listingRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<FavoriteListResponse> GetFavoritesAsync(
        Guid userId,
        VenueType? type,
        string? search = null,
        string? sortBy = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching favorites for user {UserId}", userId);

        var favorites = (await _favoriteRepository.GetByUserIdAsync(userId, cancellationToken)).ToList();

        if (type.HasValue)
        {
            favorites = favorites.Where(f => f.VenueType == type.Value).ToList();
        }

        var favoriteItems = new List<FavoriteItemDto>(favorites.Count);

        foreach (var favorite in favorites)
        {
            var listing = favorite.Listing ?? await _listingRepository.GetByIdAsync(favorite.VenueId, cancellationToken);

            if (listing == null)
            {
                continue;
            }

            favoriteItems.Add(new FavoriteItemDto(
                Id: favorite.Id,
                VenueId: favorite.VenueId,
                VenueType: favorite.VenueType,
                VenueName: GetLocalizedText(listing.Title),
                VenueSlug: listing.Slug,
                VenueImageUrl: listing.Images.FirstOrDefault(i => i.IsCover)?.Url ?? listing.Images.FirstOrDefault()?.Url,
                VenueCity: listing.City,
                DistanceKm: null,
                VenuePrice: listing.BasePricePerDay,
                Currency: listing.PriceCurrency,
                PriceUnit: "person",
                VenueRating: listing.Rating,
                VenueReviewCount: listing.ReviewCount,
                StatusBadge: listing.IsActive ? "Open Today" : "Closed",
                AddedAt: favorite.CreatedAt
            ));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchValue = search.Trim();
            favoriteItems = favoriteItems
                .Where(x => x.VenueName.Contains(searchValue, StringComparison.OrdinalIgnoreCase)
                            || (x.VenueCity?.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ?? false))
                .ToList();
        }

        var sorted = (sortBy ?? "").Trim().ToLowerInvariant() switch
        {
            "price" => favoriteItems.OrderBy(x => x.VenuePrice).ThenByDescending(x => x.AddedAt),
            "distance" => favoriteItems.OrderBy(x => x.DistanceKm ?? decimal.MaxValue).ThenByDescending(x => x.AddedAt),
            _ => favoriteItems.OrderByDescending(x => x.VenueRating ?? 0).ThenByDescending(x => x.AddedAt)
        };

        var safePage = Math.Max(1, page);
        var safePageSize = Math.Clamp(pageSize, 1, 100);
        var orderedList = sorted.ToList();
        var totalCount = orderedList.Count;

        var pagedItems = orderedList
            .Skip((safePage - 1) * safePageSize)
            .Take(safePageSize)
            .ToList();

        return new FavoriteListResponse(pagedItems, totalCount);
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
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new FavoriteItemDto(
            Id: favorite.Id,
            VenueId: favorite.VenueId,
            VenueType: favorite.VenueType,
            VenueName: GetLocalizedText(listing.Title),
            VenueSlug: listing.Slug,
            VenueImageUrl: listing.Images.FirstOrDefault(i => i.IsCover)?.Url ?? listing.Images.FirstOrDefault()?.Url,
            VenueCity: listing.City,
            DistanceKm: null,
            VenuePrice: listing.BasePricePerDay,
            Currency: listing.PriceCurrency,
            PriceUnit: "person",
            VenueRating: listing.Rating,
            VenueReviewCount: listing.ReviewCount,
            StatusBadge: listing.IsActive ? "Open Today" : "Closed",
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
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static string GetLocalizedText(Dictionary<string, string>? values)
    {
        if (values == null || values.Count == 0)
        {
            return string.Empty;
        }

        if (values.TryGetValue("tr", out var tr) && !string.IsNullOrWhiteSpace(tr))
        {
            return tr;
        }

        if (values.TryGetValue("en", out var en) && !string.IsNullOrWhiteSpace(en))
        {
            return en;
        }

        return values.Values.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)) ?? string.Empty;
    }
}
