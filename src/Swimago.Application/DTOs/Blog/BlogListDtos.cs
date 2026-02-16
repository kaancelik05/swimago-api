using Swimago.Application.DTOs.Common;

namespace Swimago.Application.DTOs.Blog;

/// <summary>
/// Blog list response with featured article
/// </summary>
public record BlogListResponse(
    BlogPostDto? FeaturedArticle,
    IEnumerable<BlogPostDto> Articles,
    IEnumerable<string> Categories,
    int TotalCount,
    int Page,
    int PageSize
);

/// <summary>
/// Blog post item for list view
/// </summary>
public record BlogPostDto(
    Guid Id,
    string Slug,
    string Title,
    string? Description,
    string? ImageUrl,
    string? Category,
    IEnumerable<string>? Tags,
    string AuthorName,
    string? AuthorAvatar,
    int ReadTime,
    int ViewCount,
    bool IsFeatured,
    DateTime PublishedAt
);

/// <summary>
/// Blog post detail response
/// </summary>
public record BlogDetailResponse(
    Guid Id,
    string Slug,
    MultiLanguageDto Title,
    MultiLanguageDto? Description,
    MultiLanguageDto Content,
    string? ImageUrl,
    string? Category,
    IEnumerable<string>? Tags,
    BlogAuthorDto Author,
    int ReadTime,
    int ViewCount,
    bool IsFeatured,
    DateTime PublishedAt,
    DateTime? UpdatedAt,
    IEnumerable<RelatedArticleDto>? RelatedArticles,
    BlogMetaDto? Meta
);

/// <summary>
/// Blog author info
/// </summary>
public record BlogAuthorDto(
    Guid Id,
    string Name,
    string? Avatar,
    string? Bio,
    int ArticleCount
);

/// <summary>
/// Related article summary
/// </summary>
public record RelatedArticleDto(
    Guid Id,
    string Slug,
    string Title,
    string? ImageUrl,
    string? Category,
    int ReadTime
);

/// <summary>
/// SEO metadata
/// </summary>
public record BlogMetaDto(
    string? MetaTitle,
    string? MetaDescription,
    string? OgImage
);
