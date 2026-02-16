using AutoMapper;
using Swimago.Application.DTOs.Blog;
using Swimago.Application.DTOs.Common;
using Swimago.Application.Interfaces;
using Swimago.Domain.Entities;
using Swimago.Domain.Interfaces;

namespace Swimago.Application.Services;

public class BlogService : IBlogService
{
    private readonly IBlogPostRepository _blogRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public BlogService(IBlogPostRepository blogRepository, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _blogRepository = blogRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<BlogPostResponse> CreateBlogPostAsync(Guid authorId, CreateBlogPostRequest request, CancellationToken cancellationToken = default)
    {
        var slug = GenerateSlug(request.Title);

        var blogPost = new BlogPost
        {
            Id = Guid.NewGuid(),
            AuthorId = authorId,
            Slug = slug,
            Title = new Dictionary<string, string> { { "tr", request.Title } },
            Description = new Dictionary<string, string> { { "tr", request.Title.Length > 200 ? request.Title[..200] : request.Title } },
            Content = new Dictionary<string, string> { { "tr", request.Content } },
            ImageUrl = request.CoverImageUrl,
            IsPublished = request.IsPublished,
            IsFeatured = false,
            ReadTime = CalculateReadTime(request.Content),
            CreatedAt = DateTime.UtcNow,
            PublishedAt = request.IsPublished ? DateTime.UtcNow : null,
            ViewCount = 0
        };

        await _blogRepository.AddAsync(blogPost, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetBlogPostByIdAsync(blogPost.Id, cancellationToken)
            ?? throw new InvalidOperationException("Blog yazısı oluşturulduktan sonra alınamadı");
    }

    public async Task<BlogPostResponse> UpdateBlogPostAsync(Guid id, Guid authorId, UpdateBlogPostRequest request, CancellationToken cancellationToken = default)
    {
        var blogPost = await _blogRepository.GetByIdAsync(id, cancellationToken);
        if (blogPost == null)
            throw new KeyNotFoundException("Blog yazısı bulunamadı");

        if (blogPost.AuthorId != authorId)
            throw new UnauthorizedAccessException("Bu blog yazısını düzenleme yetkiniz yok");

        if (request.Title != null)
        {
            blogPost.Title["tr"] = request.Title;
            blogPost.Slug = GenerateSlug(request.Title);
        }

        if (request.Content != null)
        {
            blogPost.Content["tr"] = request.Content;
            blogPost.ReadTime = CalculateReadTime(request.Content);
        }

        if (request.CoverImageUrl != null)
            blogPost.ImageUrl = request.CoverImageUrl;

        if (request.IsPublished.HasValue)
            blogPost.IsPublished = request.IsPublished.Value;

        blogPost.UpdatedAt = DateTime.UtcNow;

        await _blogRepository.UpdateAsync(blogPost, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetBlogPostByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("Blog yazısı güncellendikten sonra alınamadı");
    }

    public async Task<bool> DeleteBlogPostAsync(Guid id, Guid authorId, CancellationToken cancellationToken = default)
    {
        var blogPost = await _blogRepository.GetByIdAsync(id, cancellationToken);
        if (blogPost == null)
            throw new KeyNotFoundException("Blog yazısı bulunamadı");

        if (blogPost.AuthorId != authorId)
            throw new UnauthorizedAccessException("Bu blog yazısını silme yetkiniz yok");

        await _blogRepository.DeleteAsync(blogPost, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<BlogPostResponse?> GetBlogPostByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var blogPost = await _blogRepository.GetByIdAsync(id, cancellationToken);
        if (blogPost == null) return null;

        await _blogRepository.IncrementViewCountAsync(id, cancellationToken);

        return _mapper.Map<BlogPostResponse>(blogPost);
    }

    public async Task<BlogPostResponse?> GetBlogPostBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var blogPost = await _blogRepository.GetBySlugAsync(slug, cancellationToken);
        if (blogPost == null) return null;

        await _blogRepository.IncrementViewCountAsync(blogPost.Id, cancellationToken);

        return _mapper.Map<BlogPostResponse>(blogPost);
    }

    public async Task<PagedResult<BlogPostResponse>> GetPublishedPostsAsync(PaginationQuery query, CancellationToken cancellationToken = default)
    {
        var posts = await _blogRepository.GetPublishedPostsAsync(cancellationToken);
        var totalCount = posts.Count();

        var pagedPosts = posts
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();

        var responses = _mapper.Map<IEnumerable<BlogPostResponse>>(pagedPosts);

        return new PagedResult<BlogPostResponse>
        {
            Items = responses,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<IEnumerable<BlogPostResponse>> GetAuthorPostsAsync(Guid authorId, CancellationToken cancellationToken = default)
    {
        var posts = await _blogRepository.GetByAuthorIdAsync(authorId, cancellationToken);
        return _mapper.Map<IEnumerable<BlogPostResponse>>(posts);
    }

    public async Task<bool> PublishPostAsync(Guid id, Guid authorId, CancellationToken cancellationToken = default)
    {
        var blogPost = await _blogRepository.GetByIdAsync(id, cancellationToken);
        if (blogPost == null)
            throw new KeyNotFoundException("Blog yazısı bulunamadı");

        if (blogPost.AuthorId != authorId)
            throw new UnauthorizedAccessException("Bu blog yazısını yayınlama yetkiniz yok");

        if (blogPost.IsPublished)
            throw new InvalidOperationException("Blog yazısı zaten yayınlanmış");

        blogPost.IsPublished = true;
        blogPost.PublishedAt = DateTime.UtcNow;

        await _blogRepository.UpdateAsync(blogPost, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static string GenerateSlug(string title)
    {
        var slug = title.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("ç", "c")
            .Replace("ğ", "g")
            .Replace("ı", "i")
            .Replace("ö", "o")
            .Replace("ş", "s")
            .Replace("ü", "u")
            .Replace("İ", "i");

        // Remove special characters
        slug = new string(slug.Where(c => char.IsLetterOrDigit(c) || c == '-').ToArray());

        return slug;
    }

    private static int CalculateReadTime(string content)
    {
        // Average reading speed: 200 words per minute
        var wordCount = content.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
        return Math.Max(1, wordCount / 200);
    }
}
