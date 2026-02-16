using Microsoft.Extensions.Logging;
using Swimago.Application.Interfaces;

namespace Swimago.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        // TODO: Implement actual email sending (SMTP, SendGrid, Mailgun, etc.)
        _logger.LogInformation("Email would be sent to {To}: {Subject}", to, subject);
        await Task.CompletedTask;
    }

    public async Task SendReservationConfirmationAsync(string to, string guestName, string listingName, DateTime startTime, CancellationToken cancellationToken = default)
    {
        var subject = "Rezervasyonunuz Onaylandı";
        var body = $@"
            <h2>Merhaba {guestName},</h2>
            <p>Rezervasyonunuz başarıyla oluşturuldu.</p>
            <p><strong>Liste:</strong> {listingName}</p>
            <p><strong>Tarih:</strong> {startTime:dd MMMM yyyy HH:mm}</p>
            <p>İyi tatiller dileriz!</p>
        ";

        await SendEmailAsync(to, subject, body, cancellationToken);
    }

    public async Task SendReservationCancellationAsync(string to, string guestName, string listingName, CancellationToken cancellationToken = default)
    {
        var subject = "Rezervasyon İptali";
        var body = $@"
            <h2>Merhaba {guestName},</h2>
            <p>Rezervasyonunuz iptal edildi.</p>
            <p><strong>Liste:</strong> {listingName}</p>
            <p>Tekrar görüşmek üzere!</p>
        ";

        await SendEmailAsync(to, subject, body, cancellationToken);
    }

    public async Task SendReviewNotificationAsync(string to, string hostName, string listingName, int rating, CancellationToken cancellationToken = default)
    {
        var subject = "Yeni Yorum Aldınız";
        var body = $@"
            <h2>Merhaba {hostName},</h2>
            <p>Listeniz için yeni bir yorum aldınız.</p>
            <p><strong>Liste:</strong> {listingName}</p>
            <p><strong>Puan:</strong> {rating}/5</p>
            <p>Yorumu görmek için panele giriş yapabilirsiniz.</p>
        ";

        await SendEmailAsync(to, subject, body, cancellationToken);
    }

    public async Task SendWelcomeEmailAsync(string to, string userName, CancellationToken cancellationToken = default)
    {
        var subject = "Swimago'ya Hoş Geldiniz!";
        var body = $@"
            <h2>Merhaba {userName},</h2>
            <p>Swimago ailesine katıldığınız için teşekkür ederiz!</p>
            <p>Artık plaj, havuz ve tekne turları için rezervasyon yapabilirsiniz.</p>
            <p>İyi tatiller dileriz!</p>
        ";

        await SendEmailAsync(to, subject, body, cancellationToken);
    }
}
