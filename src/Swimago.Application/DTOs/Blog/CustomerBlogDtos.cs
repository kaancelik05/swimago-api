using Swimago.Application.DTOs.Common;

namespace Swimago.Application.DTOs.Blog;

public record CustomerBlogDetailResponse(
    Guid Id,
    string Slug,
    string Title,
    string? Description,
    string? HeroImageUrl,
    string? Category,
    IEnumerable<string> Tags,
    int ReadTime,
    DateTime? PublishedAt,
    CustomerBlogAuthorDto Author,
    IEnumerable<CustomerBlogTocItemDto> TableOfContents,
    IEnumerable<CustomerBlogContentBlockDto> ContentBlocks
);

public record CustomerBlogAuthorDto(
    string Name,
    string? Bio,
    string? AvatarUrl
);

public record CustomerBlogTocItemDto(
    string Id,
    string Title
);

public record CustomerBlogContentBlockDto(
    string Type,
    string? Id,
    string? Text,
    string? ImageUrl,
    string? Caption,
    string? Author
);

public record BlogRelatedResponse(
    IEnumerable<BlogRelatedItemDto> Items
);

public record BlogRelatedItemDto(
    string Slug,
    string Title,
    string? Description,
    string? ImageUrl,
    string? Category
);

public record BlogCommentListResponse(
    IEnumerable<BlogCommentDto> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages,
    bool HasPrevious,
    bool HasNext
);

public record BlogCommentDto(
    Guid Id,
    BlogCommentAuthorDto Author,
    string Text,
    DateTime CreatedAt
);

public record BlogCommentAuthorDto(
    string Name,
    string? AvatarUrl
);

public record CreateBlogCommentRequest(
    string Text
);
