using Microsoft.EntityFrameworkCore;
using Swimago.Domain.Entities;
using Swimago.Domain.Interfaces;
using Swimago.Infrastructure.Data;

namespace Swimago.Infrastructure.Repositories;

public class ReviewRepository : Repository<Review>, IReviewRepository
{
    public ReviewRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Review>> GetByListingIdAsync(Guid listingId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(r => r.Guest)
                .ThenInclude(g => g.Profile)
            .Where(r => r.ListingId == listingId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Review?> GetByReservationIdAsync(Guid reservationId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(r => r.Guest)
                .ThenInclude(g => g.Profile)
            .FirstOrDefaultAsync(r => r.ReservationId == reservationId, cancellationToken);
    }

    public async Task<bool> HasUserReviewedReservationAsync(Guid reservationId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(r => r.ReservationId == reservationId, cancellationToken);
    }
}
