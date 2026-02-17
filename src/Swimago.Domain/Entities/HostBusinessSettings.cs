namespace Swimago.Domain.Entities;

public class HostBusinessSettings
{
    public Guid Id { get; set; }
    public Guid HostId { get; set; }
    public bool AutoConfirmReservations { get; set; } = false;
    public bool AllowSameDayBookings { get; set; } = true;
    public int MinimumNoticeHours { get; set; } = 2;
    public int CancellationWindowHours { get; set; } = 24;
    public bool DynamicPricingEnabled { get; set; } = false;
    public bool SmartOverbookingProtection { get; set; } = true;
    public bool WhatsappNotifications { get; set; } = false;
    public bool EmailNotifications { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public User Host { get; set; } = null!;
}
