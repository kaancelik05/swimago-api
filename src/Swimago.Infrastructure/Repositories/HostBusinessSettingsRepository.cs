using Microsoft.EntityFrameworkCore;
using Swimago.Domain.Entities;
using Swimago.Domain.Interfaces;
using Swimago.Infrastructure.Data;

namespace Swimago.Infrastructure.Repositories;

public class HostBusinessSettingsRepository : Repository<HostBusinessSettings>, IHostBusinessSettingsRepository
{
    public HostBusinessSettingsRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<HostBusinessSettings?> GetByHostIdAsync(Guid hostId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(x => x.HostId == hostId, cancellationToken);
    }
}
