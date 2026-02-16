using Swimago.Domain.Entities;

namespace Swimago.Domain.Interfaces;

public interface IAmenityRepository : IRepository<Amenity>
{
    Task<IEnumerable<Amenity>> GetActiveAsync(CancellationToken cancellationToken = default);
}
