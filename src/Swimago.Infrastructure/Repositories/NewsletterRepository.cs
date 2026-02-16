using Microsoft.EntityFrameworkCore;
using Swimago.Domain.Entities;
using Swimago.Domain.Interfaces;
using Swimago.Infrastructure.Data;

namespace Swimago.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for newsletter subscriptions
/// </summary>
public class NewsletterRepository : Repository<NewsletterSubscriber>, INewsletterRepository
{
    public NewsletterRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <inheritdoc/>
    public async Task<NewsletterSubscriber?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(n => n.Email.ToLower() == email.ToLower(), cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<NewsletterSubscriber>> GetActiveSubscriptionsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(n => n.IsActive)
            .OrderBy(n => n.SubscribedAt)
            .ToListAsync(cancellationToken);
    }
}
