using Microsoft.EntityFrameworkCore;
using Swimago.Domain.Entities;
using Swimago.Domain.Interfaces;
using Swimago.Infrastructure.Data;

namespace Swimago.Infrastructure.Repositories;

public class CityRepository : Repository<City>, ICityRepository
{
    public CityRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<City>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<City>> GetByCountryAsync(string country, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.Country == country && c.IsActive)
            .ToListAsync(cancellationToken);
    }
}
