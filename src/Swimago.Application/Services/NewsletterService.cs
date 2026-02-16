using Microsoft.Extensions.Logging;
using Swimago.Application.DTOs.Newsletter;
using Swimago.Application.Interfaces;
using Swimago.Domain.Entities;
using Swimago.Domain.Interfaces;

namespace Swimago.Application.Services;

public class NewsletterService : INewsletterService
{
    private readonly INewsletterRepository _newsletterRepository;
    private readonly ILogger<NewsletterService> _logger;

    public NewsletterService(
        INewsletterRepository newsletterRepository,
        ILogger<NewsletterService> logger)
    {
        _newsletterRepository = newsletterRepository;
        _logger = logger;
    }

    public async Task<NewsletterSubscribeResponse> SubscribeAsync(NewsletterSubscribeRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Newsletter subscription request: {Email}", request.Email);

        if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains('@'))
            return new NewsletterSubscribeResponse(false, "Geçerli bir e-posta adresi giriniz");

        var existing = await _newsletterRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existing != null)
        {
            if (existing.IsActive)
                return new NewsletterSubscribeResponse(true, "Bu e-posta adresi zaten kayıtlı");
            
            existing.IsActive = true;
            existing.UnsubscribedAt = null;
            await _newsletterRepository.UpdateAsync(existing, cancellationToken);
            
            return new NewsletterSubscribeResponse(true, "Bülten aboneliğiniz yeniden aktifleştirildi");
        }

        var subscription = new NewsletterSubscriber
        {
            Id = Guid.NewGuid(),
            Email = request.Email.ToLowerInvariant().Trim(),
            IsActive = true,
            SubscribedAt = DateTime.UtcNow
        };

        await _newsletterRepository.AddAsync(subscription, cancellationToken);

        _logger.LogInformation("Newsletter subscription created: {Email}", request.Email);

        return new NewsletterSubscribeResponse(true, "Bülten aboneliğiniz başarıyla oluşturuldu");
    }

    public async Task<NewsletterSubscribeResponse> UnsubscribeAsync(string email, string? token, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Newsletter unsubscription request: {Email}", email);

        var subscription = await _newsletterRepository.GetByEmailAsync(email, cancellationToken);
        if (subscription == null)
            return new NewsletterSubscribeResponse(false, "Bu e-posta adresi için abonelik bulunamadı");

        // Token verification would go here

        subscription.IsActive = false;
        subscription.UnsubscribedAt = DateTime.UtcNow;
        
        await _newsletterRepository.UpdateAsync(subscription, cancellationToken);

        _logger.LogInformation("Newsletter unsubscription completed: {Email}", email);

        return new NewsletterSubscribeResponse(true, "Bülten aboneliğiniz iptal edildi");
    }
}
