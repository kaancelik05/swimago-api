using Swimago.Domain.Entities;

namespace Swimago.Domain.Interfaces;

/// <summary>
/// Repository interface for newsletter subscriptions
/// </summary>
public interface INewsletterRepository : IRepository<NewsletterSubscriber>
{
    /// <summary>
    /// Get subscription by email address
    /// </summary>
    Task<NewsletterSubscriber?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all active subscriptions
    /// </summary>
    Task<IEnumerable<NewsletterSubscriber>> GetActiveSubscriptionsAsync(CancellationToken cancellationToken = default);
}
