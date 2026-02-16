namespace Swimago.Domain.Entities;

public class NewsletterSubscriber
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime SubscribedAt { get; set; }
    public DateTime? UnsubscribedAt { get; set; }
}
