using Swimago.Application.DTOs.Common;

namespace Swimago.Application.DTOs.Blog;

public record BlogPostResponse(
    Guid Id,
    string Slug,
    MultiLanguageDto Title,
    MultiLanguageDto? Description,
    MultiLanguageDto Content,
    string? ImageUrl,
    string? Category,
    List<string>? Tags,
    int ReadTime,
    bool IsFeatured,
    Guid AuthorId,
    string AuthorName,
    bool IsPublished,
    DateTime CreatedAt,
    DateTime? PublishedAt,
    DateTime? UpdatedAt,
    int ViewCount
);
