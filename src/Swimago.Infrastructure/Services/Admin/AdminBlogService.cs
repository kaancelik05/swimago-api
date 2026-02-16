using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Swimago.Application.DTOs.Admin.Blogs;
using Swimago.Application.DTOs.Admin.Shared;
using Swimago.Application.Interfaces;
using Swimago.Domain.Entities;
using Swimago.Infrastructure.Data;

namespace Swimago.Infrastructure.Services.Admin;

public class AdminBlogService : IAdminBlogService
{
    private readonly ApplicationDbContext _context;
    private readonly JsonSerializerOptions _jsonOptions;

    public AdminBlogService(ApplicationDbContext context)
    {
        _context = context;
        _jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    }

    public async Task<PaginatedResponse<BlogListItemDto>> GetBlogsAsync(string? search, string? category, bool? isPublished, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.BlogPosts.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            // Assuming title search in default language or slug
            query = query.Where(b => b.Slug.Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(b => b.Category == category);
        }

        if (isPublished.HasValue)
        {
            query = query.Where(b => b.IsPublished == isPublished.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(b => b.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(b => new BlogListItemDto
            {
                Id = b.Id,
                Slug = b.Slug,
                ImageUrl = b.ImageUrl ?? "",
                Category = b.Category ?? "",
                ViewCount = b.ViewCount,
                IsPublished = b.IsPublished,
                PublishedAt = b.PublishedAt,
                IsFeatured = b.IsFeatured,
                // Title mapping simplified
                Title = b.Slug 
            })
            .ToListAsync(cancellationToken);

        return new PaginatedResponse<BlogListItemDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<BlogDetailDto> GetBlogAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var blog = await _context.BlogPosts
            .Include(b => b.Author).ThenInclude(u => u.Profile)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

        if (blog == null) throw new KeyNotFoundException($"Blog with ID {id} not found.");

        return MapToBlogDetailDto(blog);
    }

    public async Task<BlogDetailDto> CreateBlogAsync(CreateBlogRequest request, CancellationToken cancellationToken = default)
    {
        if (await _context.BlogPosts.AnyAsync(b => b.Slug == request.Slug, cancellationToken))
            throw new InvalidOperationException($"Blog with slug '{request.Slug}' already exists.");

        // In a real app, resolve AuthorId from logged in user or request
        // For now, assuming a system user or first admin found
        var author = await _context.Users.FirstOrDefaultAsync(cancellationToken);
        if (author == null) throw new InvalidOperationException("No users found to assign as author.");

        var blog = new BlogPost
        {
            Id = Guid.NewGuid(),
            Slug = request.Slug,
            ImageUrl = request.ImageUrl,
            Category = request.Category,
            IsPublished = request.IsPublished,
            IsFeatured = request.IsFeatured,
            PublishedAt = request.PublishedAt,
            CreatedAt = DateTime.UtcNow,
            AuthorId = author.Id,
            
            // Map Multi-Language fields (assuming request has default lang content for now)
            Title = new Dictionary<string, string> { { "en", request.Title } },
            Description = new Dictionary<string, string> { { "en", request.Description } },
            
            // Store content as JSONB in Content dictionary or specific field if extended
            Content = new Dictionary<string, string> 
            { 
                 { "en", JsonSerializer.Serialize(request.Content, _jsonOptions) } 
            },
            
            Tags = request.Tags,
            ReadTime = int.TryParse(request.ReadTime.Split(' ')[0], out int minutes) ? minutes : 5
        };

        _context.BlogPosts.Add(blog);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToBlogDetailDto(blog);
    }

    public async Task<BlogDetailDto> UpdateBlogAsync(Guid id, CreateBlogRequest request, CancellationToken cancellationToken = default)
    {
        var blog = await _context.BlogPosts.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
        if (blog == null) throw new KeyNotFoundException($"Blog with ID {id} not found.");

        blog.Slug = request.Slug;
        blog.ImageUrl = request.ImageUrl;
        blog.Category = request.Category;
        blog.IsPublished = request.IsPublished;
        blog.IsFeatured = request.IsFeatured;
        blog.PublishedAt = request.PublishedAt;
        blog.UpdatedAt = DateTime.UtcNow;
        blog.Tags = request.Tags;
        
        blog.Title = new Dictionary<string, string> { { "en", request.Title } };
        blog.Description = new Dictionary<string, string> { { "en", request.Description } };
        blog.Content = new Dictionary<string, string> 
        { 
             { "en", JsonSerializer.Serialize(request.Content, _jsonOptions) } 
        };

        await _context.SaveChangesAsync(cancellationToken);
        return MapToBlogDetailDto(blog);
    }

    public async Task DeleteBlogAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var blog = await _context.BlogPosts.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
        if (blog != null)
        {
            _context.BlogPosts.Remove(blog);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    private BlogDetailDto MapToBlogDetailDto(BlogPost b)
    {
        return new BlogDetailDto
        {
            Id = b.Id,
            Slug = b.Slug,
            ImageUrl = b.ImageUrl ?? "",
            Category = b.Category ?? "",
            IsPublished = b.IsPublished,
            IsFeatured = b.IsFeatured,
            PublishedAt = b.PublishedAt,
            Title = b.Title.GetValueOrDefault("en") ?? "",
            Description = b.Description.GetValueOrDefault("en") ?? "",
            Tags = b.Tags ?? new List<string>(),
            ReadTime = $"{b.ReadTime} min read"
        };
    }
}
