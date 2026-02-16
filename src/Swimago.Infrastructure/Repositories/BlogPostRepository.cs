using Microsoft.EntityFrameworkCore;
using Swimago.Domain.Entities;
using Swimago.Domain.Interfaces;
using Swimago.Infrastructure.Data;

namespace Swimago.Infrastructure.Repositories;

public class BlogPostRepository : Repository<BlogPost>, IBlogPostRepository
{
    public BlogPostRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<BlogPost>> GetPublishedPostsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(b => b.Author)
                .ThenInclude(a => a.Profile)
            .Where(b => b.IsPublished)
            .OrderByDescending(b => b.PublishedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<BlogPost?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(b => b.Author)
                .ThenInclude(a => a.Profile)
            .FirstOrDefaultAsync(b => b.Slug == slug && b.IsPublished, cancellationToken);
    }

    public async Task<BlogPost?> GetFeaturedAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(b => b.Author)
                .ThenInclude(a => a.Profile)
            .Where(b => b.IsPublished && b.IsFeatured)
            .OrderByDescending(b => b.PublishedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<BlogPost>> GetByAuthorIdAsync(Guid authorId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(b => b.Author)
            .Where(b => b.AuthorId == authorId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<BlogPost>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(b => b.Author)
                .ThenInclude(a => a.Profile)
            .Where(b => b.IsPublished && b.Category == category)
            .OrderByDescending(b => b.PublishedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<string>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(b => b.IsPublished && b.Category != null)
            .Select(b => b.Category!)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public async Task IncrementViewCountAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var post = await _dbSet.FindAsync([id], cancellationToken);
        if (post != null)
        {
            post.ViewCount++;
        }
    }
}
