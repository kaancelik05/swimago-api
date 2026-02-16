using Microsoft.EntityFrameworkCore;
using Swimago.Domain.Entities;
using Swimago.Domain.Interfaces;
using Swimago.Infrastructure.Data;

namespace Swimago.Infrastructure.Repositories;

public class AmenityRepository : Repository<Amenity>, IAmenityRepository
{
    public AmenityRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Amenity>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(a => a.IsActive)
            .ToListAsync(cancellationToken);
    }
}
