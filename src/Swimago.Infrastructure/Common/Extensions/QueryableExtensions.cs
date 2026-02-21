using Microsoft.EntityFrameworkCore;
using Swimago.Application.Common.Models;

namespace Swimago.Infrastructure.Common.Extensions;

public static class QueryableExtensions
{
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> source, 
        int page, 
        int pageSize, 
        CancellationToken cancellationToken = default)
    {
        var count = await source.CountAsync(cancellationToken);
        
        if (count == 0)
        {
            return new PagedResult<T>(Enumerable.Empty<T>(), count, page, pageSize);
        }
        
        var items = await source.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        
        return new PagedResult<T>(items, count, page, pageSize);
    }
}
