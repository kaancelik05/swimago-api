using Swimago.Application.Interfaces;
using Swimago.Domain.Entities;
using Swimago.Domain.Enums;

namespace Swimago.Application.Services;

public class PricingService : IPricingService
{
    public Task<decimal> CalculateTotalPriceAsync(Listing listing, DateTime startTime, DateTime endTime, int guestCount, CancellationToken cancellationToken = default)
    {
        var duration = endTime - startTime;
        decimal totalPrice = 0;

        // Note: Real-world implementation would check listing.DailyPricings for specific date overrides.
        // For now, we use the base prices.

        if (listing.Type == ListingType.Pool || (listing.Type == ListingType.Beach && duration.TotalHours < 24))
        {
            // Hourly or simple daily for single day
            var hours = (decimal)Math.Ceiling(duration.TotalHours);
            totalPrice = hours * listing.BasePricePerHour;
        }
        else
        {
            // Daily
            var days = (decimal)Math.Ceiling(duration.TotalDays);
            totalPrice = days * listing.BasePricePerDay;
        }

        // Apply guest multiplier if applicable (simplified)
        if (guestCount > 1)
        {
            // Example: +10% for each guest after the first
            // totalPrice *= (1 + (guestCount - 1) * 0.1m);
        }

        return Task.FromResult(totalPrice);
    }
}
