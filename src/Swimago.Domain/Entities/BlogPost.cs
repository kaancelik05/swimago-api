namespace Swimago.Domain.Entities;

public class BlogPost
{
    public Guid Id { get; set; }
    public Guid AuthorId { get; set; }
    
    public string Slug { get; set; } = string.Empty;
    public Dictionary<string, string> Title { get; set; } = new(); // Multi-language
    public Dictionary<string, string> Description { get; set; } = new(); // Multi-language excerpt
    public Dictionary<string, string> Content { get; set; } = new(); // Multi-language HTML/Markdown
    public string? ImageUrl { get; set; } // Featured image
    public string? Category { get; set; }
    public List<string>? Tags { get; set; }
    public int ReadTime { get; set; } // Minutes to read
    
    public bool IsPublished { get; set; }
    public bool IsFeatured { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    public int ViewCount { get; set; }
    
    // Navigation property
    public User Author { get; set; } = null!;
}
