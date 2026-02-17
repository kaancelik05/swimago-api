namespace Swimago.Domain.Enums;

/// <summary>
/// Status of a listing in the approval workflow
/// </summary>
public enum ListingStatus
{
    Pending = 0,        // Onay bekliyor
    Active = 1,         // Aktif
    Inactive = 2,       // Pasif
    Rejected = 3,       // Reddedildi
    Draft = 4,          // Taslak
    PendingReview = 5   // Inceleme bekliyor
}
