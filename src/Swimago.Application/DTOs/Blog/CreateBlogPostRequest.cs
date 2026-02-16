namespace Swimago.Application.DTOs.Blog;

public record CreateBlogPostRequest(
    string Title,
    string Content,
    string? CoverImageUrl = null,
    bool IsPublished = false
);
