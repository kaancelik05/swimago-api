using Swimago.Domain.Entities;

namespace Swimago.Domain.Interfaces;

public interface IBlogCommentRepository : IRepository<BlogComment>
{
    Task<IEnumerable<BlogComment>> GetByBlogPostIdAsync(Guid blogPostId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountByBlogPostIdAsync(Guid blogPostId, CancellationToken cancellationToken = default);
}
