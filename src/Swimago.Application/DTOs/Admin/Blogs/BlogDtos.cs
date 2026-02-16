using System.ComponentModel.DataAnnotations;
using Swimago.Application.DTOs.Admin.Shared;

namespace Swimago.Application.DTOs.Admin.Blogs;

public class BlogListItemDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public string ReadTime { get; set; } = string.Empty;
    public int ViewCount { get; set; }
    public bool? IsFeatured { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }
}

public class BlogDetailDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string HeroImageUrl { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public BlogAuthorDto Author { get; set; } = new();
    public string ReadTime { get; set; } = string.Empty;
    public List<ContentBlockDto> Content { get; set; } = new();
    public List<TOCItemDto> TableOfContents { get; set; } = new();
    public bool IsFeatured { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }
}

public class BlogAuthorDto
{
    public string Name { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string AvatarUrl { get; set; } = string.Empty;
}

public class ContentBlockDto
{
    public string Type { get; set; } = string.Empty;  // "paragraph","heading","subheading","image","quote","tip"
    public string? Text { get; set; }
    public string? ImageUrl { get; set; }
    public string? Caption { get; set; }
    public string? Author { get; set; }       // For quotes
    public int? Level { get; set; }           // Heading level (2, 3, 4)
    public string? Id { get; set; }           // Anchor link / TOC connection
}

public class TOCItemDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
}

public class CreateBlogRequest
{
    [Required] public string Title { get; set; } = string.Empty;
    [Required] public string Slug { get; set; } = string.Empty;
    [Required] public string Description { get; set; } = string.Empty;
    [Required] public string ImageUrl { get; set; } = string.Empty;
    [Required] public string HeroImageUrl { get; set; } = string.Empty;
    [Required] public string Category { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    [Required] public BlogAuthorDto Author { get; set; } = new();
    public string ReadTime { get; set; } = string.Empty;
    public List<ContentBlockDto> Content { get; set; } = new();
    public List<TOCItemDto> TableOfContents { get; set; } = new();
    public bool IsFeatured { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }
}
