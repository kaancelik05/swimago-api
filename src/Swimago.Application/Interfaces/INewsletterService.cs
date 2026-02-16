using Swimago.Application.DTOs.Newsletter;

namespace Swimago.Application.Interfaces;

public interface INewsletterService
{
    Task<NewsletterSubscribeResponse> SubscribeAsync(NewsletterSubscribeRequest request, CancellationToken cancellationToken = default);
    Task<NewsletterSubscribeResponse> UnsubscribeAsync(string email, string? token, CancellationToken cancellationToken = default);
}
