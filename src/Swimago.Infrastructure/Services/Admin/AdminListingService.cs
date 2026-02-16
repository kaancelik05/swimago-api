using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Swimago.Application.DTOs.Admin.Listings;
using Swimago.Application.DTOs.Admin.Shared;
using Swimago.Application.Interfaces;
using Swimago.Domain.Entities;
using Swimago.Domain.Enums;
using Swimago.Infrastructure.Data;

namespace Swimago.Infrastructure.Services.Admin;

public class AdminListingService : IAdminListingService
{
    private readonly ApplicationDbContext _context;
    private readonly JsonSerializerOptions _jsonOptions;

    public AdminListingService(ApplicationDbContext context)
    {
        _context = context;
        _jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    }

    // --- COMMON LISTING OPERATIONS ---

    public async Task<PaginatedResponse<ListingListItemDto>> GetListingsAsync(ListingType type, string? search, string? city, bool? isActive, decimal? minPrice, decimal? maxPrice, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Listings.AsNoTracking().Where(l => l.Type == type);

        if (!string.IsNullOrWhiteSpace(search))
        {
            // Note: Searching inside JSONB titles is more complex in EF Core translation, 
            // for simplicity we assume slug or city search, or raw SQL if needed.
            // Here assuming simple filtering or searching by slug/city/host name if applicable.
            query = query.Where(l => l.Slug.Contains(search) || l.City.Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(city))
            query = query.Where(l => l.City == city);

        if (isActive.HasValue)
            query = query.Where(l => l.IsActive == isActive.Value);

        if (minPrice.HasValue)
            query = query.Where(l => l.BasePricePerDay >= minPrice.Value);

        if (maxPrice.HasValue)
            query = query.Where(l => l.BasePricePerDay <= maxPrice.Value);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new ListingListItemDto
            {
                Id = l.Id,
                Slug = l.Slug,
                City = l.City,
                ImageUrl = l.Images.Where(i => i.IsCover).Select(i => i.Url).FirstOrDefault() ?? string.Empty,
                Price = l.BasePricePerDay,
                Currency = l.PriceCurrency,
                Rating = (double)l.Rating,
                ReviewCount = l.ReviewCount,
                IsActive = l.IsActive,
                IsFeatured = l.IsFeatured,
                Type = l.Type,
                // Name needs to be handled post-query or via simpler property if available
                Name = l.Slug // Fallback
            })
            .ToListAsync(cancellationToken);

        // Fetch Names (MultiLanguage) separately if needed or refine query to extract active language
        // For MVP, we pass Slug or first available title from JSONB if we loaded it.
        // Optimization: Use separate DTO projection that includes Title Dictionary and map in memory.

        return new PaginatedResponse<ListingListItemDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task DeleteListingAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var listing = await _context.Listings.FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
        if (listing != null)
        {
            _context.Listings.Remove(listing);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    // --- HELPER METHODS ---
    
    private void UpdateCommonListingProperties(Listing listing, dynamic request)
    {
        listing.Slug = request.Slug;
        listing.City = request.City;
        listing.Country = request.Country;
        listing.Latitude = (decimal)request.Latitude;
        listing.Longitude = (decimal)request.Longitude;
        listing.Location = new NetTopologySuite.Geometries.Point((double)request.Longitude, (double)request.Latitude) { SRID = 4326 };
        listing.BasePricePerDay = request.PricePerDay;
        listing.PriceCurrency = request.Currency;
        listing.IsActive = request.IsActive;
        listing.IsFeatured = request.IsFeatured;
        
        // Multi-Language Mappings
        listing.Title = new Dictionary<string, string>
        {
            { "tr", request.Name.Tr ?? "" },
            { "en", request.Name.En ?? "" },
            { "de", request.Name.De ?? "" },
            { "ru", request.Name.Ru ?? "" }
        };

        listing.Description = new Dictionary<string, string>
        {
            { "tr", request.Description.Tr ?? "" },
            { "en", request.Description.En ?? "" },
            { "de", request.Description.De ?? "" },
            { "ru", request.Description.Ru ?? "" }
        };

        // Simplified images update - replace all
        // In production, handle differential updates
        listing.Images = ((List<ImageDto>)request.Images).Select(img => new ListingImage
        {
            Id = Guid.NewGuid(),
            Url = img.Url,
            IsCover = img.IsPrimary,
            DisplayOrder = img.Order
        }).ToList();
    }

    // --- BEACH IMPLEMENTATION ---

    public async Task<BeachDetailDto> GetBeachAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var listing = await _context.Listings
            .Include(l => l.Images)
            .Include(l => l.Amenities).ThenInclude(la => la.Amenity)
            .FirstOrDefaultAsync(l => l.Id == id && l.Type == ListingType.Beach, cancellationToken);

        if (listing == null) throw new KeyNotFoundException($"Beach with ID {id} not found.");

        return MapToBeachDto(listing);
    }

    public async Task<BeachDetailDto> CreateBeachAsync(CreateBeachRequest request, CancellationToken cancellationToken = default)
    {
        var listing = new Listing
        {
            Id = Guid.NewGuid(),
            Type = ListingType.Beach,
            CreatedAt = DateTime.UtcNow
        };

        UpdateCommonListingProperties(listing, request);

        // Specifics
        var details = new
        {
            request.Conditions,
            request.LocationSubtitle,
            request.MapImageUrl,
            request.PriceUnit,
            request.RareFindMessage,
            request.Breadcrumbs
        };
        listing.Details = JsonSerializer.Serialize(details, _jsonOptions);

        _context.Listings.Add(listing);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToBeachDto(listing);
    }

    public async Task<BeachDetailDto> UpdateBeachAsync(Guid id, CreateBeachRequest request, CancellationToken cancellationToken = default)
    {
        var listing = await _context.Listings
            .Include(l => l.Images)
            .FirstOrDefaultAsync(l => l.Id == id && l.Type == ListingType.Beach, cancellationToken);

        if (listing == null) throw new KeyNotFoundException($"Beach with ID {id} not found.");

        UpdateCommonListingProperties(listing, request);

        var details = new
        {
            request.Conditions,
            request.LocationSubtitle,
            request.MapImageUrl,
            request.PriceUnit,
            request.RareFindMessage,
            request.Breadcrumbs
        };
        listing.Details = JsonSerializer.Serialize(details, _jsonOptions);
        listing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return MapToBeachDto(listing);
    }

    private BeachDetailDto MapToBeachDto(Listing l)
    {
        // Deserialize details
        dynamic? details = string.IsNullOrEmpty(l.Details) ? null : JsonSerializer.Deserialize<dynamic>(l.Details, _jsonOptions);
        
        // This is a simplified mapping. In a real app, use AutoMapper or robust manual mapping.
        // Complex JSON properties need careful handling.
        
        return new BeachDetailDto
        {
            Id = l.Id,
            Slug = l.Slug,
            City = l.City,
            Country = l.Country,
            Latitude = (double)l.Latitude,
            Longitude = (double)l.Longitude,
            PricePerDay = l.BasePricePerDay,
            Currency = l.PriceCurrency,
            IsActive = l.IsActive,
            IsFeatured = l.IsFeatured,
            Images = l.Images.Select(i => new ImageDto { Url = i.Url, IsPrimary = i.IsCover }).ToList(),
            Name = MapDictToMultiLang(l.Title),
            Description = MapDictToMultiLang(l.Description),
            // Map other properties from Details JSON if implementing fully
        };
    }

    // --- HELPER FOR LANG ---
    private static MultiLanguageDto MapDictToMultiLang(Dictionary<string, string> dict)
    {
        return new MultiLanguageDto
        {
            Tr = dict.GetValueOrDefault("tr"),
            En = dict.GetValueOrDefault("en"),
            De = dict.GetValueOrDefault("de"),
            Ru = dict.GetValueOrDefault("ru")
        };
    }
    
    // --- STUBS FOR OTHER TYPES (IMPLEMENT SIMILARLY) ---

    public Task<PoolDetailDto> GetPoolAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task<PoolDetailDto> CreatePoolAsync(CreatePoolRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task<PoolDetailDto> UpdatePoolAsync(Guid id, CreatePoolRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();

    public Task<YachtTourDetailDto> GetYachtTourAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task<YachtTourDetailDto> CreateYachtTourAsync(CreateYachtTourRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task<YachtTourDetailDto> UpdateYachtTourAsync(Guid id, CreateYachtTourRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();

    public Task<DayTripDetailDto> GetDayTripAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task<DayTripDetailDto> CreateDayTripAsync(CreateDayTripRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task<DayTripDetailDto> UpdateDayTripAsync(Guid id, CreateDayTripRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
}
