using Microsoft.EntityFrameworkCore;
using Swimago.Domain.Entities;
using Swimago.Domain.Interfaces;
using Swimago.Infrastructure.Data;

namespace Swimago.Infrastructure.Repositories;

public class BlogCommentRepository : Repository<BlogComment>, IBlogCommentRepository
{
    public BlogCommentRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<BlogComment>> GetByBlogPostIdAsync(
        Guid blogPostId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var safePage = Math.Max(1, page);
        var safePageSize = Math.Clamp(pageSize, 1, 100);

        return await _dbSet
            .Include(x => x.User)
                .ThenInclude(x => x.Profile)
            .Where(x => x.BlogPostId == blogPostId)
            .OrderByDescending(x => x.CreatedAt)
            .Skip((safePage - 1) * safePageSize)
            .Take(safePageSize)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountByBlogPostIdAsync(Guid blogPostId, CancellationToken cancellationToken = default)
    {
        return _dbSet.CountAsync(x => x.BlogPostId == blogPostId, cancellationToken);
    }
}
