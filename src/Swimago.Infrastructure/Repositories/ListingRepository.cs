using Microsoft.EntityFrameworkCore;
using Swimago.Domain.Entities;
using Swimago.Domain.Enums;
using Swimago.Domain.Interfaces;
using Swimago.Infrastructure.Data;

namespace Swimago.Infrastructure.Repositories;

public class ListingRepository : Repository<Listing>, IListingRepository
{
    public ListingRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Listing>> GetActiveListingsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(l => l.Images)
            .Where(l => l.IsActive && l.Status == ListingStatus.Active)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Listing>> GetByHostIdAsync(Guid hostId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(l => l.Images)
            .Where(l => l.HostId == hostId)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Listing>> GetByTypeAsync(ListingType type, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(l => l.Images)
            .Where(l => l.Type == type && l.IsActive && l.Status == ListingStatus.Active)
            .ToListAsync(cancellationToken);
    }

    public async Task<Listing?> GetWithDetailsAsync(Guid listingId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(l => l.Host)
                .ThenInclude(h => h.Profile)
            .Include(l => l.Images.OrderBy(i => i.DisplayOrder))
            .Include(l => l.Amenities)
                .ThenInclude(la => la.Amenity)
            .Include(l => l.Reviews.OrderByDescending(r => r.CreatedAt).Take(10))
                .ThenInclude(r => r.Guest)
                    .ThenInclude(g => g.Profile)
            .Include(l => l.PricingCalendar.Where(p => p.Date >= DateOnly.FromDateTime(DateTime.UtcNow)))
            .FirstOrDefaultAsync(l => l.Id == listingId, cancellationToken);
    }

    public async Task<Listing?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(l => l.Host)
                .ThenInclude(h => h.Profile)
            .Include(l => l.Images.OrderBy(i => i.DisplayOrder))
            .Include(l => l.Amenities)
                .ThenInclude(la => la.Amenity)
            .Include(l => l.Reviews.OrderByDescending(r => r.CreatedAt).Take(10))
                .ThenInclude(r => r.Guest)
                    .ThenInclude(g => g.Profile)
            .FirstOrDefaultAsync(l => l.Slug == slug, cancellationToken);
    }

    public async Task<IEnumerable<Listing>> SearchNearbyAsync(decimal latitude, decimal longitude, decimal radiusKm, ListingType? type = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Include(l => l.Images)
            .Where(l => l.IsActive && l.Status == ListingStatus.Active && l.Location != null);

        if (type.HasValue)
        {
            query = query.Where(l => l.Type == type.Value);
        }

        // PostGIS distance query (ST_DWithin uses meters)
        var radiusMeters = (double)(radiusKm * 1000);
        
        return await query
            .Where(l => l.Location!.Distance(
                NetTopologySuite.Geometries.GeometryFactory.Default.CreatePoint(
                    new NetTopologySuite.Geometries.Coordinate((double)longitude, (double)latitude))) <= radiusMeters)
            .OrderByDescending(l => l.Rating)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Listing>> GetByStatusAsync(ListingStatus status, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(l => l.Host)
                .ThenInclude(h => h.Profile)
            .Include(l => l.Images)
            .Where(l => l.Status == status)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Listing>> GetFeaturedAsync(int limit = 10, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(l => l.Images)
            .Where(l => l.IsFeatured && l.IsActive && l.Status == ListingStatus.Active)
            .OrderByDescending(l => l.Rating)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }
}
