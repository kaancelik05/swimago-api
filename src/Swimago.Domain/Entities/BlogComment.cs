namespace Swimago.Domain.Entities;

public class BlogComment
{
    public Guid Id { get; set; }
    public Guid BlogPostId { get; set; }
    public Guid UserId { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public BlogPost BlogPost { get; set; } = null!;
    public User User { get; set; } = null!;
}
