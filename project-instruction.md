---
name: Backend Development Guide - Final Version
overview: Swimago projesi için kapsamlı backend geliştirme rehberi dokümanı. .NET 9 + Supabase/PostgreSQL + Cloudflare R2 + Hangfire + OpenAI GPT-4o-mini teknolojilerini kullanarak tam kapsamlı bir backend sistemi tasarlanacak.
todos: []
---

# Backend Development Guide - Swimago

## Teknoloji Stack

### Core Framework
- **.NET Core 9** - Backend framework
- **EF Core 9** - ORM
- **PostgreSQL** - Database (via Supabase)

### Third-Party Services
- **Supabase** - PostgreSQL hosting, Authentication, Storage
- **Cloudflare R2** - Object storage (images)
- **Cloudflare CDN/WAF** - Content delivery & Security
- **OpenAI GPT-4o-mini** - AI-powered translations

### Libraries
- **Hangfire** - Background job scheduler
- **FluentEmail** - Email service abstraction (provider to be determined)
- **SignalR** - Real-time communication
- **PostGIS** - Geospatial queries
- **Npgsql** - PostgreSQL driver with JSONB support

### Target Languages (Initial)
- **tr** - Türkçe (Default)
- **en** - English
- **de** - Deutsch
- **ru** - Русский

---

## 1. Core Entities & Database Schema

### 1.1 User & Authentication Entities

```csharp
public class User
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public Role Role { get; set; } // Admin, Host, Customer
    public bool IsEmailVerified { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime RefreshTokenExpiry { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}

public class UserProfile
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public Dictionary<string, string> FirstName { get; set; } // Multi-language
    public Dictionary<string, string> LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? ProfileImageUrl { get; set; } // Cloudflare R2 URL
    public Dictionary<string, string>? Bio { get; set; }
}

public enum Role
{
    Admin,
    Host,
    Customer
}
```

### 1.2 Listing Management (Multi-Type Support)

```csharp
public class Listing
{
    public int Id { get; set; }
    public int HostId { get; set; }
    public ListingType Type { get; set; } // Beach, Pool, BoatTour
    public bool IsActive { get; set; }

    // Multi-language content (JSONB)
    public Dictionary<string, string> Title { get; set; } // { "tr": "Plaj Adı", "en": "Beach Name" }
    public Dictionary<string, string> Description { get; set; }
    public Dictionary<string, string> Address { get; set; }

    // Geospatial data (PostGIS)
    public GeographyPoint Location { get; set; } // PostGIS Geography type
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }

    // Capacity
    public int MaxGuestCount { get; set; }

    // Images
    public ICollection<ListingImage> Images { get; set; }

    // Dynamic pricing calendar
    public ICollection<DailyPricing> PricingCalendar { get; set; }

    // Availability
    public ICollection<AvailabilityBlock> AvailabilityBlocks { get; set; }

    // Amenities
    public ICollection<ListingAmenity> Amenities { get; set; }

    // Metadata
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public decimal Rating { get; set; }
    public int ReviewCount { get; set; }
}

public enum ListingType
{
    Beach,
    Pool,
    BoatTour
}

public class ListingImage
{
    public int Id { get; set; }
    public int ListingId { get; set; }
    public string ImageUrl { get; set; } // Cloudflare R2 CDN URL
    public int DisplayOrder { get; set; }
    public bool IsPrimary { get; set; }
    public Dictionary<string, string>? AltText { get; set; } // Multi-language
}

public class DailyPricing
{
    public int Id { get; set; }
    public int ListingId { get; set; }
    public DateOnly Date { get; set; }
    public decimal Price { get; set; } // Özgür fiyatlandırma
    public decimal? HourlyPrice { get; set; } // Opsiyonel saatlik fiyat
    public bool IsAvailable { get; set; }
    public string? Notes { get; set; } // Özel notlar
}

public class AvailabilityBlock
{
    public int Id { get; set; }
    public int ListingId { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public bool IsBlocked { get; set; }
    public string? Reason { get; set; } // Özel sebep
}

public class Amenity
{
    public int Id { get; set; }
    public string IconName { get; set; } // Material Icons, FontAwesome vb.
    public Dictionary<string, string> Name { get; set; } // Multi-language
    public Dictionary<string, string>? Description { get; set; }
    public AmenityType Type { get; set; }
    public bool IsActive { get; set; }
}

public enum AmenityType
{
    General,
    Safety,
    Entertainment,
    FoodAndDrink,
    Sports,
    Family
}

public class ListingAmenity
{
    public int ListingId { get; set; }
    public int AmenityId { get; set; }
    public Amenity Amenity { get; set; }
}
```

### 1.3 Reservation & Booking Entities

```csharp
public class Reservation
{
    public int Id { get; set; }
    public int ListingId { get; set; }
    public int GuestId { get; set; }
    public Listing Listing { get; set; }
    public User Guest { get; set; }

    // Booking details
    public BookingType BookingType { get; set; } // Hourly, Daily
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int GuestCount { get; set; }

    // Pricing
    public decimal UnitPrice { get; set; }
    public int UnitCount { get; set; } // Saat veya gün sayısı
    public decimal TotalPrice { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal FinalPrice { get; set; }

    // Status
    public ReservationStatus Status { get; set; }
    public Dictionary<string, string>? SpecialRequests { get; set; } // Multi-language

    // Payment
    public ReservationPayment? Payment { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }
}

public enum BookingType
{
    Hourly,
    Daily
}

public enum ReservationStatus
{
    Pending,
    Confirmed,
    InProgress,
    Completed,
    Cancelled,
    NoShow
}

public class ReservationPayment
{
    public int Id { get; set; }
    public int ReservationId { get; set; }
    public string Provider { get; set; } // Stripe, Iyzico, Custom (en son aşamada belirlenecek)
    public string? ProviderTransactionId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } // TRY, USD, EUR
    public PaymentStatus Status { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? RefundedAt { get; set; }
    public decimal? RefundedAmount { get; set; }
}

public enum PaymentStatus
{
    Pending,
    Processing,
    Completed,
    Failed,
    Refunded,
    PartiallyRefunded
}
```

### 1.4 Content Entities (JSONB Multi-Language)

```csharp
public class Review
{
    public int Id { get; set; }
    public int ReservationId { get; set; }
    public int ListingId { get; set; }
    public int GuestId { get; set; }

    public int Rating { get; set; } // 1-5
    public Dictionary<string, string> Comment { get; set; } // Multi-language

    // Host response
    public Dictionary<string, string>? HostResponse { get; set; }
    public DateTime? HostResponseDate { get; set; }

    // Metadata
    public DateTime CreatedAt { get; set; }
    public bool IsVerified { get; set; } // Verified purchase
}

public class BlogPost
{
    public int Id { get; set; }
    public int AuthorId { get; set; }

    public Dictionary<string, string> Title { get; set; }
    public Dictionary<string, string> Content { get; set; }
    public Dictionary<string, string> Slug { get; set; } // Multi-language slug
    public string? FeaturedImageUrl { get; set; } // Cloudflare R2 URL

    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public int ViewCount { get; set; }
}

public class StaticContentPage
{
    public int Id { get; set; }
    public string PageType { get; set; } // About, Terms, Privacy, Contact, etc.
    public Dictionary<string, string> Title { get; set; }
    public Dictionary<string, string> Content { get; set; }
    public bool IsActive { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

### 1.5 Multi-Language Management Entities

```csharp
public class TranslationTask
{
    public int Id { get; set; }
    public string EntityType { get; set; } // Listing, Review, BlogPost, Amenity
    public int EntityId { get; set; }

    // Translation details
    public string SourceLanguage { get; set; } // "tr"
    public string[] TargetLanguages { get; set; } // ["en", "de", "ru"]
    public Dictionary<string, string> SourceFields { get; set; } // { "Title": "Plaj Adı", "Description": "..." }

    // Status
    public TranslationTaskStatus Status { get; set; }
    public int RetryCount { get; set; }
    public string? LastError { get; set; }
    public Dictionary<string, string>? TranslationResults { get; set; } // { "en_Title": "Beach Name", "de_Title": "Strand Name" }

    // AI provider info
    public string AIProvider { get; set; } // "openai-gpt-4o-mini"
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime? NextRetryAt { get; set; }
}

public enum TranslationTaskStatus
{
    Pending,
    Processing,
    Completed,
    Failed,
    PartiallyCompleted
}

public class SupportedLanguage
{
    public string Code { get; set; } // "tr", "en", "de", "ru"
    public string Name { get; set; } // "Turkish"
    public string NativeName { get; set; } // "Türkçe"
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
    public int Priority { get; set; } // Background job sıralaması için
    public string? Locale { get; set; } // "tr-TR", "en-US"
}

public class TranslationCache
{
    public int Id { get; set; }
    public string SourceTextHash { get; set; } // SHA256 hash
    public string SourceLanguage { get; set; }
    public string TargetLanguage { get; set; }
    public string SourceText { get; set; }
    public string TranslatedText { get; set; }
    public string AIProvider { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

### 1.6 Other Entities

```csharp
public class Favorite
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ListingId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class Notification
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public NotificationType Type { get; set; }
    public string Title { get; set; }
    public string Message { get; set; }
    public string? ActionUrl { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}

public enum NotificationType
{
    NewReservation,
    ReservationConfirmed,
    ReservationCancelled,
    NewReview,
    PaymentReceived,
    SystemAlert
}

public class ActivityLog
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string Action { get; set; }
    public string EntityType { get; set; }
    public int? EntityId { get; set; }
    public Dictionary<string, object>? Changes { get; set; } // JSONB
    public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SystemSetting
{
    public string Key { get; set; } // "site_name", "support_email"
    public string Value { get; set; }
    public string Description { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

---

## 2. API Endpoints Design

### 2.1 Authentication & Authorization

```http
POST   /api/auth/register
POST   /api/auth/login
POST   /api/auth/logout
POST   /api/auth/refresh-token
POST   /api/auth/forgot-password
POST   /api/auth/reset-password
POST   /api/auth/verify-email
GET    /api/auth/me
PUT    /api/auth/profile
POST   /api/auth/oauth/{provider}  # Google, Facebook, Apple
GET    /api/auth/oauth/{provider}/callback
```

### 2.2 Listings Management (Multi-Type)

```http
# Listing CRUD
GET    /api/listings
POST   /api/listings                    # Host only
GET    /api/listings/{id}
PUT    /api/listings/{id}               # Owner only
DELETE /api/listings/{id}               # Owner only

# Dynamic Pricing Calendar
GET    /api/listings/{id}/pricing-calendar?startDate=2026-01-01&endDate=2026-12-31
POST   /api/listings/{id}/pricing-calendar/daily
PUT    /api/listings/{id}/pricing-calendar/daily/{pricingId}
DELETE /api/listings/{id}/pricing-calendar/daily/{pricingId}
POST   /api/listings/{id}/pricing-calendar/bulk  # Birden fazla gün için toplu fiyat güncelleme

# Availability Management
GET    /api/listings/{id}/availability
POST   /api/listings/{id}/availability/blocks
PUT    /api/listings/{id}/availability/blocks/{blockId}
DELETE /api/listings/{id}/availability/blocks/{blockId}

# Images
POST   /api/listings/{id}/images        # Upload to Cloudflare R2
GET    /api/listings/{id}/images
PUT    /api/listings/{id}/images/{imageId}
DELETE /api/listings/{id}/images/{imageId}
POST   /api/listings/{id}/images/reorder

# Amenities
GET    /api/listings/{id}/amenities
POST   /api/listings/{id}/amenities
DELETE /api/listings/{id}/amenities/{amenityId}
```

### 2.3 Reservations & Bookings

```http
# Reservations
GET    /api/reservations                # Current user
POST   /api/reservations                # Create new booking
GET    /api/reservations/{id}
PUT    /api/reservations/{id}/cancel
PUT    /api/reservations/{id}/special-requests

# Availability Check
GET    /api/listings/{id}/availability/check?startDate=2026-01-01&endDate=2026-01-05&guestCount=2

# Host Reservations
GET    /api/host/reservations
GET    /api/host/reservations/{id}
PUT    /api/host/reservations/{id}/status  # Confirm, Reject
PUT    /api/host/reservations/{id}/cancel  # Force cancel
```

### 2.4 Reviews & Ratings

```http
GET    /api/listings/{id}/reviews?page=1&pageSize=10&sort=latest
POST   /api/listings/{id}/reviews        # After completed reservation
GET    /api/reviews/{id}
DELETE /api/reviews/{id}                 # Author only

# Host Response
POST   /api/reviews/{id}/response        # Host only
PUT    /api/reviews/{id}/response
```

### 2.5 Blog Posts

```http
GET    /api/blog/posts?page=1&pageSize=10
GET    /api/blog/posts/{slug}
GET    /api/blog/posts/{id}

# Admin only
POST   /api/admin/blog/posts
PUT    /api/admin/blog/posts/{id}
DELETE /api/admin/blog/posts/{id}
POST   /api/admin/blog/posts/{id}/publish
POST   /api/admin/blog/posts/{id}/unpublish
```

### 2.6 Payments (To be determined - Final Stage)

```http
POST   /api/payments/create-intent
POST   /api/payments/confirm
POST   /api/payments/cancel
POST   /api/payments/refund
GET    /api/payments/{reservationId}
POST   /api/payments/webhook            # Provider-specific
```

### 2.7 Multi-Language & Translation Management

```http
# Languages
GET    /api/content/languages            # Get all supported languages

# Translation Status
GET    /api/content/translate/status/{entityType}/{entityId}
GET    /api/content/translate/{entityType}/{entityId}/{languageCode}

# Admin Translation Management
GET    /api/admin/translation-tasks?status=failed&page=1
POST   /api/admin/translation-tasks/trigger-all
POST   /api/admin/translation-tasks/{id}/retry
POST   /api/admin/translation-tasks/{id}/cancel
GET    /api/admin/translation-cache/stats
POST   /api/admin/translation-cache/clear
```

### 2.8 Host Panel Endpoints

```http
GET    /api/host/dashboard
GET    /api/host/listings
GET    /api/host/revenue?startDate=2026-01-01&endDate=2026-01-31
GET    /api/host/revenue/summary         # Aggregated stats
GET    /api/host/reservations
GET    /api/host/reviews/pending         # Awaiting response
```

### 2.9 Admin Panel Endpoints

```http
GET    /api/admin/dashboard
GET    /api/admin/users
PUT    /api/admin/users/{id}/role
DELETE /api/admin/users/{id}
GET    /api/admin/listings
PUT    /api/admin/listings/{id}/approve
PUT    /api/admin/listings/{id}/reject
DELETE /api/admin/listings/{id}
GET    /api/admin/settings
PUT    /api/admin/settings
GET    /api/admin/activity-logs
GET    /api/admin/translation-monitoring
GET    /api/admin/hangfire/jobs          # Background job monitoring
```

### 2.10 Search & Discovery

```http
GET    /api/search?q=plaj&type=Beach&lat=41.0082&lng=28.9784&radius=10
GET    /api/search/autocomplete?q=plas
GET    /api/nearby?lat=41.0082&lng=28.9784&radius=5&type=Beach,Pool
```

---

## 3. Authentication & Authorization

### 3.1 Implementation Details

```csharp
// JWT Token Service
public interface IJwtTokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken(User user);
    ClaimsPrincipal? ValidateAccessToken(string token);
    ClaimsPrincipal? ValidateRefreshToken(string token);
}

// Password Hashing
public class PasswordService
{
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}

// Role-Based Authorization
public enum Permission
{
    ManageListings,
    ManageUsers,
    ManageTranslations,
    ViewAnalytics,
    // ...
}

[AttributeUsage(AttributeTargets.Method)]
public class RequirePermissionAttribute : Attribute
{
    public Permission RequiredPermission { get; }
    public RequirePermissionAttribute(Permission permission)
    {
        RequiredPermission = permission;
    }
}
```

### 3.2 Middleware Pipeline

```csharp
public class JwtMiddleware
{
    public async Task InvokeAsync(HttpContext context, IUserService userService, IJwtTokenService jwtService)
    {
        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

        if (token != null)
        {
            var principal = jwtService.ValidateAccessToken(token);
            if (principal != null)
            {
                var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdClaim, out int userId))
                {
                    var user = await userService.GetByIdAsync(userId);
                    if (user != null)
                    {
                        context.Items["User"] = user;
                    }
                }
            }
        }

        await _next(context);
    }
}
```

---

## 4. File Upload & Storage (Cloudflare R2)

### 4.1 Cloudflare R2 Integration

```csharp
public interface ICloudflareR2Service
{
    Task<string> UploadImageAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default);
    Task DeleteImageAsync(string imageUrl, CancellationToken cancellationToken = default);
    string GetPublicUrl(string objectKey);
    string GetOptimizedImageUrl(string objectKey, int? width = null, int? height = null, string format = "webp");
}

public class CloudflareR2Service : ICloudflareR2Service
{
    private readonly CloudflareR2Options _options;
    private readonly S3Client _s3Client;

    public CloudflareR2Service(IOptions<CloudflareR2Options> options)
    {
        _options = options.Value;
        var config = new AmazonS3Config
        {
            ServiceURL = _options.Endpoint,
            ForcePathStyle = true
        };
        _s3Client = new S3Client(_options.AccessKeyId, _options.SecretAccessKey, config);
    }

    public async Task<string> UploadImageAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        var objectKey = $"listings/{Guid.NewGuid()}/{fileName}";

        var request = new PutObjectRequest
        {
            BucketName = _options.BucketName,
            Key = objectKey,
            InputStream = fileStream,
            ContentType = contentType,
            CannedACL = S3CannedACL.PublicRead
        };

        await _s3Client.PutObjectAsync(request, cancellationToken);
        return GetPublicUrl(objectKey);
    }

    public string GetPublicUrl(string objectKey)
    {
        return $"https://{_options.CustomDomain}/{objectKey}";
    }

    public string GetOptimizedImageUrl(string objectKey, int? width = null, int? height = null, string format = "webp")
    {
        var baseUrl = GetPublicUrl(objectKey);
        var queryParams = new List<string>();

        if (width.HasValue) queryParams.Add($"width={width.Value}");
        if (height.HasValue) queryParams.Add($"height={height.Value}");
        queryParams.Add($"format={format}");

        return queryParams.Count > 0 ? $"{baseUrl}?{string.Join("&", queryParams)}" : baseUrl;
    }
}
```

### 4.2 Image Optimization Strategy

- Use Cloudflare CDN with image transformations
- Automatic format conversion (WebP, AVIF)
- Responsive image serving
- Lazy loading hints via headers

---

## 5. Dynamic Pricing System

### 5.1 Pricing Calendar Logic

```csharp
public interface IPricingService
{
    Task<decimal> CalculateTotalPriceAsync(int listingId, DateTime startTime, DateTime endTime, BookingType bookingType);
    Task<DailyPricing[]> GetPricingForDateRangeAsync(int listingId, DateOnly startDate, DateOnly endDate);
    Task UpdateDailyPricingAsync(int listingId, DateOnly date, decimal? dailyPrice, decimal? hourlyPrice);
    Task BulkUpdatePricingAsync(int listingId, Dictionary<DateOnly, (decimal? dailyPrice, decimal? hourlyPrice)> priceUpdates);
}

public class PricingService : IPricingService
{
    public async Task<decimal> CalculateTotalPriceAsync(int listingId, DateTime startTime, DateTime endTime, BookingType bookingType)
    {
        var listing = await _listingRepository.GetByIdAsync(listingId);
        var pricingCalendar = await _pricingRepository.GetByListingAndDateRangeAsync(
            listingId,
            DateOnly.FromDateTime(startTime),
            DateOnly.FromDateTime(endTime.AddDays(-1))
        );

        decimal total = 0;

        if (bookingType == BookingType.Daily)
        {
            var days = DateOnly.FromDateTime(startTime).DaysUntil(DateOnly.FromDateTime(endTime));
            for (int i = 0; i < days; i++)
            {
                var currentDate = DateOnly.FromDateTime(startTime).AddDays(i);
                var pricing = pricingCalendar.FirstOrDefault(p => p.Date == currentDate);
                total += pricing?.Price ?? listing.DefaultDailyPrice ?? 0;
            }
        }
        else // Hourly
        {
            var hours = (int)(endTime - startTime).TotalHours;
            var date = DateOnly.FromDateTime(startTime);
            var pricing = pricingCalendar.FirstOrDefault(p => p.Date == date);
            total += (pricing?.HourlyPrice ?? listing.DefaultHourlyPrice ?? 0) * hours;
        }

        return total;
    }

    public async Task BulkUpdatePricingAsync(int listingId, Dictionary<DateOnly, (decimal? dailyPrice, decimal? hourlyPrice)> priceUpdates)
    {
        var existingPricing = await _pricingRepository.GetByListingAndDateRangeAsync(
            listingId,
            priceUpdates.Keys.Min(),
            priceUpdates.Keys.Max()
        );

        foreach (var update in priceUpdates)
        {
            var existing = existingPricing.FirstOrDefault(p => p.Date == update.Key);
            if (existing != null)
            {
                existing.Price = update.Value.dailyPrice ?? existing.Price;
                existing.HourlyPrice = update.Value.hourlyPrice ?? existing.HourlyPrice;
                await _pricingRepository.UpdateAsync(existing);
            }
            else
            {
                var newPricing = new DailyPricing
                {
                    ListingId = listingId,
                    Date = update.Key,
                    Price = update.Value.dailyPrice ?? 0,
                    HourlyPrice = update.Value.hourlyPrice,
                    IsAvailable = true
                };
                await _pricingRepository.AddAsync(newPricing);
            }
        }
    }
}
```

---

## 6. Multi-Language Support (JSONB Architecture)

### 6.1 Database Configuration

```csharp
public class ApplicationDbContext : DbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // JSONB configuration for multi-language fields
        modelBuilder.Entity<Listing>()
            .Property(l => l.Title)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions)null)
            );

        modelBuilder.Entity<Listing>()
            .Property(l => l.Description)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions)null)
            );

        // GIN index for JSONB queries
        modelBuilder.Entity<Listing>()
            .HasIndex(l => l.Title)
            .HasMethod("gin")
            .HasOperators("jsonb_path_ops");
    }
}
```

### 6.2 Language Extraction Middleware

```csharp
public class LanguageMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        var acceptLanguage = context.Request.Headers["Accept-Language"].FirstOrDefault();
        var defaultLanguage = "tr"; // Türkçe default

        // Extract preferred language
        var userLanguage = ExtractLanguage(acceptLanguage, defaultLanguage);
        context.Items["Language"] = userLanguage;

        await _next(context);
    }

    private string ExtractLanguage(string? acceptLanguage, string defaultLanguage)
    {
        if (string.IsNullOrEmpty(acceptLanguage))
            return defaultLanguage;

        // Parse Accept-Language header
        var languages = acceptLanguage.Split(',')
            .Select(l =>
            {
                var parts = l.Split(';');
                var lang = parts[0].Trim();
                var quality = parts.Length > 1 && parts[1].StartsWith("q=")
                    ? float.Parse(parts[1].Substring(2))
                    : 1.0f;
                return (lang, quality);
            })
            .OrderByDescending(x => x.quality)
            .Select(x => x.lang)
            .ToList();

        // Return first supported language
        var supportedLanguages = new[] { "tr", "en", "de", "ru" };
        return languages.FirstOrDefault(l => supportedLanguages.Contains(l)) ?? defaultLanguage;
    }
}
```

### 6.3 Multi-Language Content Service

```csharp
public interface IMultiLanguageService
{
    T GetLocalizedContent<T>(T multiLanguageContent, string languageCode);
    T EnsureAllLanguages<T>(T multiLanguageContent, string defaultLanguage = "tr");
}

public class MultiLanguageService : IMultiLanguageService
{
    public T GetLocalizedContent<T>(T multiLanguageContent, string languageCode)
    {
        if (multiLanguageContent is Dictionary<string, string> dictionary)
        {
            if (dictionary.TryGetValue(languageCode, out var localizedValue))
            {
                return (T)(object)localizedValue;
            }
            // Fallback to Turkish (default)
            if (dictionary.TryGetValue("tr", out var fallbackValue))
            {
                return (T)(object)fallbackValue;
            }
        }
        return multiLanguageContent;
    }

    public T EnsureAllLanguages<T>(T multiLanguageContent, string defaultLanguage = "tr")
    {
        if (multiLanguageContent is Dictionary<string, string> dictionary)
        {
            var requiredLanguages = new[] { "tr", "en", "de", "ru" };
            var defaultContent = dictionary.GetValueOrDefault(defaultLanguage);

            foreach (var lang in requiredLanguages.Where(l => !dictionary.ContainsKey(l)))
            {
                dictionary[lang] = defaultContent ?? "";
            }
        }
        return multiLanguageContent;
    }
}
```

---

## 7. AI-Powered Translation System (Hangfire + OpenAI GPT-4o-mini)

### 7.1 Hangfire Job Definition

```csharp
public class TranslationJobs
{
    private readonly IOpenAIService _openAIService;
    private readonly ITranslationRepository _translationRepository;
    private readonly ITranslationCacheRepository _cacheRepository;
    private readonly ILogger<TranslationJobs> _logger;

    [Queue("translations")]
    [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 60, 300, 900 })]
    public async Task ExecuteTranslationTask(int translationTaskId)
    {
        var task = await _translationRepository.GetByIdAsync(translationTaskId);
        if (task == null || task.Status != TranslationTaskStatus.Pending)
            return;

        task.Status = TranslationTaskStatus.Processing;
        task.StartedAt = DateTime.UtcNow;
        await _translationRepository.UpdateAsync(task);

        try
        {
            var results = new Dictionary<string, string>();

            foreach (var targetLang in task.TargetLanguages)
            {
                foreach (var (fieldName, sourceText) in task.SourceFields)
                {
                    // Check cache first
                    var cacheKey = HashHelper.ComputeSHA256(sourceText);
                    var cached = await _cacheRepository.GetAsync(cacheKey, task.SourceLanguage, targetLang);

                    if (cached != null)
                    {
                        results[$"{targetLang}_{fieldName}"] = cached.TranslatedText;
                        continue;
                    }

                    // Call OpenAI GPT-4o-mini
                    var translated = await _openAIService.TranslateTextAsync(
                        sourceText,
                        task.SourceLanguage,
                        targetLang
                    );

                    results[$"{targetLang}_{fieldName}"] = translated;

                    // Save to cache
                    await _cacheRepository.AddAsync(new TranslationCache
                    {
                        SourceTextHash = cacheKey,
                        SourceLanguage = task.SourceLanguage,
                        TargetLanguage = targetLang,
                        SourceText = sourceText,
                        TranslatedText = translated,
                        AIProvider = "openai-gpt-4o-mini",
                        ExpiresAt = DateTime.UtcNow.AddDays(30)
                    });
                }
            }

            task.TranslationResults = results;
            task.Status = TranslationTaskStatus.Completed;
            task.CompletedAt = DateTime.UtcNow;
            await _translationRepository.UpdateAsync(task);

            // Update entity with translations
            await ApplyTranslationsToEntity(task);
        }
        catch (Exception ex)
        {
            task.Status = TranslationTaskStatus.Failed;
            task.LastError = ex.Message;
            task.RetryCount++;
            task.NextRetryAt = DateTime.UtcNow.AddMinutes(5 * (int)Math.Pow(2, task.RetryCount));
            await _translationRepository.UpdateAsync(task);

            _logger.LogError(ex, "Translation task failed: {TaskId}", translationTaskId);
            throw; // Re-throw to trigger Hangfire retry
        }
    }

    private async Task ApplyTranslationsToEntity(TranslationTask task)
    {
        switch (task.EntityType.ToLower())
        {
            case "listing":
                var listing = await _listingRepository.GetByIdAsync(task.EntityId);
                if (listing != null)
                {
                    foreach (var result in task.TranslationResults)
                    {
                        var parts = result.Key.Split('_');
                        var lang = parts[0];
                        var field = string.Join('_', parts.Skip(1));

                        switch (field.ToLower())
                        {
                            case "title":
                                if (!listing.Title.ContainsKey(lang))
                                    listing.Title[lang] = result.Value;
                                break;
                            case "description":
                                if (!listing.Description.ContainsKey(lang))
                                    listing.Description[lang] = result.Value;
                                break;
                            case "address":
                                if (!listing.Address.ContainsKey(lang))
                                    listing.Address[lang] = result.Value;
                                break;
                        }
                    }
                    await _listingRepository.UpdateAsync(listing);
                }
                break;

            // Handle other entity types (Review, BlogPost, etc.)
        }
    }
}
```

### 7.2 OpenAI Service Integration

```csharp
public interface IOpenAIService
{
    Task<string> TranslateTextAsync(string text, string sourceLanguage, string targetLanguage);
    Task<Dictionary<string, string>> TranslateBatchAsync(Dictionary<string, string> texts, string sourceLanguage, string targetLanguage);
}

public class OpenAIService : IOpenAIService
{
    private readonly OpenAIClient _client;
    private readonly ILogger<OpenAIService> _logger;

    public OpenAIService(IAzureOpenAIConfiguration config, ILogger<OpenAIService> logger)
    {
        _client = new OpenAIClient(config.ApiKey);
        _logger = logger;
    }

    public async Task<string> TranslateTextAsync(string text, string sourceLanguage, string targetLanguage)
    {
        var prompt = $@"Translate the following text from {GetLanguageName(sourceLanguage)} to {GetLanguageName(targetLanguage)}.
Only return the translated text without any additional explanation or formatting.

Text: {text}";

        var chatCompletionsOptions = new ChatCompletionsOptions
        {
            DeploymentName = "gpt-4o-mini", // Using gpt-4o-mini for cost-efficiency
            Messages =
            {
                new ChatRequestUserMessage(prompt)
            },
            Temperature = 0.3f, // Low temperature for more consistent translations
            MaxTokens = 2000
        };

        var response = await _client.GetChatCompletionsAsync(chatCompletionsOptions);
        return response.Value.Choices[0].Message.Content;
    }

    public async Task<Dictionary<string, string>> TranslateBatchAsync(Dictionary<string, string> texts, string sourceLanguage, string targetLanguage)
    {
        var results = new Dictionary<string, string>();

        var prompt = $@"Translate the following texts from {GetLanguageName(sourceLanguage)} to {GetLanguageName(targetLanguage)}.
For each text, return the translation in the following JSON format:
{{
    ""key1"": ""translation1"",
    ""key2"": ""translation2""
}}

Texts to translate:
{JsonSerializer.Serialize(texts)}";

        var chatCompletionsOptions = new ChatCompletionsOptions
        {
            DeploymentName = "gpt-4o-mini",
            Messages =
            {
                new ChatRequestUserMessage(prompt)
            },
            Temperature = 0.3f,
            ResponseFormat = new ChatCompletionsResponseFormat { Type = "json_object" },
            MaxTokens = 4000
        };

        var response = await _client.GetChatCompletionsAsync(chatCompletionsOptions);
        var jsonContent = response.Value.Choices[0].Message.Content;
        results = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonContent);

        return results;
    }

    private string GetLanguageName(string code)
    {
        return code switch
        {
            "tr" => "Turkish",
            "en" => "English",
            "de" => "German",
            "ru" => "Russian",
            _ => code
        };
    }
}
```

### 7.3 Translation Task Trigger

```csharp
public interface ITranslationTriggerService
{
    Task TriggerTranslationAsync(string entityType, int entityId, Dictionary<string, string> sourceFields, string sourceLanguage = "tr");
    Task TriggerBulkTranslationAsync<T>(IEnumerable<T> entities, string entityType) where T : class;
}

public class TranslationTriggerService : ITranslationTriggerService
{
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly ITranslationRepository _translationRepository;
    private readonly ISupportedLanguageRepository _languageRepository;

    public async Task TriggerTranslationAsync(string entityType, int entityId, Dictionary<string, string> sourceFields, string sourceLanguage = "tr")
    {
        var targetLanguages = await _languageRepository.GetActiveLanguagesAsync(sourceLanguage);

        var task = new TranslationTask
        {
            EntityType = entityType,
            EntityId = entityId,
            SourceLanguage = sourceLanguage,
            TargetLanguages = targetLanguages.Select(l => l.Code).ToArray(),
            SourceFields = sourceFields,
            Status = TranslationTaskStatus.Pending,
            AIProvider = "openai-gpt-4o-mini",
            CreatedAt = DateTime.UtcNow
        };

        await _translationRepository.AddAsync(task);

        // Enqueue to Hangfire with priority
        _backgroundJobClient.Create<TranslationJobs>(
            job => job.ExecuteTranslationTask(task.Id),
            new EnqueuedState("translations")
        );
    }

    public async Task TriggerBulkTranslationAsync<T>(IEnumerable<T> entities, string entityType) where T : class
    {
        foreach (var entity in entities)
        {
            var id = (int)entity.GetType().GetProperty("Id").GetValue(entity);
            var sourceFields = ExtractMultiLanguageFields(entity);
            await TriggerTranslationAsync(entityType, id, sourceFields);
        }
    }
}
```

---

## 8. Email System (FluentEmail)

### 8.1 Email Service Configuration

```csharp
public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string htmlContent, CancellationToken cancellationToken = default);
    Task SendWelcomeEmailAsync(User user, CancellationToken cancellationToken = default);
    Task SendReservationConfirmationEmailAsync(Reservation reservation, CancellationToken cancellationToken = default);
    Task SendPasswordResetEmailAsync(User user, string resetToken, CancellationToken cancellationToken = default);
    Task SendTranslationCompleteEmailAsync(User user, TranslationTask task, CancellationToken cancellationToken = default);
}

public class EmailService : IEmailService
{
    private readonly IFluentEmail _fluentEmail;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IFluentEmailFactory fluentEmailFactory, ILogger<EmailService> logger)
    {
        _fluentEmail = fluentEmailFactory.Create();
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string htmlContent, CancellationToken cancellationToken = default)
    {
        var email = _fluentEmail
            .To(to)
            .Subject(subject)
            .Body(htmlContent, isHtml: true);

        var response = await email.SendAsync(cancellationToken);
        if (!response.Successful)
        {
            _logger.LogError("Failed to send email to {To}. Error: {Error}", to, response.ErrorMessages.FirstOrDefault());
            throw new Exception($"Failed to send email: {response.ErrorMessages.FirstOrDefault()}");
        }
    }

    public async Task SendWelcomeEmailAsync(User user, CancellationToken cancellationToken = default)
    {
        var template = await LoadTemplateAsync("WelcomeEmail.html");
        var content = template
            .Replace("{{UserName}}", user.Email)
            .Replace("{{ConfirmEmailUrl}}", $"https://swimago.com/confirm-email?token={Guid.NewGuid()}");

        await SendEmailAsync(user.Email, "Welcome to Swimago!", content, cancellationToken);
    }

    public async Task SendReservationConfirmationEmailAsync(Reservation reservation, CancellationToken cancellationToken = default)
    {
        var template = await LoadTemplateAsync("ReservationConfirmation.html");
        var content = template
            .Replace("{{UserName}}", reservation.Guest.Email)
            .Replace("{{ListingTitle}}", GetLocalizedText(reservation.Listing.Title))
            .Replace("{{StartDate}}", reservation.StartTime.ToString("dd MMM yyyy HH:mm"))
            .Replace("{{EndDate}}", reservation.EndTime.ToString("dd MMM yyyy HH:mm"))
            .Replace("{{TotalPrice}}", $"{reservation.FinalPrice:C}")
            .Replace("{{ReservationId}}", reservation.Id.ToString());

        await SendEmailAsync(reservation.Guest.Email, "Reservation Confirmed", content, cancellationToken);
    }

    private async Task<string> LoadTemplateAsync(string templateName)
    {
        var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "EmailTemplates", templateName);
        return await File.ReadAllTextAsync(templatePath);
    }

    private string GetLocalizedText(Dictionary<string, string> multiLanguageText)
    {
        // Return English text or fallback to Turkish
        return multiLanguageText.GetValueOrDefault("en") ?? multiLanguageText.GetValueOrDefault("tr") ?? "";
    }
}
```

### 8.2 FluentEmail Setup (Program.cs)

```csharp
// Program.cs
builder.Services.AddFluentEmail(builder.Configuration["Email:DefaultFrom"])
    .AddRazorRenderer()
    // Email provider will be determined later - placeholder for now
    // Example for SMTP (can be replaced later):
    // .AddSmtpSender(builder.Configuration["Email:Host"], int.Parse(builder.Configuration["Email:Port"]))
    // Example for SendGrid:
    // .AddSendGridSender(builder.Configuration["Email:SendGridApiKey"])
    // Example for Mailgun:
    // .AddMailgunSender(builder.Configuration["Email:MailgunApiKey"], builder.Configuration["Email:MailgunDomain"]);
```

---

## 9. Search & Filtering (PostGIS)

### 9.1 Geospatial Search Service

```csharp
public interface ISearchService
{
    Task<PagedResult<Listing>> SearchListingsAsync(SearchQuery query, CancellationToken cancellationToken = default);
    Task<List<Listing>> GetNearbyListingsAsync(decimal latitude, decimal longitude, decimal radiusKm, ListingType? type = null, CancellationToken cancellationToken = default);
    Task<List<string>> GetAutocompleteSuggestionsAsync(string query, string languageCode = "tr", CancellationToken cancellationToken = default);
}

public class SearchService : ISearchService
{
    private readonly ApplicationDbContext _context;

    public async Task<PagedResult<Listing>> SearchListingsAsync(SearchQuery query, CancellationToken cancellationToken = default)
    {
        var listings = _context.Listings
            .Include(l => l.Images)
            .Include(l => l.Amenities)
            .Where(l => l.IsActive);

        // Filter by type
        if (query.Type.HasValue)
        {
            listings = listings.Where(l => l.Type == query.Type.Value);
        }

        // Geospatial search (PostGIS)
        if (query.Latitude.HasValue && query.Longitude.HasValue && query.RadiusKm.HasValue)
        {
            listings = listings.Where(l =>
                l.Location.Distance(new GeographyPoint(query.Latitude.Value, query.Longitude.Value)) <= query.RadiusKm.Value * 1000 // Convert km to meters
            );
        }

        // Price range filtering
        if (query.MinPrice.HasValue)
        {
            listings = listings.Where(l =>
                l.PricingCalendar.Any(p =>
                    p.Date >= query.StartDate &&
                    p.Date <= query.EndDate &&
                    p.Price >= query.MinPrice.Value &&
                    (!query.MaxPrice.HasValue || p.Price <= query.MaxPrice.Value)
                )
            );
        }

        // Full-text search (multi-language)
        if (!string.IsNullOrEmpty(query.SearchText))
        {
            listings = listings.Where(l =>
                l.Title[query.LanguageCode ?? "tr"].Contains(query.SearchText) ||
                l.Description[query.LanguageCode ?? "tr"].Contains(query.SearchText) ||
                l.Address[query.LanguageCode ?? "tr"].Contains(query.SearchText)
            );
        }

        // Guest count filter
        if (query.GuestCount.HasValue)
        {
            listings = listings.Where(l => l.MaxGuestCount >= query.GuestCount.Value);
        }

        // Rating filter
        if (query.MinRating.HasValue)
        {
            listings = listings.Where(l => l.Rating >= query.MinRating.Value);
        }

        // Sorting
        listings = query.SortBy switch
        {
            "price_asc" => listings.OrderBy(l => l.PricingCalendar.Min(p => p.Price)),
            "price_desc" => listings.OrderByDescending(l => l.PricingCalendar.Max(p => p.Price)),
            "rating" => listings.OrderByDescending(l => l.Rating),
            "newest" => listings.OrderByDescending(l => l.CreatedAt),
            _ => listings.OrderBy(l => l.Rating) // Default: by rating
        };

        var totalCount = await listings.CountAsync(cancellationToken);
        var results = await listings
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Listing>(results, totalCount, query.Page, query.PageSize);
    }

    public async Task<List<Listing>> GetNearbyListingsAsync(decimal latitude, decimal longitude, decimal radiusKm, ListingType? type = null, CancellationToken cancellationToken = default)
    {
        var point = new GeographyPoint(latitude, longitude);

        var query = _context.Listings
            .Include(l => l.Images)
            .Where(l => l.IsActive)
            .Where(l => l.Location.Distance(point) <= radiusKm * 1000);

        if (type.HasValue)
        {
            query = query.Where(l => l.Type == type.Value);
        }

        return await query.OrderBy(l => l.Location.Distance(point))
            .Take(20)
            .ToListAsync(cancellationToken);
    }
}

public record SearchQuery
{
    public string? SearchText { get; init; }
    public ListingType? Type { get; init; }
    public decimal? Latitude { get; init; }
    public decimal? Longitude { get; init; }
    public decimal? RadiusKm { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    public int? GuestCount { get; init; }
    public decimal? MinRating { get; init; }
    public string? LanguageCode { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string SortBy { get; init; } = "rating";
}
```

---

## 10. Real-time Features (SignalR)

### 10.1 SignalR Hub

```csharp
public interface IBookingHub
{
    Task ReservationCreated(int reservationId);
    Task ReservationStatusChanged(int reservationId, ReservationStatus status);
    Task NewNotification(int userId);
}

public class BookingHub : Hub<IBookingHub>
{
    private readonly IUserConnectionManager _connectionManager;

    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        if (userId != null && int.TryParse(userId, out int parsedUserId))
        {
            await _connectionManager.AddConnectionAsync(parsedUserId, Context.ConnectionId);
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;
        if (userId != null && int.TryParse(userId, out int parsedUserId))
        {
            await _connectionManager.RemoveConnectionAsync(parsedUserId, Context.ConnectionId);
        }
        await base.OnDisconnectedAsync(exception);
    }
}

public class UserConnectionManager
{
    private readonly ConcurrentDictionary<int, HashSet<string>> _userConnections = new();

    public Task AddConnectionAsync(int userId, string connectionId)
    {
        _userConnections.AddOrUpdate(userId,
            new HashSet<string> { connectionId },
            (_, existing) =>
            {
                existing.Add(connectionId);
                return existing;
            });
        return Task.CompletedTask;
    }

    public Task RemoveConnectionAsync(int userId, string connectionId)
    {
        if (_userConnections.TryGetValue(userId, out var connections))
        {
            connections.Remove(connectionId);
            if (connections.Count == 0)
            {
                _userConnections.TryRemove(userId, out _);
            }
        }
        return Task.CompletedTask;
    }

    public IEnumerable<string> GetConnections(int userId)
    {
        return _userConnections.GetValueOrDefault(userId) ?? Enumerable.Empty<string>();
    }
}
```

---

## 11. Security Best Practices

### 11.1 Cloudflare WAF Configuration

```yaml
# Cloudflare WAF Rules (Example)
rules:
  - name: "Block SQL Injection"
    expression: "http.request.uri contains \"'\" or http.request.uri contains \"--\""
    action: block

  - name: "Rate Limiting - API"
    expression: "http.request.uri.path matches \"^/api/\""
    action:
      type: rate_limit
      rate_limit:
        requests_per_period: 100
        period: 60

  - name: "Hotlink Protection"
    expression: "http.referer ne \"swimago.com\" and http.request.uri.path matches \"\\.(jpg|png|gif|webp)$\""
    action: block
```

### 11.2 API Security

```csharp
// Rate Limiting Middleware
public class RateLimitMiddleware
{
    public async Task InvokeAsync(HttpContext context, IRateLimitStore rateLimitStore)
    {
        var clientId = context.Connection.RemoteIpAddress?.ToString();
        var endpoint = context.Request.Path;

        var key = $"{clientId}:{endpoint}";
        var count = await rateLimitStore.GetAsync(key);

        if (count >= 100) // 100 requests per minute
        {
            context.Response.StatusCode = 429;
            return;
        }

        await rateLimitStore.IncrementAsync(key, TimeSpan.FromMinutes(1));
        await _next(context);
    }
}

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        builder => builder
            .WithOrigins("https://swimago.com", "https://www.swimago.com")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

// Secure Headers
public class SecurityHeadersMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Add("X-Frame-Options", "DENY");
        context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
        context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
        context.Response.Headers.Add("Content-Security-Policy", "default-src 'self'; img-src 'self' data: https://*.cloudflare.com; script-src 'self' 'unsafe-inline'");

        await _next(context);
    }
}
```

---

## 12. Hangfire Dashboard & Background Job Monitoring

### 12.1 Hangfire Configuration

```csharp
// Program.cs
builder.Services.AddHangfire(config => config
    .UsePostgreSqlStorage(connectionString, new PostgreSqlStorageOptions
    {
        QueuePollInterval = TimeSpan.FromSeconds(15)
    })
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings());

builder.Services.AddHangfireServer(options =>
{
    options.WorkerCount = Environment.ProcessorCount * 2; // Adjust based on needs
    options.Queues = new[] { "translations", "emails", "default", "high-priority" };
});

// Enable Hangfire Dashboard (Admin only)
app.UseHangfireDashboard("/admin/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() }
});
```

---

## 13. Deployment & CI/CD

### 13.1 Docker Configuration

```dockerfile
# Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/Swimago.API/Swimago.API.csproj", "src/Swimago.API/"]
RUN dotnet restore "src/Swimago.API/Swimago.API.csproj"
COPY . .
WORKDIR "/src/src/Swimago.API"
RUN dotnet build "Swimago.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Swimago.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Swimago.API.dll"]
```

### 13.2 GitHub Actions Workflow

```yaml
# .github/workflows/deploy.yml
name: Deploy to Production

on:
  push:
    branches: [main]

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3

      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: 9.0.x

      - name: Build
        run: dotnet build --configuration Release

      - name: Run Tests
        run: dotnet test --configuration Release

      - name: Build Docker Image
        run: docker build -t swimago-api:${{ github.sha }} .

      - name: Push to Registry
        run: |
          echo ${{ secrets.REGISTRY_PASSWORD }} | docker login -u ${{ secrets.REGISTRY_USERNAME }} --password-stdin
          docker push swimago-api:${{ github.sha }}

      - name: Deploy to Server
        run: |
          # SSH into server and deploy
          ssh ${{ secrets.DEPLOY_USER }}@${{ secrets.DEPLOY_HOST }} "docker pull swimago-api:${{ github.sha }} && docker-compose up -d"
```

---

## 14. Configuration Management

### 14.1 appsettings.json Structure

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=your-supabase-host;Database=swimago;Username=postgres;Password=your-password"
  },
  "Jwt": {
    "SecretKey": "your-256-bit-secret-key",
    "Issuer": "swimago.com",
    "Audience": "swimago-api",
    "ExpiryMinutes": 60,
    "RefreshTokenExpiryDays": 30
  },
  "Cloudflare": {
    "R2": {
      "AccessKeyId": "your-access-key-id",
      "SecretAccessKey": "your-secret-access-key",
      "BucketName": "swimago-images",
      "Endpoint": "https://your-account-id.r2.cloudflarestorage.com",
      "CustomDomain": "cdn.swimago.com"
    },
    "CDN": {
      "ZoneId": "your-zone-id",
      "AccountId": "your-account-id",
      "ApiToken": "your-api-token"
    }
  },
  "OpenAI": {
    "ApiKey": "your-openai-api-key",
    "DeploymentName": "gpt-4o-mini",
    "MaxRetries": 3,
    "TimeoutSeconds": 30
  },
  "Email": {
    "DefaultFrom": "noreply@swimago.com",
    "Provider": "sendgrid", // To be determined
    "SendGridApiKey": "your-sendgrid-key",
    "SmtpHost": "smtp.example.com",
    "SmtpPort": 587,
    "SmtpUsername": "username",
    "SmtpPassword": "password"
  },
  "SupportedLanguages": {
    "Default": "tr",
    "Available": ["tr", "en", "de", "ru"]
  },
  "Hangfire": {
    "DashboardEnabled": true,
    "WorkerCount": 4
  },
  "RateLimiting": {
    "RequestsPerMinute": 100,
    "BurstLimit": 200
  }
}
```

---

## 15. Testing Strategy

### 15.1 Unit Tests

```csharp
// PricingService.Tests.cs
public class PricingServiceTests
{
    [Fact]
    public async Task CalculateTotalPrice_DailyBooking_ReturnsCorrectTotal()
    {
        // Arrange
        var service = new PricingService(/* dependencies */);
        var startDate = new DateTime(2026, 1, 1);
        var endDate = new DateTime(2026, 1, 3); // 2 nights

        // Act
        var result = await service.CalculateTotalPriceAsync(1, startDate, endDate, BookingType.Daily);

        // Assert
        Assert.Equal(400, result); // Assuming 200 per night
    }
}
```

### 15.2 Integration Tests

```csharp
// ListingsController.IntegrationTests.cs
public class ListingsControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ListingsControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetListings_ReturnsOkResponse()
    {
        // Act
        var response = await _client.GetAsync("/api/listings");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
```

---

## 16. AI Prompt Templates for Development

### 16.1 Entity Creation Prompt

```
Create a C# entity class for {EntityName} with the following requirements:
- Must include JSONB multi-language fields for {Field1}, {Field2}
- Include all necessary EF Core configurations
- Add GIN index for JSONB fields
- Include PostGIS support if geospatial data is needed
- Follow the existing codebase patterns from the provided examples
```

### 16.2 Translation Job Generation Prompt

```
Generate a Hangfire background job class for translating {EntityType} entities:
- Use OpenAI GPT-4o-mini for translations
- Implement automatic retry logic with exponential backoff
- Include translation caching mechanism
- Support batch translation
- Follow the existing TranslationJobs class pattern
```

### 16.3 API Endpoint Generation Prompt

```
Create a REST API controller for {EntityName} with these endpoints:
- GET /api/{entityName} (with pagination, filtering, and sorting)
- GET /api/{entityName}/{id}
- POST /api/{entityName} (with multi-language validation)
- PUT /api/{entityName}/{id}
- DELETE /api/{entityName}/{id}

Requirements:
- Include proper authorization checks
- Support multi-language content extraction
- Include DTOs for request/response
- Add proper error handling
- Return HTTP status codes appropriately
```

---

## Summary

This comprehensive backend development guide covers:

✅ **Technology Stack**: .NET 9 + Supabase/PostgreSQL + Cloudflare R2 + Hangfire + OpenAI GPT-4o-mini
✅ **Multi-Language Support**: JSONB-based multi-language architecture with AI-powered translations
✅ **Dynamic Pricing**: Flexible pricing calendar allowing hosts to set prices per date/hour
✅ **File Storage**: Cloudflare R2 with CDN optimization
✅ **Background Jobs**: Hangfire for translation, email, and other async tasks
✅ **Geospatial Search**: PostGIS integration for location-based queries
✅ **Security**: Cloudflare WAF, JWT auth, RBAC, rate limiting
✅ **Email System**: FluentEmail with pluggable provider architecture
✅ **Real-time Features**: SignalR for live updates
✅ **Testing**: Unit and integration test strategies
✅ **Deployment**: Docker + GitHub Actions + Cloudflare configuration

**Target Languages**: Turkish (Default), English, German, Russian
**AI Translation Provider**: OpenAI GPT-4o-mini
**Email Provider**: To be determined (FluentEmail abstraction ready)
**Payment Provider**: To be determined (Final stage)

