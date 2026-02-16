# Swimago API - Implementation Plan

Kapsamlı bir .NET Core 9 Web API projesi geliştireceğiz. Proje, plajlar, havuzlar ve tekne turları için rezervasyon sistemi sağlayacak ve Clean Architecture prensipleriyle yapılandırılacak.

## User Review Required

> [!IMPORTANT]
> **Clean Architecture Yapısı**: Proje 4 katmanlı bir yapıda organize edilecek (Domain, Application, Infrastructure, API). Bu yaklaşım kodun test edilebilirliğini, bakımını ve genişletilebilirliğini kolaylaştırır.

> [!IMPORTANT]
> **Multi-Language Desteği**: PostgreSQL JSONB kullanarak çoklu dil desteği sağlanacak. İçerikler Türkçe girilecek ve OpenAI GPT-4o-mini ile otomatik olarak İngilizce, Almanca ve Rusça'ya çevrilecek.

> [!WARNING]
> **Ödeme Sağlayıcısı**: Dokümanda belirtildiği üzere ödeme entegrasyonu son aşamaya bırakılacak. Şimdilik sadece altyapı hazırlanacak.

> [!WARNING]
> **Email Sağlayıcısı**: FluentEmail kullanılacak ancak provider (SendGrid, Mailgun, SMTP) henüz belirlenmedi. Abstraction layer hazırlanacak.

## Proposed Changes

### Component 1: Solution Structure

Proje Clean Architecture prensiplerine uygun olarak organize edilecek.

#### [NEW] [Swimago.sln](file:///Users/kaancelik/my-projects/swimago-stich-api/Swimago.sln)
- Ana solution dosyası
- 4 proje içerecek: Domain, Application, Infrastructure, API

#### [NEW] [src/Swimago.Domain](file:///Users/kaancelik/my-projects/swimago-stich-api/src/Swimago.Domain)
- **Entities**: User, UserProfile, Listing, Reservation, Review, BlogPost, Amenity, vb.
- **Enums**: Role, ListingType, ReservationStatus, BookingType, PaymentStatus, vb.
- **Interfaces**: Core repository interface'leri
- **Value Objects**: Gerektiğinde domain value object'leri
- **Hiçbir dış bağımlılık yok** (Pure domain model)

#### [NEW] [src/Swimago.Application](file:///Users/kaancelik/my-projects/swimago-stich-api/src/Swimago.Application)
- **DTOs**: Request/Response modelleri
- **Interfaces**: Servis interface'leri (IAuthService, IListingService, vb.)
- **Services**: Business logic implementations
- **Commands/Queries**: CQRS pattern (opsiyonel, ilerleyen fazlarda)
- **Validators**: FluentValidation ile input validation
- **Bağımlılıklar**: Sadece Domain katmanı

#### [NEW] [src/Swimago.Infrastructure](file:///Users/kaancelik/my-projects/swimago-stich-api/src/Swimago.Infrastructure)
- **Data**: EF Core DbContext, Configurations, Migrations
- **Repositories**: Repository implementasyonları
- **Services**: 
  - CloudflareR2Service (File upload)
  - OpenAIService (AI translations)
  - EmailService (FluentEmail)
  - BackgroundJobs (Hangfire jobs)
- **External Integrations**: Supabase, Cloudflare, OpenAI
- **Bağımlılıklar**: Domain ve Application katmanları

#### [NEW] [src/Swimago.API](file:///Users/kaancelik/my-projects/swimago-stich-api/src/Swimago.API)
- **Controllers**: REST API endpoints
- **Middleware**: JWT, Language, RateLimit, Security Headers
- **SignalR Hubs**: Real-time communication
- **Configuration**: Dependency injection, middleware pipeline
- **Bağımlılıklar**: Application ve Infrastructure katmanları

---

### Component 2: Domain Layer (Pure Entities)

#### [NEW] [Entities/User.cs](file:///Users/kaancelik/my-projects/swimago-stich-api/src/Swimago.Domain/Entities/User.cs)
```csharp
public class User
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public Role Role { get; set; }
    public bool IsEmailVerified { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime RefreshTokenExpiry { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    
    // Navigation
    public UserProfile? Profile { get; set; }
    public ICollection<Reservation> Reservations { get; set; }
    public ICollection<Listing> Listings { get; set; }
}
```

#### [NEW] [Entities/Listing.cs](file:///Users/kaancelik/my-projects/swimago-stich-api/src/Swimago.Domain/Entities/Listing.cs)
- JSONB dictionary properties for multi-language support (Title, Description, Address)
- PostGIS GeographyPoint for location data
- Collections for Images, PricingCalendar, AvailabilityBlocks, Amenities

#### [NEW] [Entities/Reservation.cs](file:///Users/kaancelik/my-projects/swimago-stich-api/src/Swimago.Domain/Entities/Reservation.cs)
- Booking details (dates, guest count, pricing)
- Status tracking
- Payment relationship

#### [NEW] Diğer Core Entities
- `Review.cs`, `BlogPost.cs`, `Amenity.cs`, `TranslationTask.cs`, `Notification.cs`, vb.

---

### Component 3: Infrastructure Layer

#### [NEW] [Data/ApplicationDbContext.cs](file:///Users/kaancelik/my-projects/swimago-stich-api/src/Swimago.Infrastructure/Data/ApplicationDbContext.cs)
```csharp
public class ApplicationDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Listing> Listings { get; set; }
    public DbSet<Reservation> Reservations { get; set; }
    // ... diğer DbSet'ler
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // JSONB configuration
        modelBuilder.Entity<Listing>()
            .Property(l => l.Title)
            .HasColumnType("jsonb");
            
        // PostGIS configuration
        modelBuilder.HasPostgresExtension("postgis");
        
        // GIN indexes for JSONB
        modelBuilder.Entity<Listing>()
            .HasIndex(l => l.Title)
            .HasMethod("gin");
            
        // Apply all configurations
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
```

#### [NEW] [Data/Configurations](file:///Users/kaancelik/my-projects/swimago-stich-api/src/Swimago.Infrastructure/Data/Configurations)
- `UserConfiguration.cs`, `ListingConfiguration.cs`, vb.
- Fluent API ile entity konfigürasyonları

#### [NEW] [Repositories](file:///Users/kaancelik/my-projects/swimago-stich-api/src/Swimago.Infrastructure/Repositories)
- Generic Repository pattern implementation
- `UserRepository.cs`, `ListingRepository.cs`, `ReservationRepository.cs`, vb.

#### [NEW] [Services/CloudflareR2Service.cs](file:///Users/kaancelik/my-projects/swimago-stich-api/src/Swimago.Infrastructure/Services/CloudflareR2Service.cs)
- S3-compatible API kullanarak dosya yükleme
- CDN URL oluşturma
- Image optimization

#### [NEW] [Services/OpenAIService.cs](file:///Users/kaancelik/my-projects/swimago-stich-api/src/Swimago.Infrastructure/Services/OpenAIService.cs)
- GPT-4o-mini ile metin çevirisi
- Batch translation desteği
- Cost optimization

#### [NEW] [BackgroundJobs/TranslationJobs.cs](file:///Users/kaancelik/my-projects/swimago-stich-api/src/Swimago.Infrastructure/BackgroundJobs/TranslationJobs.cs)
- Hangfire job definitions
- Automatic retry logic
- Translation cache implementasyonu

---

### Component 4: Application Layer

#### [NEW] [DTOs/Auth](file:///Users/kaancelik/my-projects/swimago-stich-api/src/Swimago.Application/DTOs/Auth)
- `LoginRequest.cs`, `LoginResponse.cs`
- `RegisterRequest.cs`, `RefreshTokenRequest.cs`

#### [NEW] [DTOs/Listings](file:///Users/kaancelik/my-projects/swimago-stich-api/src/Swimago.Application/DTOs/Listings)
- `CreateListingRequest.cs`, `UpdateListingRequest.cs`
- `ListingResponse.cs`, `ListingDetailResponse.cs`

#### [NEW] [Services/AuthService.cs](file:///Users/kaancelik/my-projects/swimago-stich-api/src/Swimago.Application/Services/AuthService.cs)
- JWT token generation ve validation
- Password hashing (BCrypt)
- Refresh token yönetimi

#### [NEW] [Services/ListingService.cs](file:///Users/kaancelik/my-projects/swimago-stich-api/src/Swimago.Application/Services/ListingService.cs)
- CRUD operations
- Multi-language content yönetimi
- Translation trigger

#### [NEW] [Services/PricingService.cs](file:///Users/kaancelik/my-projects/swimago-stich-api/src/Swimago.Application/Services/PricingService.cs)
- Dynamic pricing calculation
- Bulk pricing updates
- Availability checks

#### [NEW] [Services/SearchService.cs](file:///Users/kaancelik/my-projects/swimago-stich-api/src/Swimago.Application/Services/SearchService.cs)
- PostGIS geospatial queries
- Multi-language full-text search
- Filtering ve sorting

---

### Component 5: API Layer

#### [NEW] [Controllers/AuthController.cs](file:///Users/kaancelik/my-projects/swimago-stich-api/src/Swimago.API/Controllers/AuthController.cs)
```csharp
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    [HttpPost("register")]
    [HttpPost("login")]
    [HttpPost("refresh-token")]
    [HttpPost("logout")]
    [HttpGet("me")]
    // ... diğer endpoints
}
```

#### [NEW] [Controllers/ListingsController.cs](file:///Users/kaancelik/my-projects/swimago-stich-api/src/Swimago.API/Controllers/ListingsController.cs)
- GET, POST, PUT, DELETE endpoints
- Image upload endpoints
- Pricing calendar endpoints
- Amenities management

#### [NEW] Diğer Controllers
- `ReservationsController.cs`, `ReviewsController.cs`, `BlogController.cs`
- `SearchController.cs`, `AdminController.cs`, `HostController.cs`

#### [NEW] [Middleware/JwtMiddleware.cs](file:///Users/kaancelik/my-projects/swimago-stich-api/src/Swimago.API/Middleware/JwtMiddleware.cs)
- Token validation
- User context injection

#### [NEW] [Middleware/LanguageMiddleware.cs](file:///Users/kaancelik/my-projects/swimago-stich-api/src/Swimago.API/Middleware/LanguageMiddleware.cs)
- Accept-Language header parsing
- Culture setting

#### [NEW] [Middleware/RateLimitMiddleware.cs](file:///Users/kaancelik/my-projects/swimago-stich-api/src/Swimago.API/Middleware/RateLimitMiddleware.cs)
- Per-client rate limiting
- Redis cache integration (opsiyonel)

#### [NEW] [Hubs/BookingHub.cs](file:///Users/kaancelik/my-projects/swimago-stich-api/src/Swimago.API/Hubs/BookingHub.cs)
- SignalR hub for real-time updates
- Connection management

---

### Component 6: Configuration Files

#### [NEW] [appsettings.json](file:///Users/kaancelik/my-projects/swimago-stich-api/src/Swimago.API/appsettings.json)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=swimago;Username=postgres;Password=postgres"
  },
  "Jwt": {
    "SecretKey": "your-secret-key-here-minimum-256-bits",
    "Issuer": "swimago.com",
    "Audience": "swimago-api",
    "ExpiryMinutes": 60,
    "RefreshTokenExpiryDays": 30
  },
  "Cloudflare": {
    "R2": {
      "AccessKeyId": "",
      "SecretAccessKey": "",
      "BucketName": "swimago-images",
      "Endpoint": "",
      "CustomDomain": "cdn.swimago.com"
    }
  },
  "OpenAI": {
    "ApiKey": "",
    "DeploymentName": "gpt-4o-mini"
  },
  "SupportedLanguages": {
    "Default": "tr",
    "Available": ["tr", "en", "de", "ru"]
  }
}
```

#### [NEW] [appsettings.Development.json](file:///Users/kaancelik/my-projects/swimago-stich-api/src/Swimago.API/appsettings.Development.json)
- Development-specific settings
- Local database connection

#### [NEW] [Program.cs](file:///Users/kaancelik/my-projects/swimago-stich-api/src/Swimago.API/Program.cs)
- Dependency injection configuration
- Middleware pipeline setup
- EF Core, Hangfire, SignalR, Swagger configuration

---

### Component 7: DevOps & Deployment

#### [NEW] [Dockerfile](file:///Users/kaancelik/my-projects/swimago-stich-api/Dockerfile)
- Multi-stage build
- .NET 9 runtime
- Production-ready configuration

#### [NEW] [.github/workflows/ci-cd.yml](file:///Users/kaancelik/my-projects/swimago-stich-api/.github/workflows/ci-cd.yml)
- Build and test automation
- Docker image creation
- Deployment pipeline

#### [NEW] [docker-compose.yml](file:///Users/kaancelik/my-projects/swimago-stich-api/docker-compose.yml)
- Local development setup
- PostgreSQL with PostGIS
- pgAdmin (database management)

---

### Component 8: Database Migrations

#### [NEW] Initial Migration
```bash
dotnet ef migrations add InitialCreate -p src/Swimago.Infrastructure -s src/Swimago.API
dotnet ef database update -p src/Swimago.Infrastructure -s src/Swimago.API
```

Migration içeriği:
- Tüm entities için tablolar
- JSONB columns
- PostGIS extension
- GIN indexes
- Foreign key relationships

---

## Verification Plan

### Automated Tests

#### Unit Tests
```bash
dotnet test src/Swimago.Application.Tests
dotnet test src/Swimago.Domain.Tests
```

- `AuthService` testleri (token generation, validation)
- `PricingService` testleri (calculation logic)
- `SearchService` testleri (filtering, sorting)

#### Integration Tests
```bash
dotnet test src/Swimago.API.Tests
```

- Authentication flow testi
- Listing CRUD operations
- Reservation creation flow
- Multi-language content handling

### Manual Verification

1. **Database Setup**: PostgreSQL ve PostGIS extension'ların doğru kurulduğunu kontrol et
2. **Swagger Documentation**: `https://localhost:5001/swagger` üzerinden API dokümantasyonunu incele
3. **Authentication**: Login/register flow'unu test et
4. **File Upload**: Cloudflare R2'ye dosya yükleme işlemini test et
5. **Background Jobs**: Hangfire dashboard'dan translation job'ların çalıştığını kontrol et
6. **SignalR**: Real-time bildirim sistemini test et

### Performance Testing

- **Load Testing**: k6 veya Apache Bench ile API endpoint'lerini test et
- **Database Queries**: EF Core logging ile N+1 query problemlerini kontrol et
- **CDN Performance**: Image loading sürelerini ölç

---

## Implementation Steps

### Adım 1: Proje Yapısını Oluşturma
1. Solution ve projeler oluştur
2. NuGet paketlerini yükle
3. Proje referanslarını ayarla

### Adım 2: Domain Layer
1. Core entities oluştur
2. Enums tanımla
3. Repository interface'leri tanımla

### Adım 3: Infrastructure Layer
1. DbContext ve configurations
2. Repository implementations
3. External service integrations (R2, OpenAI)
4. Hangfire setup

### Adım 4: Application Layer
1. DTOs oluştur
2. Service interface'leri tanımla
3. Service implementations
4. Validators

### Adım 5: API Layer
1. Controllers oluştur
2. Middleware'leri implement et
3. SignalR hubs
4. Program.cs configuration

### Adım 6: Database Migrations
1. İlk migration oluştur
2. Database update
3. Seed data (opsiyonel)

### Adım 7: Testing & Documentation
1. Unit testler
2. Integration testler
3. Swagger documentation
4. README dosyası

---

## Timeline Estimate

- **Adım 1-2** (Proje Yapısı + Domain): ~2 saat
- **Adım 3** (Infrastructure): ~4 saat
- **Adım 4** (Application): ~3 saat
- **Adım 5** (API): ~4 saat
- **Adım 6** (Migrations): ~1 saat
- **Adım 7** (Testing): ~3 saat

**Toplam Tahmini Süre**: ~17 saat (yaklaşık 2-3 iş günü)
