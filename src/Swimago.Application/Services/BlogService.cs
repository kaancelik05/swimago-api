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
    private readonly IBlogCommentRepository _blogCommentRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public BlogService(
        IBlogPostRepository blogRepository,
        IBlogCommentRepository blogCommentRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _blogRepository = blogRepository;
        _blogCommentRepository = blogCommentRepository;
        _userRepository = userRepository;
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
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<BlogPostResponse>(blogPost);
    }

    public async Task<BlogPostResponse?> GetBlogPostBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var blogPost = await _blogRepository.GetBySlugAsync(slug, cancellationToken);
        if (blogPost == null) return null;

        await _blogRepository.IncrementViewCountAsync(blogPost.Id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<BlogPostResponse>(blogPost);
    }

    public async Task<PagedResult<BlogPostResponse>> GetPublishedPostsAsync(PaginationQuery query, CancellationToken cancellationToken = default)
    {
        var posts = (await _blogRepository.GetPublishedPostsAsync(cancellationToken)).ToList();
        var totalCount = posts.Count;

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

    public async Task<CustomerBlogDetailResponse?> GetBlogDetailBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var post = await _blogRepository.GetBySlugAsync(slug, cancellationToken);
        if (post == null)
        {
            return null;
        }

        await _blogRepository.IncrementViewCountAsync(post.Id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var content = GetLocalizedText(post.Content);
        var toc = BuildTableOfContents(content).ToList();
        var blocks = BuildContentBlocks(content).ToList();

        return new CustomerBlogDetailResponse(
            Id: post.Id,
            Slug: post.Slug,
            Title: GetLocalizedText(post.Title),
            Description: GetLocalizedText(post.Description),
            HeroImageUrl: post.ImageUrl,
            Category: post.Category,
            Tags: post.Tags ?? [],
            ReadTime: post.ReadTime,
            PublishedAt: post.PublishedAt,
            Author: new CustomerBlogAuthorDto(
                Name: GetAuthorName(post.Author),
                Bio: GetLocalizedText(post.Author?.Profile?.Bio),
                AvatarUrl: post.Author?.Profile?.Avatar),
            TableOfContents: toc,
            ContentBlocks: blocks
        );
    }

    public async Task<BlogRelatedResponse> GetRelatedPostsBySlugAsync(string slug, int limit = 3, CancellationToken cancellationToken = default)
    {
        var post = await _blogRepository.GetBySlugAsync(slug, cancellationToken);
        if (post == null)
            throw new KeyNotFoundException("Blog yazısı bulunamadı");

        var related = new List<BlogPost>();

        if (!string.IsNullOrWhiteSpace(post.Category))
        {
            related.AddRange((await _blogRepository.GetByCategoryAsync(post.Category, cancellationToken))
                .Where(x => x.Id != post.Id));
        }

        if (related.Count < limit)
        {
            var fallback = await _blogRepository.GetPublishedPostsAsync(cancellationToken);
            related.AddRange(fallback.Where(x => x.Id != post.Id));
        }

        var items = related
            .GroupBy(x => x.Id)
            .Select(x => x.First())
            .Take(Math.Max(1, limit))
            .Select(x => new BlogRelatedItemDto(
                Slug: x.Slug,
                Title: GetLocalizedText(x.Title),
                Description: GetLocalizedText(x.Description),
                ImageUrl: x.ImageUrl,
                Category: x.Category))
            .ToList();

        return new BlogRelatedResponse(items);
    }

    public async Task<BlogCommentListResponse> GetCommentsBySlugAsync(
        string slug,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var post = await _blogRepository.GetBySlugAsync(slug, cancellationToken);
        if (post == null)
            throw new KeyNotFoundException("Blog yazısı bulunamadı");

        var safePage = Math.Max(1, page);
        var safePageSize = Math.Clamp(pageSize, 1, 100);

        var comments = await _blogCommentRepository.GetByBlogPostIdAsync(post.Id, safePage, safePageSize, cancellationToken);
        var totalCount = await _blogCommentRepository.CountByBlogPostIdAsync(post.Id, cancellationToken);
        var totalPages = (int)Math.Ceiling(totalCount / (double)safePageSize);

        var items = comments
            .Select(MapComment)
            .ToList();

        return new BlogCommentListResponse(
            Items: items,
            Page: safePage,
            PageSize: safePageSize,
            TotalCount: totalCount,
            TotalPages: totalPages,
            HasPrevious: safePage > 1,
            HasNext: safePage < totalPages);
    }

    public async Task<BlogCommentDto> AddCommentBySlugAsync(
        Guid userId,
        string slug,
        CreateBlogCommentRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Text))
            throw new ArgumentException("Yorum metni boş olamaz");

        var post = await _blogRepository.GetBySlugAsync(slug, cancellationToken);
        if (post == null)
            throw new KeyNotFoundException("Blog yazısı bulunamadı");

        var user = await _userRepository.GetWithProfileAsync(userId, cancellationToken)
            ?? throw new KeyNotFoundException("Kullanıcı bulunamadı");

        var comment = new BlogComment
        {
            Id = Guid.NewGuid(),
            BlogPostId = post.Id,
            UserId = userId,
            Text = request.Text.Trim(),
            CreatedAt = DateTime.UtcNow,
            User = user
        };

        await _blogCommentRepository.AddAsync(comment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapComment(comment);
    }

    private static BlogCommentDto MapComment(BlogComment comment)
    {
        return new BlogCommentDto(
            Id: comment.Id,
            Author: new BlogCommentAuthorDto(
                Name: GetAuthorName(comment.User),
                AvatarUrl: comment.User?.Profile?.Avatar),
            Text: comment.Text,
            CreatedAt: comment.CreatedAt
        );
    }

    private static IEnumerable<CustomerBlogTocItemDto> BuildTableOfContents(string content)
    {
        var lines = content.Split('\n', StringSplitOptions.TrimEntries);
        foreach (var line in lines)
        {
            if (!line.StartsWith("#"))
            {
                continue;
            }

            var title = line.TrimStart('#').Trim();
            if (string.IsNullOrWhiteSpace(title))
            {
                continue;
            }

            yield return new CustomerBlogTocItemDto(
                Id: GenerateSlug(title),
                Title: title
            );
        }
    }

    private static IEnumerable<CustomerBlogContentBlockDto> BuildContentBlocks(string content)
    {
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var line in lines)
        {
            if (line.StartsWith("#"))
            {
                var heading = line.TrimStart('#').Trim();
                yield return new CustomerBlogContentBlockDto(
                    Type: "heading",
                    Id: GenerateSlug(heading),
                    Text: heading,
                    ImageUrl: null,
                    Caption: null,
                    Author: null);
                continue;
            }

            yield return new CustomerBlogContentBlockDto(
                Type: "paragraph",
                Id: null,
                Text: line,
                ImageUrl: null,
                Caption: null,
                Author: null);
        }
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

        slug = new string(slug.Where(c => char.IsLetterOrDigit(c) || c == '-').ToArray());

        return slug;
    }

    private static int CalculateReadTime(string content)
    {
        var wordCount = content.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
        return Math.Max(1, wordCount / 200);
    }

    private static string GetLocalizedText(Dictionary<string, string>? values)
    {
        if (values == null || values.Count == 0)
        {
            return string.Empty;
        }

        if (values.TryGetValue("tr", out var tr) && !string.IsNullOrWhiteSpace(tr))
        {
            return tr;
        }

        if (values.TryGetValue("en", out var en) && !string.IsNullOrWhiteSpace(en))
        {
            return en;
        }

        return values.Values.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)) ?? string.Empty;
    }

    private static string GetAuthorName(User? user)
    {
        if (user?.Profile == null)
        {
            return "Anonim";
        }

        var firstName = GetLocalizedText(user.Profile.FirstName);
        var lastName = GetLocalizedText(user.Profile.LastName);
        var fullName = $"{firstName} {lastName}".Trim();

        return string.IsNullOrWhiteSpace(fullName) ? "Anonim" : fullName;
    }
}
