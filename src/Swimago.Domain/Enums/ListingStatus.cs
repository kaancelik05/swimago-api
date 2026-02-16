namespace Swimago.Domain.Enums;

/// <summary>
/// Status of a listing in the approval workflow
/// </summary>
public enum ListingStatus
{
    Pending,    // Onay bekliyor
    Active,     // Aktif
    Inactive,   // Pasif
    Rejected    // Reddedildi
}
