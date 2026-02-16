namespace Swimago.Application.DTOs.Blog;

public record UpdateBlogPostRequest(
    string? Title = null,
    string? Content = null,
    string? CoverImageUrl = null,
    bool? IsPublished = null
);
