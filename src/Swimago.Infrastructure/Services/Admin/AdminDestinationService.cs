using Microsoft.EntityFrameworkCore;
using Swimago.Application.DTOs.Admin.Destinations;
using Swimago.Application.DTOs.Admin.Shared;
using Swimago.Application.Interfaces;
using Swimago.Domain.Entities;
using Swimago.Infrastructure.Data;

namespace Swimago.Infrastructure.Services.Admin;

public class AdminDestinationService : IAdminDestinationService
{
    private readonly ApplicationDbContext _context;

    public AdminDestinationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResponse<DestinationListItemDto>> GetDestinationsAsync(string? search, string? country, bool? isFeatured, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Destinations.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(d => d.Name.Contains(search) || d.Slug.Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(country))
        {
            query = query.Where(d => d.Country == country);
        }

        if (isFeatured.HasValue)
        {
            query = query.Where(d => d.IsFeatured == isFeatured.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(d => d.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(d => new DestinationListItemDto
            {
                Id = d.Id,
                Name = d.Name,
                Slug = d.Slug,
                Country = d.Country,
                ImageUrl = d.ImageUrl,
                SpotCount = d.SpotCount,
                AverageRating = d.AverageRating,
                IsFeatured = d.IsFeatured
            })
            .ToListAsync(cancellationToken);

        return new PaginatedResponse<DestinationListItemDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<DestinationDetailDto> GetDestinationAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var destination = await _context.Destinations.AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

        if (destination == null)
            throw new KeyNotFoundException($"Destination with ID {id} not found.");

        return MapToDetailDto(destination);
    }

    public async Task<DestinationDetailDto> CreateDestinationAsync(CreateDestinationRequest request, CancellationToken cancellationToken = default)
    {
        // Check slug uniqueness
        if (await _context.Destinations.AnyAsync(d => d.Slug == request.Slug, cancellationToken))
            throw new InvalidOperationException($"Destination with slug '{request.Slug}' already exists.");

        var destination = new Destination
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Slug = request.Slug,
            Country = request.Country,
            Description = request.Description,
            Subtitle = request.Subtitle,
            ImageUrl = request.ImageUrl,
            MapImageUrl = request.MapImageUrl,
            AvgWaterTemp = request.AvgWaterTemp,
            SunnyDaysPerYear = request.SunnyDaysPerYear,
            Tags = request.Tags,
            IsFeatured = request.IsFeatured,
            Features = request.Features.Select(f => new DestinationFeature
            {
                Icon = f.Icon,
                Title = f.Title,
                Description = f.Description
            }).ToList(),
            CreatedAt = DateTime.UtcNow
        };

        if (request.Latitude.HasValue && request.Longitude.HasValue)
        {
            destination.Latitude = request.Latitude.Value;
            destination.Longitude = request.Longitude.Value;
            destination.Location = new NetTopologySuite.Geometries.Point(request.Longitude.Value, request.Latitude.Value) { SRID = 4326 };
        }

        _context.Destinations.Add(destination);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDetailDto(destination);
    }

    public async Task<DestinationDetailDto> UpdateDestinationAsync(Guid id, CreateDestinationRequest request, CancellationToken cancellationToken = default)
    {
        var destination = await _context.Destinations.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

        if (destination == null)
            throw new KeyNotFoundException($"Destination with ID {id} not found.");

        // Check slug uniqueness if changed
        if (destination.Slug != request.Slug && await _context.Destinations.AnyAsync(d => d.Slug == request.Slug, cancellationToken))
            throw new InvalidOperationException($"Destination with slug '{request.Slug}' already exists.");

        destination.Name = request.Name;
        destination.Slug = request.Slug;
        destination.Country = request.Country;
        destination.Description = request.Description;
        destination.Subtitle = request.Subtitle;
        destination.ImageUrl = request.ImageUrl;
        destination.MapImageUrl = request.MapImageUrl;
        destination.AvgWaterTemp = request.AvgWaterTemp;
        destination.SunnyDaysPerYear = request.SunnyDaysPerYear;
        destination.Tags = request.Tags;
        destination.IsFeatured = request.IsFeatured;
        destination.Features = request.Features.Select(f => new DestinationFeature
        {
            Icon = f.Icon,
            Title = f.Title,
            Description = f.Description
        }).ToList();
        destination.UpdatedAt = DateTime.UtcNow;

        if (request.Latitude.HasValue && request.Longitude.HasValue)
        {
            destination.Latitude = request.Latitude.Value;
            destination.Longitude = request.Longitude.Value;
            destination.Location = new NetTopologySuite.Geometries.Point(request.Longitude.Value, request.Latitude.Value) { SRID = 4326 };
        }
        else
        {
            destination.Latitude = null;
            destination.Longitude = null;
            destination.Location = null;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return MapToDetailDto(destination);
    }

    public async Task DeleteDestinationAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var destination = await _context.Destinations.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
        if (destination != null)
        {
            _context.Destinations.Remove(destination);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    private static DestinationDetailDto MapToDetailDto(Destination d)
    {
        return new DestinationDetailDto
        {
            Id = d.Id,
            Name = d.Name,
            Slug = d.Slug,
            Country = d.Country,
            Description = d.Description,
            Subtitle = d.Subtitle,
            ImageUrl = d.ImageUrl,
            MapImageUrl = d.MapImageUrl,
            Latitude = d.Latitude,
            Longitude = d.Longitude,
            AvgWaterTemp = d.AvgWaterTemp,
            SunnyDaysPerYear = d.SunnyDaysPerYear,
            Tags = d.Tags,
            IsFeatured = d.IsFeatured,
            Features = d.Features.Select(f => new DestinationFeatureDto
            {
                Icon = f.Icon,
                Title = f.Title,
                Description = f.Description
            }).ToList()
        };
    }
}
