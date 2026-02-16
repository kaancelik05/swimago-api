using Microsoft.EntityFrameworkCore;
using Swimago.Domain.Entities;
using Swimago.Domain.Interfaces;
using Swimago.Infrastructure.Data;

namespace Swimago.Infrastructure.Repositories;

public class FavoriteRepository : Repository<Favorite>, IFavoriteRepository
{
    public FavoriteRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Favorite>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(f => f.Listing)
                .ThenInclude(l => l!.Images.OrderBy(i => i.DisplayOrder).Take(1))
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Favorite?> GetByUserAndVenueAsync(Guid userId, Guid venueId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(f => f.UserId == userId && f.VenueId == venueId, cancellationToken);
    }

    public async Task<bool> IsFavoriteAsync(Guid userId, Guid venueId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(f => f.UserId == userId && f.VenueId == venueId, cancellationToken);
    }
}
