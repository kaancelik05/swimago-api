using Swimago.Domain.Entities;

namespace Swimago.Domain.Interfaces;

public interface IHostBusinessSettingsRepository : IRepository<HostBusinessSettings>
{
    Task<HostBusinessSettings?> GetByHostIdAsync(Guid hostId, CancellationToken cancellationToken = default);
}
