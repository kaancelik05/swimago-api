using Swimago.Domain.Entities;

namespace Swimago.Domain.Interfaces;

public interface ICityRepository : IRepository<City>
{
    Task<IEnumerable<City>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<City>> GetByCountryAsync(string country, CancellationToken cancellationToken = default);
}
