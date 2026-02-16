using Swimago.Domain.Entities;

namespace Swimago.Application.Interfaces;

public interface IPricingService
{
    Task<decimal> CalculateTotalPriceAsync(Listing listing, DateTime startTime, DateTime endTime, int guestCount, CancellationToken cancellationToken = default);
}
