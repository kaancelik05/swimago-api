namespace Swimago.Application.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default);
    Task SendReservationConfirmationAsync(string to, string guestName, string listingName, DateTime startTime, CancellationToken cancellationToken = default);
    Task SendReservationCancellationAsync(string to, string guestName, string listingName, CancellationToken cancellationToken = default);
    Task SendReviewNotificationAsync(string to, string hostName, string listingName, int rating, CancellationToken cancellationToken = default);
    Task SendWelcomeEmailAsync(string to, string userName, CancellationToken cancellationToken = default);
}
