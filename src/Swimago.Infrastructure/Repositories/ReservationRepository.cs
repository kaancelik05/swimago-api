using Microsoft.EntityFrameworkCore;
using Swimago.Domain.Entities;
using Swimago.Domain.Enums;
using Swimago.Domain.Interfaces;
using Swimago.Infrastructure.Data;

namespace Swimago.Infrastructure.Repositories;

public class ReservationRepository : Repository<Reservation>, IReservationRepository
{
    public ReservationRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Reservation>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(r => r.Listing)
                .ThenInclude(l => l.Images.OrderBy(i => i.DisplayOrder).Take(1))
            .Include(r => r.Payment)
            .Where(r => r.GuestId == userId)
            .OrderByDescending(r => r.StartTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Reservation>> GetByGuestIdAsync(Guid guestId, CancellationToken cancellationToken = default)
    {
        return await GetByUserIdAsync(guestId, cancellationToken);
    }

    public async Task<IEnumerable<Reservation>> GetByGuestIdAndStatusAsync(Guid guestId, ReservationStatus? status = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Include(r => r.Listing)
                .ThenInclude(l => l.Images.OrderBy(i => i.DisplayOrder).Take(1))
            .Include(r => r.Payment)
            .Where(r => r.GuestId == guestId);

        if (status.HasValue)
        {
            query = query.Where(r => r.Status == status.Value);
        }

        return await query
            .OrderByDescending(r => r.StartTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Reservation>> GetByVenueIdAsync(Guid venueId, CancellationToken cancellationToken = default)
    {
        return await _context.Reservations
            .Include(r => r.Guest)
            .Include(r => r.Listing)
            .Where(r => r.ListingId == venueId) // Assuming VenueId map to ListingId for now as per prior context
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Reservation>> GetByListingIdsAsync(IEnumerable<Guid> listingIds, CancellationToken cancellationToken = default)
    {
        return await _context.Reservations
            .Include(r => r.Guest)
            .ThenInclude(u => u.Profile)
            .Include(r => r.Listing)
            .Where(r => listingIds.Contains(r.ListingId))
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Reservation>> GetByListingIdAsync(Guid listingId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(r => r.Guest)
                .ThenInclude(g => g.Profile)
            .Where(r => r.ListingId == listingId)
            .OrderByDescending(r => r.StartTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Reservation>> GetByStatusAsync(ReservationStatus status, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(r => r.Listing)
            .Include(r => r.Guest)
            .Where(r => r.Status == status)
            .OrderByDescending(r => r.StartTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<Reservation?> GetWithDetailsAsync(Guid reservationId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(r => r.Listing)
                .ThenInclude(l => l.Images.OrderBy(i => i.DisplayOrder))
            .Include(r => r.Listing)
                .ThenInclude(l => l.Host)
                    .ThenInclude(h => h.Profile)
            .Include(r => r.Guest)
                .ThenInclude(g => g.Profile)
            .Include(r => r.Payment)
            .Include(r => r.Review)
            .FirstOrDefaultAsync(r => r.Id == reservationId, cancellationToken);
    }

    public async Task<bool> HasOverlappingReservationsAsync(Guid listingId, DateTime startTime, DateTime endTime, Guid? excludeReservationId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Where(r => r.ListingId == listingId)
            .Where(r => r.Status != ReservationStatus.Cancelled)
            .Where(r => r.StartTime < endTime && r.EndTime > startTime);

        if (excludeReservationId.HasValue)
        {
            query = query.Where(r => r.Id != excludeReservationId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<IDictionary<ReservationStatus, int>> GetStatusCountsAsync(Guid guestId, CancellationToken cancellationToken = default)
    {
        var counts = await _dbSet
            .Where(r => r.GuestId == guestId)
            .GroupBy(r => r.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return counts.ToDictionary(x => x.Status, x => x.Count);
    }
}
