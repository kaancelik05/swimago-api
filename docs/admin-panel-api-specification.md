# Swimago Admin Panel - Backend API Spesifikasyonu

> **Teknoloji:** .NET Core 8, Supabase PostgreSQL, Entity Framework Core
> **Base URL:** `https://api.swimago.com/api/admin`
> **Auth:** Tüm endpointler `[Authorize(Roles = "Admin")]` ile korunmalıdır.
> **Dil Desteği:** tr, en, de, ru (JSONB olarak saklanır)

---

## İçindekiler

1. [Ortak Modeller (Shared DTOs)](#1-ortak-modeller)
2. [Destinations API](#2-destinations-api)
3. [Beaches API](#3-beaches-api)
4. [Pools API](#4-pools-api)
5. [Yacht Tours API](#5-yacht-tours-api)
6. [Day Trips API](#6-day-trips-api)
7. [Blogs API](#7-blogs-api)
8. [Ortak Sayfalama & Filtreleme](#8-ortak-sayfalama--filtreleme)
9. [Media Upload API](#9-media-upload-api)

---

## 1. Ortak Modeller

Aşağıdaki modeller birden fazla entity tarafından kullanılır. Bunları `Shared/DTOs` veya `Common` namespace altında tanımlayın.

### MultiLanguageDto

```csharp
public class MultiLanguageDto
{
    public string? Tr { get; set; }
    public string? En { get; set; }
    public string? De { get; set; }
    public string? Ru { get; set; }
}
```

> **Veritabanı:** PostgreSQL JSONB column olarak saklanır. EF Core'da `HasColumnType("jsonb")` kullanılır.

### ImageDto

```csharp
public class ImageDto
{
    public Guid? Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? Alt { get; set; }
    public MultiLanguageDto? Caption { get; set; }
    public int Order { get; set; }
    public bool IsPrimary { get; set; }
}
```

### AmenityDto

```csharp
public class AmenityDto
{
    public Guid? Id { get; set; }
    public string Icon { get; set; } = string.Empty;    // Material icon adı (ör: "beach_access")
    public MultiLanguageDto Label { get; set; } = new();
    public bool Available { get; set; }
}
```

### BookingBreakdownItemDto

```csharp
public class BookingBreakdownItemDto
{
    public string Label { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
```

### BreadcrumbItemDto

```csharp
public class BreadcrumbItemDto
{
    public string Label { get; set; } = string.Empty;
    public string? Link { get; set; }
}
```

### Enum'lar

```csharp
public enum ListingStatus
{
    Pending,
    Active,
    Inactive,
    Rejected
}

public enum VenueType
{
    Beach,
    Pool,
    Yacht,
    DayTrip
}
```

### PaginatedResponse<T> (Ortak Sayfalama Cevabı)

```csharp
public class PaginatedResponse<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}
```

---

## 2. Destinations API

**Controller:** `AdminDestinationsController`
**Route Prefix:** `/api/admin/destinations`

### Endpointler

| Method | Endpoint | Açıklama |
|--------|----------|----------|
| GET | `/` | Tüm destinasyonları listele (sayfalı) |
| GET | `/{id}` | Tek destinasyon detay (form doldurma için) |
| POST | `/` | Yeni destinasyon oluştur |
| PUT | `/{id}` | Destinasyon güncelle |
| DELETE | `/{id}` | Destinasyon sil |

### GET `/` — Liste Response

```csharp
public class DestinationListItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public int SpotCount { get; set; }           // İlişkili beach + pool sayısı
    public double? AverageRating { get; set; }
    public bool IsFeatured { get; set; }
}
```

**Query Parametreleri:**

| Parametre | Tip | Varsayılan | Açıklama |
|-----------|-----|------------|----------|
| page | int | 1 | Sayfa numarası |
| pageSize | int | 10 | Sayfa boyutu |
| search | string? | null | İsme göre arama |
| country | string? | null | Ülkeye göre filtre |
| isFeatured | bool? | null | Öne çıkan filtresi |

**Response:** `PaginatedResponse<DestinationListItemDto>`

### GET `/{id}` — Detay Response (Form için)

```csharp
public class DestinationDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? MapImageUrl { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? AvgWaterTemp { get; set; }
    public int? SunnyDaysPerYear { get; set; }
    public List<string> Tags { get; set; } = new();
    public bool IsFeatured { get; set; }
    public List<DestinationFeatureDto> Features { get; set; } = new();
}

public class DestinationFeatureDto
{
    public string Icon { get; set; } = string.Empty;    // Material icon adı
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
```

### POST `/` — Create Request

```csharp
public class CreateDestinationRequest
{
    [Required] public string Name { get; set; } = string.Empty;
    [Required] public string Slug { get; set; } = string.Empty;
    [Required] public string Country { get; set; } = string.Empty;
    [Required] public string Description { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    [Required] public string ImageUrl { get; set; } = string.Empty;
    public string? MapImageUrl { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? AvgWaterTemp { get; set; }
    public int? SunnyDaysPerYear { get; set; }
    public List<string> Tags { get; set; } = new();
    public bool IsFeatured { get; set; }
    public List<DestinationFeatureDto> Features { get; set; } = new();
}
```

**Response:** `201 Created` → `DestinationDetailDto`

### PUT `/{id}` — Update Request

Aynı `CreateDestinationRequest` modeli kullanılır (veya `UpdateDestinationRequest` olarak kopyalanır).

**Response:** `200 OK` → `DestinationDetailDto`

### DELETE `/{id}`

**Response:** `204 No Content`

---

## 3. Beaches API

**Controller:** `AdminBeachesController`
**Route Prefix:** `/api/admin/beaches`

### Endpointler

| Method | Endpoint | Açıklama |
|--------|----------|----------|
| GET | `/` | Plajları listele (sayfalı) |
| GET | `/{id}` | Plaj detay (form için) |
| POST | `/` | Yeni plaj oluştur |
| PUT | `/{id}` | Plaj güncelle |
| DELETE | `/{id}` | Plaj sil |

### GET `/` — Liste Response

```csharp
public class BeachListItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;   // Aktif dilin name değeri
    public string Slug { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty; // İlk primary image URL
    public decimal PricePerDay { get; set; }
    public string Currency { get; set; } = "USD";
    public double Rating { get; set; }
    public int ReviewCount { get; set; }
    public bool IsActive { get; set; }
}
```

**Query Parametreleri:**

| Parametre | Tip | Varsayılan | Açıklama |
|-----------|-----|------------|----------|
| page | int | 1 | Sayfa numarası |
| pageSize | int | 10 | Sayfa boyutu |
| search | string? | null | İsme göre arama |
| city | string? | null | Şehre göre filtre |
| isActive | bool? | null | Aktiflik filtresi |
| minPrice | decimal? | null | Min fiyat |
| maxPrice | decimal? | null | Max fiyat |

### GET `/{id}` — Detay Response

```csharp
public class BeachDetailDto
{
    public Guid Id { get; set; }
    public MultiLanguageDto Name { get; set; } = new();
    public string Slug { get; set; } = string.Empty;
    public MultiLanguageDto Description { get; set; } = new();
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? LocationSubtitle { get; set; }
    public string? MapImageUrl { get; set; }
    public decimal PricePerDay { get; set; }
    public string Currency { get; set; } = "USD";
    public string PriceUnit { get; set; } = "day";   // "day" | "hour"
    public List<ImageDto> Images { get; set; } = new();
    public BeachConditionsDto Conditions { get; set; } = new();
    public List<AmenityDto> Amenities { get; set; } = new();
    public string? RareFindMessage { get; set; }
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }
    public List<BreadcrumbItemDto> Breadcrumbs { get; set; } = new();
}

public class BeachConditionsDto
{
    public string WindSpeed { get; set; } = string.Empty;     // ör: "12 km/h"
    public string WaterDepth { get; set; } = string.Empty;    // ör: "Max 2.5m"
    public string GroundType { get; set; } = string.Empty;    // ör: "Fine Sand"
    public string WaveStatus { get; set; } = string.Empty;    // ör: "Calm"
}
```

### POST `/` — Create Request

```csharp
public class CreateBeachRequest
{
    [Required] public MultiLanguageDto Name { get; set; } = new();
    [Required] public string Slug { get; set; } = string.Empty;
    [Required] public MultiLanguageDto Description { get; set; } = new();
    [Required] public string City { get; set; } = string.Empty;
    [Required] public string Country { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? LocationSubtitle { get; set; }
    public string? MapImageUrl { get; set; }
    [Required] public decimal PricePerDay { get; set; }
    public string Currency { get; set; } = "USD";
    public string PriceUnit { get; set; } = "day";
    public List<ImageDto> Images { get; set; } = new();
    public BeachConditionsDto Conditions { get; set; } = new();
    public List<AmenityDto> Amenities { get; set; } = new();
    public string? RareFindMessage { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; }
    public List<BreadcrumbItemDto> Breadcrumbs { get; set; } = new();
}
```

**Response:** `201 Created` → `BeachDetailDto`

---

## 4. Pools API

**Controller:** `AdminPoolsController`
**Route Prefix:** `/api/admin/pools`

### Endpointler

| Method | Endpoint | Açıklama |
|--------|----------|----------|
| GET | `/` | Havuzları listele |
| GET | `/{id}` | Havuz detay |
| POST | `/` | Yeni havuz oluştur |
| PUT | `/{id}` | Havuz güncelle |
| DELETE | `/{id}` | Havuz sil |

### GET `/` — Liste Response

```csharp
public class PoolListItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public decimal PricePerDay { get; set; }
    public string Currency { get; set; } = "USD";
    public double Rating { get; set; }
    public int ReviewCount { get; set; }
    public bool IsActive { get; set; }
}
```

**Query Parametreleri:** Beach ile aynı (page, pageSize, search, city, isActive, minPrice, maxPrice).

### GET `/{id}` — Detay Response

```csharp
public class PoolDetailDto
{
    public Guid Id { get; set; }
    public MultiLanguageDto Name { get; set; } = new();
    public string Slug { get; set; } = string.Empty;
    public MultiLanguageDto Description { get; set; } = new();
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? LocationSubtitle { get; set; }
    public string? MapImageUrl { get; set; }
    public decimal PricePerDay { get; set; }
    public string Currency { get; set; } = "USD";
    public string PriceUnit { get; set; } = "day";
    public List<ImageDto> Images { get; set; } = new();
    public PoolConditionsDto Conditions { get; set; } = new();
    public List<AmenityDto> Amenities { get; set; } = new();
    public string? RareFindMessage { get; set; }
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }
    public List<BreadcrumbItemDto> Breadcrumbs { get; set; } = new();
}

public class PoolConditionsDto
{
    public string WaterTemperature { get; set; } = string.Empty;  // ör: "28°C"
    public string PoolDepth { get; set; } = string.Empty;         // ör: "1.2m - 2.5m"
    public string PoolLength { get; set; } = string.Empty;        // ör: "25 meters"
    public string SwimmingLanes { get; set; } = string.Empty;     // ör: "6 Lanes"
}
```

### POST `/` — Create Request

```csharp
public class CreatePoolRequest
{
    [Required] public MultiLanguageDto Name { get; set; } = new();
    [Required] public string Slug { get; set; } = string.Empty;
    [Required] public MultiLanguageDto Description { get; set; } = new();
    [Required] public string City { get; set; } = string.Empty;
    [Required] public string Country { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? LocationSubtitle { get; set; }
    public string? MapImageUrl { get; set; }
    [Required] public decimal PricePerDay { get; set; }
    public string Currency { get; set; } = "USD";
    public string PriceUnit { get; set; } = "day";
    public List<ImageDto> Images { get; set; } = new();
    public PoolConditionsDto Conditions { get; set; } = new();
    public List<AmenityDto> Amenities { get; set; } = new();
    public string? RareFindMessage { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; }
    public List<BreadcrumbItemDto> Breadcrumbs { get; set; } = new();
}
```

---

## 5. Yacht Tours API

**Controller:** `AdminYachtToursController`
**Route Prefix:** `/api/admin/yacht-tours`

### Endpointler

| Method | Endpoint | Açıklama |
|--------|----------|----------|
| GET | `/` | Yat turlarını listele |
| GET | `/{id}` | Yat turu detay |
| POST | `/` | Yeni yat turu oluştur |
| PUT | `/{id}` | Yat turu güncelle |
| DELETE | `/{id}` | Yat turu sil |

### GET `/` — Liste Response

```csharp
public class YachtTourListItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public decimal PricePerDay { get; set; }
    public string Currency { get; set; } = "USD";
    public int MaxCapacity { get; set; }
    public string? BoatType { get; set; }
    public double Rating { get; set; }
    public int ReviewCount { get; set; }
    public bool IsFeatured { get; set; }
}
```

**Query Parametreleri:**

| Parametre | Tip | Varsayılan | Açıklama |
|-----------|-----|------------|----------|
| page | int | 1 | Sayfa numarası |
| pageSize | int | 10 | Sayfa boyutu |
| search | string? | null | İsme göre arama |
| city | string? | null | Şehre göre filtre |
| boatType | string? | null | Tekne tipine göre filtre |
| minPrice | decimal? | null | Min fiyat |
| maxPrice | decimal? | null | Max fiyat |

### GET `/{id}` — Detay Response

```csharp
public class YachtTourDetailDto
{
    public Guid Id { get; set; }
    public MultiLanguageDto Name { get; set; } = new();
    public string Slug { get; set; } = string.Empty;
    public MultiLanguageDto Description { get; set; } = new();
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public decimal PricePerDay { get; set; }
    public string Currency { get; set; } = "USD";
    public int MinCapacity { get; set; }
    public int MaxCapacity { get; set; }
    public string BoatType { get; set; } = string.Empty;
    public double? BoatLength { get; set; }
    public int? CabinCount { get; set; }
    public int? BathroomCount { get; set; }
    public int? ManufacturerYear { get; set; }
    public List<ImageDto> Images { get; set; } = new();
    public string? RouteMapImage { get; set; }
    public List<YachtSpecDto> Specs { get; set; } = new();
    public List<YachtFeatureDto> Features { get; set; } = new();
    public List<AccommodationOptionDto> AccommodationOptions { get; set; } = new();
    public List<CateringItemDto> CateringItems { get; set; } = new();
    public List<ActivityItemDto> ActivityItems { get; set; } = new();
    public RouteInfoDto CruisingRoute { get; set; } = new();
    public MultiLanguageDto? Conditions { get; set; }
    public MultiLanguageDto? IncludedServices { get; set; }
    public MultiLanguageDto? ExcludedServices { get; set; }
    public List<BookingBreakdownItemDto> Breakdown { get; set; } = new();
    public string? LuxuryPromise { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsSuperhost { get; set; }
}

public class YachtSpecDto
{
    public string Icon { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public class YachtFeatureDto
{
    public string Icon { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
}

public class AccommodationOptionDto
{
    public string ImageUrl { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class CateringItemDto
{
    public string Icon { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}

public class ActivityItemDto
{
    public string Icon { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}

public class RouteInfoDto
{
    public string Name { get; set; } = string.Empty;
    public string Stops { get; set; } = string.Empty;
    public int TotalRoutes { get; set; }
}
```

### POST `/` — Create Request

```csharp
public class CreateYachtTourRequest
{
    [Required] public MultiLanguageDto Name { get; set; } = new();
    [Required] public string Slug { get; set; } = string.Empty;
    [Required] public MultiLanguageDto Description { get; set; } = new();
    [Required] public string City { get; set; } = string.Empty;
    [Required] public string Country { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    [Required] public decimal PricePerDay { get; set; }
    public string Currency { get; set; } = "USD";
    public int MinCapacity { get; set; } = 1;
    [Required] public int MaxCapacity { get; set; }
    [Required] public string BoatType { get; set; } = string.Empty;
    public double? BoatLength { get; set; }
    public int? CabinCount { get; set; }
    public int? BathroomCount { get; set; }
    public int? ManufacturerYear { get; set; }
    public List<ImageDto> Images { get; set; } = new();
    public string? RouteMapImage { get; set; }
    public List<YachtSpecDto> Specs { get; set; } = new();
    public List<YachtFeatureDto> Features { get; set; } = new();
    public List<AccommodationOptionDto> AccommodationOptions { get; set; } = new();
    public List<CateringItemDto> CateringItems { get; set; } = new();
    public List<ActivityItemDto> ActivityItems { get; set; } = new();
    public RouteInfoDto CruisingRoute { get; set; } = new();
    public MultiLanguageDto? Conditions { get; set; }
    public MultiLanguageDto? IncludedServices { get; set; }
    public MultiLanguageDto? ExcludedServices { get; set; }
    public List<BookingBreakdownItemDto> Breakdown { get; set; } = new();
    public string? LuxuryPromise { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsSuperhost { get; set; }
}
```

---

## 6. Day Trips API

**Controller:** `AdminDayTripsController`
**Route Prefix:** `/api/admin/day-trips`

### Endpointler

| Method | Endpoint | Açıklama |
|--------|----------|----------|
| GET | `/` | Günübirlik turları listele |
| GET | `/{id}` | Tur detay |
| POST | `/` | Yeni tur oluştur |
| PUT | `/{id}` | Tur güncelle |
| DELETE | `/{id}` | Tur sil |

### GET `/` — Liste Response

```csharp
public class DayTripListItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public decimal PricePerPerson { get; set; }
    public string Currency { get; set; } = "USD";
    public string Duration { get; set; } = string.Empty;
    public int MaxCapacity { get; set; }
    public double Rating { get; set; }
    public int ReviewCount { get; set; }
    public bool IsFeatured { get; set; }
}
```

### GET `/{id}` — Detay Response

```csharp
public class DayTripDetailDto
{
    public Guid Id { get; set; }
    public MultiLanguageDto Name { get; set; } = new();
    public string Slug { get; set; } = string.Empty;
    public List<string> Description { get; set; } = new();  // Paragraf dizisi
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? DeparturePoint { get; set; }
    public decimal PricePerPerson { get; set; }
    public decimal? PrivateCharterPrice { get; set; }
    public string Currency { get; set; } = "USD";
    public int? MinCapacity { get; set; }
    public int MaxCapacity { get; set; }
    public string Duration { get; set; } = string.Empty;
    public string? DepartureTime { get; set; }
    public List<ImageDto> Images { get; set; } = new();
    public HostInfoDto Host { get; set; } = new();
    public List<TourInfoBadgeDto> InfoBadges { get; set; } = new();
    public List<RouteStopDto> RouteStops { get; set; } = new();
    public List<AmenityDto> Amenities { get; set; } = new();
    public List<FoodItemDto> FoodItems { get; set; } = new();
    public List<ActivityTagDto> ActivityTags { get; set; } = new();
    public MusicInfoDto? MusicInfo { get; set; }
    public MultiLanguageDto? Route { get; set; }
    public MultiLanguageDto? Conditions { get; set; }
    public MultiLanguageDto? IncludedServices { get; set; }
    public MultiLanguageDto? ExcludedServices { get; set; }
    public bool? IsPrivateCharter { get; set; }
    public List<BookingBreakdownItemDto> Breakdown { get; set; } = new();
    public string? RareFindMessage { get; set; }
    public string? RareFindSubtitle { get; set; }
    public bool IsFeatured { get; set; }
    public bool? IsSuperhost { get; set; }
}

public class HostInfoDto
{
    public string AvatarUrl { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? Experience { get; set; }
    public int? YearStarted { get; set; }
}

public class TourInfoBadgeDto
{
    public string Icon { get; set; } = string.Empty;
    public string? Label { get; set; }
    public string? Value { get; set; }
    public string? Text { get; set; }
}

public class RouteStopDto
{
    public string Time { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class FoodItemDto
{
    public string Icon { get; set; } = string.Empty;
    public string? IconColor { get; set; }   // "green" | "gray" | "red"
    public string? Text { get; set; }
    public string? Name { get; set; }
}

public class ActivityTagDto
{
    public string Label { get; set; } = string.Empty;
    public string? Icon { get; set; }
}

public class MusicInfoDto
{
    public string Text { get; set; } = string.Empty;
}
```

### POST `/` — Create Request

```csharp
public class CreateDayTripRequest
{
    [Required] public MultiLanguageDto Name { get; set; } = new();
    [Required] public string Slug { get; set; } = string.Empty;
    public List<string> Description { get; set; } = new();
    [Required] public string City { get; set; } = string.Empty;
    [Required] public string Country { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? DeparturePoint { get; set; }
    [Required] public decimal PricePerPerson { get; set; }
    public decimal? PrivateCharterPrice { get; set; }
    public string Currency { get; set; } = "USD";
    public int? MinCapacity { get; set; }
    [Required] public int MaxCapacity { get; set; }
    [Required] public string Duration { get; set; } = string.Empty;
    public string? DepartureTime { get; set; }
    public List<ImageDto> Images { get; set; } = new();
    [Required] public HostInfoDto Host { get; set; } = new();
    public List<TourInfoBadgeDto> InfoBadges { get; set; } = new();
    public List<RouteStopDto> RouteStops { get; set; } = new();
    public List<AmenityDto> Amenities { get; set; } = new();
    public List<FoodItemDto> FoodItems { get; set; } = new();
    public List<ActivityTagDto> ActivityTags { get; set; } = new();
    public MusicInfoDto? MusicInfo { get; set; }
    public MultiLanguageDto? Route { get; set; }
    public MultiLanguageDto? Conditions { get; set; }
    public MultiLanguageDto? IncludedServices { get; set; }
    public MultiLanguageDto? ExcludedServices { get; set; }
    public bool? IsPrivateCharter { get; set; }
    public List<BookingBreakdownItemDto> Breakdown { get; set; } = new();
    public string? RareFindMessage { get; set; }
    public string? RareFindSubtitle { get; set; }
    public bool IsFeatured { get; set; }
    public bool? IsSuperhost { get; set; }
}
```

---

## 7. Blogs API

**Controller:** `AdminBlogsController`
**Route Prefix:** `/api/admin/blogs`

### Endpointler

| Method | Endpoint | Açıklama |
|--------|----------|----------|
| GET | `/` | Blog yazılarını listele |
| GET | `/{id}` | Blog detay |
| POST | `/` | Yeni blog oluştur |
| PUT | `/{id}` | Blog güncelle |
| DELETE | `/{id}` | Blog sil |

### GET `/` — Liste Response

```csharp
public class BlogListItemDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public string ReadTime { get; set; } = string.Empty;
    public int ViewCount { get; set; }
    public bool? IsFeatured { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }
}
```

**Query Parametreleri:**

| Parametre | Tip | Varsayılan | Açıklama |
|-----------|-----|------------|----------|
| page | int | 1 | Sayfa numarası |
| pageSize | int | 10 | Sayfa boyutu |
| search | string? | null | Başlığa göre arama |
| category | string? | null | Kategoriye göre filtre |
| isPublished | bool? | null | Yayın durumu filtresi |

### GET `/{id}` — Detay Response

```csharp
public class BlogDetailDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string HeroImageUrl { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public BlogAuthorDto Author { get; set; } = new();
    public string ReadTime { get; set; } = string.Empty;
    public List<ContentBlockDto> Content { get; set; } = new();
    public List<TOCItemDto> TableOfContents { get; set; } = new();
    public bool IsFeatured { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }
}

public class BlogAuthorDto
{
    public string Name { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string AvatarUrl { get; set; } = string.Empty;
}

public class ContentBlockDto
{
    public string Type { get; set; } = string.Empty;  // "paragraph","heading","subheading","image","quote","tip"
    public string? Text { get; set; }
    public string? ImageUrl { get; set; }
    public string? Caption { get; set; }
    public string? Author { get; set; }       // Alıntı yazarı
    public int? Level { get; set; }           // Heading seviyesi (2, 3, 4)
    public string? Id { get; set; }           // Anchor link / TOC bağlantısı
}

public class TOCItemDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
}
```

### POST `/` — Create Request

```csharp
public class CreateBlogRequest
{
    [Required] public string Title { get; set; } = string.Empty;
    [Required] public string Slug { get; set; } = string.Empty;
    [Required] public string Description { get; set; } = string.Empty;
    [Required] public string ImageUrl { get; set; } = string.Empty;
    [Required] public string HeroImageUrl { get; set; } = string.Empty;
    [Required] public string Category { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    [Required] public BlogAuthorDto Author { get; set; } = new();
    public string ReadTime { get; set; } = string.Empty;
    public List<ContentBlockDto> Content { get; set; } = new();
    public List<TOCItemDto> TableOfContents { get; set; } = new();
    public bool IsFeatured { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }
}
```

---

## 8. Ortak Sayfalama & Filtreleme

Tüm liste endpointleri aşağıdaki ortak query parametrelerini destekler:

| Parametre | Tip | Varsayılan | Açıklama |
|-----------|-----|------------|----------|
| page | int | 1 | Sayfa numarası (1-based) |
| pageSize | int | 10 | Sayfa boyutu (max: 100) |
| sortBy | string? | "createdAt" | Sıralama alanı |
| sortOrder | string? | "desc" | "asc" veya "desc" |

**Tüm response'lar `PaginatedResponse<T>` formatında dönmelidir.**

### Ortak HTTP Status Code'ları

| Code | Açıklama |
|------|----------|
| 200 | Başarılı GET/PUT |
| 201 | Başarılı POST (Location header ile) |
| 204 | Başarılı DELETE |
| 400 | Validation hatası (FluentValidation veya DataAnnotations) |
| 401 | Kimlik doğrulaması gerekli |
| 403 | Yetki yetersiz (Admin değil) |
| 404 | Kayıt bulunamadı |
| 409 | Slug çakışması (unique constraint) |
| 500 | Sunucu hatası |

### Hata Response Modeli

```csharp
public class ApiErrorResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, string[]>? Errors { get; set; }  // Validation hataları
}
```

---

## 9. Media Upload API

**Controller:** `AdminMediaController`
**Route Prefix:** `/api/admin/media`

Resim yüklemeleri için ayrı bir endpoint. Supabase Storage kullanılır.

| Method | Endpoint | Açıklama |
|--------|----------|----------|
| POST | `/upload` | Tekli resim yükle |
| POST | `/upload-multiple` | Çoklu resim yükle |
| DELETE | `/{fileName}` | Resim sil |

### POST `/upload` — Request

```
Content-Type: multipart/form-data
- file: IFormFile (max 5MB, jpg/png/webp)
- folder: string (ör: "beaches", "yachts", "blogs")
```

### POST `/upload` — Response

```csharp
public class MediaUploadResponse
{
    public string Url { get; set; } = string.Empty;         // Public URL
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
}
```

---

## Mimari Notlar

### Katmanlı Yapı (Clean Architecture)

```
Swimago.Domain/         → Entity sınıfları, temel arayüzler
Swimago.Application/    → DTO'lar, Service arabirimleri, Validasyonlar
Swimago.Infrastructure/ → EF Core DbContext, Repository implementasyonları, Supabase Storage
Swimago.API/            → Controller'lar, Middleware, Program.cs
```

### Veritabanı Notları

- **MultiLanguage alanları** → PostgreSQL `jsonb` column (EF Core `HasColumnType("jsonb")`)
- **Images, Amenities, Specs** gibi alt listeler → Ya `jsonb` array olarak ya da ayrı ilişkili tablo olarak saklanabilir. Basitlik için JSONB önerilir.
- **Slug** → Her entity'de `unique index` olmalıdır
- **Soft Delete** → `IsDeleted` flag + `DeletedAt` timestamp
- **Audit Fields** → `CreatedAt`, `UpdatedAt`, `CreatedBy`, `UpdatedBy`

### Örnek Entity Base Class

```csharp
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}
```
