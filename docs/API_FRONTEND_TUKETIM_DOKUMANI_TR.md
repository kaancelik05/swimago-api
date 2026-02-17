# Swimago API Frontend Tüketim Dokümantasyonu (TR)

Bu doküman, `/Users/kaancelik/my-projects/swimago-stich-api` içindeki controller ve DTO kaynak kodları baz alınarak hazırlanmıştır.
Amaç: frontend tarafında endpointleri doğru request/response formatı ile consume etmek.

- Son güncelleme: 2026-02-16
- Kaynak baz: `src/Swimago.API/Controllers/**`, `src/Swimago.Application/DTOs/**`
- Not: Bazı endpointler farklı hata formatı döndürüyor (detay aşağıda).

## 1) Genel Kurallar

- Base path: `/api`
- Auth tipi: `Authorization: Bearer {jwt}`
- Content-Type (JSON endpointler): `application/json`
- Dosya upload endpointleri: `multipart/form-data`

### 1.1 Hata formatı

Projede iki farklı hata formatı görülebilir:

1. Controller-level inline hata:
```json
{ "error": "..." }
```

2. Global exception middleware hata formatı:
```json
{
  "statusCode": 400,
  "message": "Validation failed.",
  "details": null,
  "validationErrors": {
    "field": ["error1", "error2"]
  }
}
```

### 1.2 Enum değerleri (frontend için)

- `Role`: `Admin`, `Host`, `Customer`
- `ListingType`: `Beach`, `Pool`, `Yacht`, `DayTrip`
- `VenueType`: `Beach`, `Pool`, `Yacht`, `DayTrip`
- `ListingStatus`: `Pending`, `Active`, `Inactive`, `Rejected`
- `ReservationStatus`: `Pending`, `Confirmed`, `InProgress`, `Completed`, `Cancelled`, `NoShow`
- `UserStatus`: `Active`, `Banned`, `Pending`
- `BookingType`: `Hourly`, `Daily`
- `PaymentBrand`: `Visa`, `Mastercard`, `Amex`
- `PaymentStatus`: `Pending`, `Processing`, `Completed`, `Failed`, `Refunded`, `PartiallyRefunded`

---

## 2) Endpoint Envanteri (Amaç + Request + Response)

## 2.1 Auth (`/api/auth`)

| Method | Endpoint | Auth | Amaç | Request | Success Response |
|---|---|---|---|---|---|
| POST | `/api/auth/register` | Yok | Yeni kullanıcı kaydı | Body: `RegisterRequest` | `200 OK` → `AuthResponse` |
| POST | `/api/auth/login` | Yok | Login | Body: `LoginRequest` | `200 OK` → `AuthResponse` |
| POST | `/api/auth/logout` | Bearer | Oturumu kapat (token blacklist TODO) | Body yok | `204 No Content` |
| POST | `/api/auth/refresh` | Yok | Refresh token ile yeni token al | Body: `RefreshTokenRequest` | `200 OK` → `AuthResponse` |
| POST | `/api/auth/forgot-password` | Yok | Şifre sıfırlama mail akışı (şu an mock başarı) | Body: `ForgotPasswordRequest` | `200 OK` → `{ message }` |
| POST | `/api/auth/reset-password` | Yok | Token ile şifre sıfırlama | Body: `ResetPasswordRequest` | `200 OK` → `{ message }` |

Ek hata kodları:
- `register`: `400`
- `login`: `401`
- `refresh`: `401`
- `reset-password`: `400`

## 2.2 Users (`/api/users`)

| Method | Endpoint | Auth | Amaç | Request | Success Response |
|---|---|---|---|---|---|
| GET | `/api/users/me` | Bearer | Giriş yapan kullanıcı profili | Body yok | `200 OK` → `UserProfileResponse` |
| PUT | `/api/users/me` | Bearer | Profil güncelle | Body: `UpdateProfileRequest` | `200 OK` → `UserProfileResponse` |
| PUT | `/api/users/me/avatar` | Bearer | Avatar güncelle | Form-data: `file` | `200 OK` → `UpdateAvatarResponse` |
| PUT | `/api/users/me/settings` | Bearer | Kullanıcı ayarlarını güncelle | Body: `UpdateSettingsRequest` | `204 No Content` |
| PUT | `/api/users/me/password` | Bearer | Şifre değiştir | Body: `ChangePasswordRequest` | `204 No Content` |
| DELETE | `/api/users/me` | Bearer | Hesap sil | Body: `DeleteAccountRequest` | `204 No Content` |

Ek hata kodları:
- `me`: `404`
- `me/avatar`: `400`, `404`
- `me/settings`: `404`
- `me/password`: `400`, `404`
- `me (delete)`: `404`

## 2.3 Listings (`/api/listings`)

| Method | Endpoint | Auth | Amaç | Request | Success Response |
|---|---|---|---|---|---|
| GET | `/api/listings` | Yok | Aktif ilanları sayfalı getir | Query: `page`, `pageSize` (`PaginationQuery`) | `200 OK` → `PagedResult<ListingResponse>` |
| GET | `/api/listings/{id}` | Yok | ID ile ilan detayı | Path: `id` | `200 OK` → `ListingResponse` |
| GET | `/api/listings/type/{type}` | Yok | Türe göre ilan listele | Path: `type: ListingType` | `200 OK` → `ListingResponse[]` |
| GET | `/api/listings/nearby` | Yok | Konuma göre yakın ilan | Query: `latitude`, `longitude`, `radius=10`, `type?` | `200 OK` → `{ searchCenter, radiusKm, type, count, results[] }` |
| POST | `/api/listings` | Bearer + Role(`Host`/`Admin`) | Yeni ilan oluştur | Body: `ListingsController.CreateListingRequest` | `201 Created` → `{ message: "İlan oluşturuldu" }` |
| POST | `/api/listings/photos/upload` | Bearer + Role(`Host`/`Admin`) | Fotoğraf yükle | Form-data: `files[]` | `200 OK` → `string[]` (uploaded URL list) |

Ek hata kodları:
- `/{id}`: `404`
- `/nearby`: `400`
- `POST /`: `400`
- `POST /photos/upload`: `400`

## 2.4 Reservations (`/api/reservations`)

| Method | Endpoint | Auth | Amaç | Request | Success Response |
|---|---|---|---|---|---|
| GET | `/api/reservations` | Bearer | Kullanıcının rezervasyonları | Query: `status?`, `page=1`, `pageSize=10` | `200 OK` → `ReservationListResponse` |
| POST | `/api/reservations` | Bearer | Rezervasyon oluştur | Body: `CreateReservationRequest` | `201 Created` → `ReservationResponse` |
| GET | `/api/reservations/{id}` | Bearer | Rezervasyon detayı | Path: `id` | `200 OK` → `ReservationResponse` |
| PUT | `/api/reservations/{id}` | Bearer | Rezervasyon güncelle (şu an mevcut kaydı döner) | Path: `id`, Body: `UpdateReservationRequest` | `200 OK` → `ReservationResponse` |
| POST | `/api/reservations/{id}/cancel` | Bearer | Rezervasyonu iptal et | Path: `id` | `204 No Content` |
| POST | `/api/reservations/{id}/check-in` | Bearer | Check-in işlemi | Path: `id` | `204 No Content` |
| POST | `/api/reservations/{id}/review` | Bearer | Tamamlanan rezervasyona yorum | Path: `id`, Body: `SubmitReviewRequest` | `201 Created` → `{ message }` |
| GET | `/api/reservations/check-availability` | Yok (`AllowAnonymous`) | Müsaitlik sorgusu | Query: `listingId`, `startTime`, `endTime` | `200 OK` → `{ listingId, startTime, endTime, isAvailable }` |

Ek hata kodları:
- `POST /`: `400`, `404`
- `GET /{id}`: `403`, `404`
- `PUT /{id}`: `400`, `403`, `404`
- `cancel`: `400`, `403`, `404`
- `check-in`: `400`, `403`, `404`
- `review`: `400`, `403`, `404`
- `check-availability`: `400`

## 2.5 Reviews (`/api/reviews`)

| Method | Endpoint | Auth | Amaç | Request | Success Response |
|---|---|---|---|---|---|
| POST | `/api/reviews` | Bearer | Yorum oluştur | Body: `CreateReviewRequest` | `201 Created` → `ReviewResponse` |
| GET | `/api/reviews/{id}` | Yok (`AllowAnonymous`) | Yorum detayı | Path: `id` | `200 OK` → `ReviewResponse` |
| GET | `/api/reviews/listing/{listingId}` | Yok (`AllowAnonymous`) | İlana ait yorumlar | Path: `listingId` | `200 OK` → `ReviewResponse[]` |
| POST | `/api/reviews/{id}/host-response` | Bearer | Host cevabı ekle | Path: `id`, Body: `AddHostResponseRequest` | `200 OK` → `ReviewResponse` |
| DELETE | `/api/reviews/{id}` | Bearer | Yorumu sil | Path: `id` | `204 No Content` |

Ek hata kodları:
- `POST /`: `400`, `404`
- `GET /{id}`: `404`
- `host-response`: `400`, `403`, `404`
- `DELETE /{id}`: `403`, `404`

## 2.6 Favorites (`/api/favorites`)

| Method | Endpoint | Auth | Amaç | Request | Success Response |
|---|---|---|---|---|---|
| GET | `/api/favorites` | Bearer | Favori listesi | Query: `type?: VenueType` | `200 OK` → `FavoriteListResponse` |
| POST | `/api/favorites` | Bearer | Favoriye ekle | Body: `AddFavoriteRequest` | `201 Created` → `FavoriteItemDto` |
| DELETE | `/api/favorites/{venueId}` | Bearer | Favoriden kaldır | Path: `venueId` | `204 No Content` |

Ek hata kodları:
- POST: `400`, `404`
- DELETE: `404`

## 2.7 Payment Methods (`/api/payment-methods`)

| Method | Endpoint | Auth | Amaç | Request | Success Response |
|---|---|---|---|---|---|
| GET | `/api/payment-methods` | Bearer | Kayıtlı kartları getir | Body yok | `200 OK` → `PaymentMethodListResponse` |
| POST | `/api/payment-methods` | Bearer | Kart ekle | Body: `AddPaymentMethodRequest` | `201 Created` → `PaymentMethodResponse` |
| DELETE | `/api/payment-methods/{id}` | Bearer | Kart sil | Path: `id` | `204 No Content` |
| PUT | `/api/payment-methods/{id}/default` | Bearer | Varsayılan kart ata | Path: `id` | `204 No Content` |

Ek hata kodları:
- POST: `400`
- DELETE: `400`, `404`
- default: `404`

## 2.8 Newsletter (`/api/newsletter`)

| Method | Endpoint | Auth | Amaç | Request | Success Response |
|---|---|---|---|---|---|
| POST | `/api/newsletter/subscribe` | Yok | Newsletter aboneliği | Body: `NewsletterSubscribeRequest` | `200 OK` → `NewsletterSubscribeResponse` |
| POST | `/api/newsletter/unsubscribe` | Yok | Newsletter abonelikten çık | Query: `email`, `token?` | `200 OK` → `NewsletterSubscribeResponse` |

Ek hata kodları:
- subscribe: `400`
- unsubscribe: `404`

## 2.9 Destinations (`/api/destinations`)

| Method | Endpoint | Auth | Amaç | Request | Success Response |
|---|---|---|---|---|---|
| GET | `/api/destinations` | Yok | Destinasyon listele | Query: `featured?: bool` | `200 OK` → `DestinationListResponse` |
| GET | `/api/destinations/{slug}` | Yok | Slug ile destinasyon detayı | Path: `slug` | `200 OK` → `DestinationDetailResponse` |

Ek hata kodları:
- `/{slug}`: `404`

## 2.10 Spots (`/api/spots`)

| Method | Endpoint | Auth | Amaç | Request | Success Response |
|---|---|---|---|---|---|
| GET | `/api/spots/{slug}` | Yok | Beach/Pool spot detayını getir | Path: `slug` | `200 OK` → `SpotDetailResponse` |

Ek hata kodları:
- `404`

## 2.11 Explore (`/api/explore`)

| Method | Endpoint | Auth | Amaç | Request | Success Response |
|---|---|---|---|---|---|
| GET | `/api/explore` | Yok | Harita marker datası | Query: `neLat?`, `neLng?`, `swLat?`, `swLng?`, `type?: VenueType` | `200 OK` → `ExploreResponse` |

## 2.12 Search (`/api/search`)

| Method | Endpoint | Auth | Amaç | Request | Success Response |
|---|---|---|---|---|---|
| POST | `/api/search/listings` | Yok | Gelişmiş arama | Body: `SearchListingsQuery` | `200 OK` → `SearchListingsResponse` |
| GET | `/api/search/suggestions` | Yok | Otomatik öneri | Query: `term` | `200 OK` → `string[]` |

Ek hata kodları:
- `POST /listings`: `400`

## 2.13 Boat Tours (`/api/boat-tours`)

| Method | Endpoint | Auth | Amaç | Request | Success Response |
|---|---|---|---|---|---|
| GET | `/api/boat-tours` | Yok | Yacht + day-trip liste | Query: `city?`, `type?`, `minPrice?`, `maxPrice?` | `200 OK` → `BoatTourListResponse` |
| GET | `/api/boat-tours/yacht/{slug}` | Yok | Yacht detay | Path: `slug` | `200 OK` → `YachtTourDetailResponse` |
| GET | `/api/boat-tours/day-trip/{slug}` | Yok | Day-trip detay | Path: `slug` | `200 OK` → `DayTripDetailResponse` |

Ek hata kodları:
- detay endpointleri: `404`

## 2.14 Blog (`/api/blog`)

| Method | Endpoint | Auth | Amaç | Request | Success Response |
|---|---|---|---|---|---|
| GET | `/api/blog` | Yok (`AllowAnonymous`) | Yayınlı blogları getir | Query: `PaginationQuery` (`page`, `pageSize`) | `200 OK` → `PagedResult<BlogPostDto>` |
| GET | `/api/blog/{slug}` | Yok (`AllowAnonymous`) | Blog detayı | Path: `slug` | `200 OK` → `BlogPostDto` |
| POST | `/api/blog` | Bearer + Role(`Admin`) | Blog oluştur | Body: `CreateBlogPostRequest` | `201 Created` → `BlogPostResponse/BlogPostDto` |
| PUT | `/api/blog/{id}` | Bearer + Role(`Admin`) | Blog güncelle | Path: `id`, Body: `UpdateBlogPostRequest` | `200 OK` → `BlogPostResponse/BlogPostDto` |
| DELETE | `/api/blog/{id}` | Bearer + Role(`Admin`) | Blog sil | Path: `id` | `204 No Content` |

Ek hata kodları:
- `GET /{slug}`: `404`

## 2.15 Host Panel (`/api/host`)

Not: Controller seviyesinde sadece `[Authorize]` var. Host doğrulaması çoğunlukla service içinde yapılır.

| Method | Endpoint | Auth | Amaç | Request | Success Response |
|---|---|---|---|---|---|
| GET | `/api/host/dashboard` | Bearer | Host dashboard metrikleri | Body yok | `200 OK` → `HostDashboardResponse` |
| GET | `/api/host/listings` | Bearer | Host listing listesi | Body yok | `200 OK` → `HostListingListResponse` |
| GET | `/api/host/listings/{id}` | Bearer | Host listing detayı | Path: `id` | `200 OK` → `HostListingItemDto` |
| PUT | `/api/host/listings/{id}` | Bearer | Listing temel bilgileri güncelle | Path: `id`, Body: `UpdateListingRequest` | `204 No Content` |
| PUT | `/api/host/listings/{id}/pricing` | Bearer | Listing fiyat bilgileri güncelle | Path: `id`, Body: `UpdatePricingRequest` | `204 No Content` |
| DELETE | `/api/host/listings/{id}` | Bearer | Listing pasife al/sil | Path: `id` | `204 No Content` |
| GET | `/api/host/reservations` | Bearer | Host rezervasyon listesi | Query: `status?: ReservationStatus` | `200 OK` → `HostReservationListResponse` |
| PUT | `/api/host/reservations/{id}/status` | Bearer | Rezervasyon durumunu güncelle | Path: `id`, Body: `UpdateReservationStatusRequest` | `204 No Content` |
| GET | `/api/host/calendar` | Bearer | Takvim verisi getir | Query: `listingId`, `start`, `end` | `200 OK` → `HostCalendarResponse` |
| PUT | `/api/host/calendar` | Bearer | Takvim güncelle | Body: `UpdateCalendarRequest` | `200 OK` → `{ message: "Takvim güncellendi" }` |
| GET | `/api/host/analytics` | Bearer | Analytics verisi | Query: `start`, `end` | `200 OK` → `HostAnalyticsResponse` |

Ek hata kodları:
- listing detail/update/delete: `403`, `404`
- reservation status: `403`, `404`
- calendar get: `404`

## 2.16 Admin Core (`/api/admin`)

Tüm endpointler: Bearer + `Role=Admin`

| Method | Endpoint | Amaç | Request | Success Response |
|---|---|---|---|---|
| GET | `/api/admin/dashboard` | Dashboard özet | Body yok | `200 OK` → `AdminDashboardResponse` |
| GET | `/api/admin/users` | Kullanıcı listesi | Query: `role?`, `status?`, `search?`, `page=1`, `pageSize=10` | `200 OK` → `AdminUserListResponse` |
| GET | `/api/admin/users/{id}` | Kullanıcı detay | Path: `id` | `200 OK` → `AdminUserDetailResponse` |
| PUT | `/api/admin/users/{id}/status` | Kullanıcı status güncelle | Path: `id`, Body: `UpdateUserStatusRequest` | `204 No Content` |
| PUT | `/api/admin/users/{id}/role` | Kullanıcı rol güncelle | Path: `id`, Body: `UpdateUserRoleRequest` | `204 No Content` |
| GET | `/api/admin/host-applications` | Host başvuruları | Body yok | `200 OK` → `HostApplicationListResponse` |
| POST | `/api/admin/host-applications/{userId}/reject` | Host başvuru reddi | Path: `userId`, Body: `RejectHostRequest` | `204 No Content` |
| GET | `/api/admin/listings` | Listing listesi | Query: `status?`, `search?`, `page=1`, `pageSize=10` | `200 OK` → `AdminListingListResponse` |
| POST | `/api/admin/listings/{id}/approve` | Listing onayla | Path: `id` | `204 No Content` |
| POST | `/api/admin/listings/{id}/reject` | Listing reddet | Path: `id`, Body: `RejectListingRequest` | `204 No Content` |
| GET | `/api/admin/reports` | Rapor/analitik | Query: `start`, `end` | `200 OK` → `AdminReportResponse` |
| GET | `/api/admin/cities` | Şehir listesi | Body yok | `200 OK` → `CityListResponse` |
| POST | `/api/admin/cities` | Şehir oluştur | Body: `CreateCityRequest` | `204 No Content` |
| GET | `/api/admin/amenities` | Amenity listesi | Body yok | `200 OK` → `AmenityListResponse` |
| POST | `/api/admin/amenities` | Amenity oluştur | Body: `CreateAmenityRequest` | `204 No Content` |

Ek hata kodları:
- `users/{id}`: `404`
- `users/{id}/status`: `404`
- `users/{id}/role`: `404`
- `listings/{id}/approve`: `404`
- `listings/{id}/reject`: `404`

## 2.17 Admin Blogs (`/api/admin/blogs`)

Tüm endpointler: Bearer + `Role=Admin`

| Method | Endpoint | Amaç | Request | Success Response |
|---|---|---|---|---|
| GET | `/api/admin/blogs` | Blog listesi | Query: `search?`, `category?`, `isPublished?`, `page=1`, `pageSize=10` | `200 OK` → `PaginatedResponse<BlogListItemDto>` |
| GET | `/api/admin/blogs/{id}` | Blog detay | Path: `id` | `200 OK` → `BlogDetailDto` |
| POST | `/api/admin/blogs` | Blog oluştur | Body: `CreateBlogRequest` | `201 Created` → `BlogDetailDto` |
| PUT | `/api/admin/blogs/{id}` | Blog güncelle | Path: `id`, Body: `CreateBlogRequest` | `200 OK` → `BlogDetailDto` |
| DELETE | `/api/admin/blogs/{id}` | Blog sil | Path: `id` | `204 No Content` |

Ek hata kodları:
- detail/update: `404`
- create: `400`

## 2.18 Admin Destinations (`/api/admin/destinations`)

Tüm endpointler: Bearer + `Role=Admin`

| Method | Endpoint | Amaç | Request | Success Response |
|---|---|---|---|---|
| GET | `/api/admin/destinations` | Destinasyon listesi | Query: `search?`, `country?`, `isFeatured?`, `page=1`, `pageSize=10` | `200 OK` → `PaginatedResponse<DestinationListItemDto>` |
| GET | `/api/admin/destinations/{id}` | Destinasyon detay | Path: `id` | `200 OK` → `DestinationDetailDto` |
| POST | `/api/admin/destinations` | Destinasyon oluştur | Body: `CreateDestinationRequest` | `201 Created` → `DestinationDetailDto` |
| PUT | `/api/admin/destinations/{id}` | Destinasyon güncelle | Path: `id`, Body: `CreateDestinationRequest` | `200 OK` → `DestinationDetailDto` |
| DELETE | `/api/admin/destinations/{id}` | Destinasyon sil | Path: `id` | `204 No Content` |

Ek hata kodları:
- detail/update: `404`
- create/update: `400`

## 2.19 Admin Beaches (`/api/admin/beaches`)

Tüm endpointler: Bearer + `Role=Admin`

| Method | Endpoint | Amaç | Request | Success Response |
|---|---|---|---|---|
| GET | `/api/admin/beaches` | Beach listesi | Query: `search?`, `city?`, `isActive?`, `minPrice?`, `maxPrice?`, `page=1`, `pageSize=10` | `200 OK` → `PaginatedResponse<ListingListItemDto>` |
| GET | `/api/admin/beaches/{id}` | Beach detay | Path: `id` | `200 OK` → `BeachDetailDto` |
| POST | `/api/admin/beaches` | Beach oluştur | Body: `CreateBeachRequest` | `201 Created` → `BeachDetailDto` |
| PUT | `/api/admin/beaches/{id}` | Beach güncelle | Path: `id`, Body: `CreateBeachRequest` | `200 OK` → `BeachDetailDto` |
| DELETE | `/api/admin/beaches/{id}` | Beach sil | Path: `id` | `204 No Content` |

Ek hata kodları:
- detail/update: `404`

## 2.20 Admin Media (`/api/admin/media`)

Tüm endpointler: Bearer + `Role=Admin`

| Method | Endpoint | Amaç | Request | Success Response |
|---|---|---|---|---|
| POST | `/api/admin/media/upload` | Tek dosya yükle | Form-data: `file`, `folder` | `200 OK` → `MediaUploadResponse` |
| POST | `/api/admin/media/upload-multiple` | Çoklu dosya yükle | Form-data: `files[]`, `folder` | `200 OK` → `MediaUploadResponse[]` |
| DELETE | `/api/admin/media/{fileName}` | Dosya sil | Path: `fileName` | `204 No Content` |

Ek hata kodları:
- upload endpointleri: `400` (dosya yoksa)

## 2.21 Health (`/api/health`)

| Method | Endpoint | Auth | Amaç | Request | Success Response |
|---|---|---|---|---|---|
| GET | `/api/health` | Yok | API sağlık kontrol | Body yok | `200 OK` → `{ status, timestamp, version, service }` |
| GET | `/api/health/db` | Yok | DB bağlantı health check | Body yok | `200 OK` → `{ status, database, timestamp }` |

Ek hata kodları:
- `/db`: `500` → `{ status, database, error, timestamp }`

---

## 3) Request Model Sözlüğü

Aşağıdaki modeller endpoint body/form request’lerinde kullanılır.

### 3.1 Auth

- `RegisterRequest`
  - `email`, `password`, `firstName`, `lastName`, `phoneNumber?`, `role?`
- `LoginRequest`
  - `email`, `password`
- `RefreshTokenRequest`
  - `refreshToken`
- `ForgotPasswordRequest`
  - `email`
- `ResetPasswordRequest`
  - `token`, `newPassword`, `confirmPassword`

### 3.2 Users

- `UpdateProfileRequest`
  - `firstName?`, `lastName?`, `phoneNumber?`, `bio?`, `dateOfBirth?`, `country?`, `city?`
- `UpdateSettingsRequest`
  - `emailNotifications?`, `smsNotifications?`, `pushNotifications?`, `language?`, `currency?`, `profilePublic?`
- `ChangePasswordRequest`
  - `currentPassword`, `newPassword`, `confirmPassword`
- `DeleteAccountRequest`
  - `password`, `reason?`
- Avatar upload
  - Form-data: `file`

### 3.3 Listings / Search

- `ListingsController.CreateListingRequest` (controller-local)
  - `title`, `description`, `type`, `pricePerDay`, `city`, `country`, `latitude`, `longitude`
- `SearchListingsQuery`
  - Konum: `latitude?`, `longitude?`, `radiusKm?`
  - Filtreler: `type?`, `minPrice?`, `maxPrice?`, `minGuestCount?`, `minRating?`, `city?`, `amenityIds?`, `searchTerm?`
  - Sıralama: `sortBy`, `sortDescending`
  - Sayfalama: `page`, `pageSize`

### 3.4 Reservations

- `CreateReservationRequest`
  - `listingId`, `bookingType`, `startTime`, `endTime`, `guestCount`, `specialRequests?`
  - Opsiyoneller: `venueType?`, `guests?`, `selections?`, `paymentMethodId?`
- `UpdateReservationRequest`
  - `startTime?`, `endTime?`, `guests?`, `selections?`, `specialRequests?`
- `SubmitReviewRequest`
  - `rating`, `comment`

### 3.5 Reviews / Favorites / Payment / Newsletter

- `CreateReviewRequest`: `listingId`, `rating`, `comment`
- `AddHostResponseRequest`: `response`
- `AddFavoriteRequest`: `venueId`, `venueType`
- `AddPaymentMethodRequest`: `cardNumber`, `expiryMonth`, `expiryYear`, `cvv`, `cardholderName`, `setAsDefault?`
- `NewsletterSubscribeRequest`: `email`, `name?`, `language?`
- Newsletter unsubscribe: query `email`, `token?`

### 3.6 Blog

- `CreateBlogPostRequest`
  - `title`, `content`, `coverImageUrl?`, `isPublished?`
- `UpdateBlogPostRequest`
  - `title?`, `content?`, `coverImageUrl?`, `isPublished?`
- `CreateBlogRequest` (Admin blog panel)
  - `title`, `slug`, `description`, `imageUrl`, `heroImageUrl`, `category`, `tags[]`, `author`, `readTime`, `content[]`, `tableOfContents[]`, `isFeatured`, `isPublished`, `publishedAt?`

### 3.7 Host

- `UpdateListingRequest`
  - `name`, `description`, `capacity?`, `price`, `checkInTime?`, `checkOutTime?`
- `UpdatePricingRequest`
  - `basePricePerDay?`, `weekendPriceMultiplier?`, `seasonalPricing?[]`, `specialDates?[]`
- `UpdateReservationStatusRequest`
  - `status`, `message?`
- `UpdateCalendarRequest`
  - `listingId`, `updates[]` (`date`, `isAvailable`, `price?`)

### 3.8 Admin Core

- `UpdateUserStatusRequest`
  - `status`, `reason?`
- `UpdateUserRoleRequest`
  - `role`
- `RejectHostRequest`
  - `reason`
- `RejectListingRequest`
  - `reason`
- `CreateCityRequest`
  - `name`, `country?`, `slug?`, `isActive?`
- `CreateAmenityRequest`
  - `name`, `icon?`, `category?`, `isActive?`

### 3.9 Admin Destinations / Beaches / Media

- `CreateDestinationRequest`
  - `name`, `slug`, `country`, `description`, `subtitle?`, `imageUrl`, `mapImageUrl?`, `latitude?`, `longitude?`, `avgWaterTemp?`, `sunnyDaysPerYear?`, `tags[]`, `isFeatured`, `features[]`
- `CreateBeachRequest`
  - `name(MultiLanguageDto)`, `slug`, `description(MultiLanguageDto)`, `city`, `country`, `latitude`, `longitude`, `locationSubtitle?`, `mapImageUrl?`, `pricePerDay`, `currency`, `priceUnit`, `images[]`, `conditions`, `amenities[]`, `rareFindMessage?`, `isActive`, `isFeatured`, `breadcrumbs[]`
- Admin media upload
  - tekli: form-data `file`, `folder`
  - çoklu: form-data `files[]`, `folder`

---

## 4) Response Model Sözlüğü (Frontend’de en çok kullanılanlar)

### 4.1 Auth / User

- `AuthResponse`
  - `userId`, `email`, `firstName`, `lastName`, `avatar?`, `role`, `token`, `refreshToken`, `tokenExpiry`, `settings?`
- `UserProfileResponse`
  - temel: `id`, `email`, `role`, `isEmailVerified`, `createdAt`, `lastLoginAt?`
  - `profile`, `stats`, `settings`
- `UpdateAvatarResponse`
  - `avatarUrl`

### 4.2 Listing / Search / Explore / Spot

- `ListingResponse`
  - listing ana alanları + konum + fiyat + rating + images + amenities
- `PagedResult<T>`
  - `items[]`, `page`, `pageSize`, `totalCount`, `totalPages`, `hasPrevious`, `hasNext`
- `SearchListingsResponse`
  - `results: PagedResult<ListingResponse>`, `metadata`
- `ExploreResponse`
  - `markers[]`, `bounds`
- `SpotDetailResponse`
  - spot detay + host + amenity/image setleri

### 4.3 Reservation / Review / Favorite / Payment / Newsletter

- `ReservationResponse`
  - `id`, `listingId`, `guestId`, `listingTitle`, `venueType`, `confirmationNumber`, `startTime`, `endTime`, `guestCount`, `totalPrice`, `finalPrice`, `currency`, `status`, `bookingType`, `createdAt`, `specialRequests?`, `payment?`
- `ReservationListResponse`
  - `reservations[]`, `counts`, `totalCount`
- `ReviewResponse`
  - `id`, `listingId`, `guestId`, `guestName`, `rating`, `comment`, `createdAt`, `hostResponse?`, `hostResponseAt?`
- `FavoriteListResponse`
  - `favorites[]`, `totalCount`
- `PaymentMethodListResponse`
  - `paymentMethods[]`, `defaultPaymentMethodId?`
- `NewsletterSubscribeResponse`
  - `success`, `message?`

### 4.4 Blog / Boat / Destination

- `BoatTourListResponse`
  - `yachtTours[]`, `dayTrips[]`, `totalCount`
- `YachtTourDetailResponse`, `DayTripDetailResponse`
  - tekne turu detay DTO’ları
- `DestinationListResponse`, `DestinationDetailResponse`
  - destinasyon liste/detay
- `BlogPostDto`, `BlogPostResponse`
  - public/admin blog dönüşleri

### 4.5 Host Panel

- `HostDashboardResponse`
  - `stats`, `recentReservations[]`, `recentReviews[]`, `earnings`
- `HostListingListResponse`
  - `listings[]`, `totalCount`, `activeCount`, `pendingCount`, `inactiveCount`
- `HostReservationListResponse`
  - `reservations[]`, `counts`, `totalCount`
- `HostCalendarResponse`
  - `listingId`, `listingName`, `days[]`
- `HostAnalyticsResponse`
  - `currentPeriod`, `previousPeriod`, `dailyData[]`, `listingPerformance[]`

### 4.6 Admin Panel

- `AdminDashboardResponse`
- `AdminUserListResponse`, `AdminUserDetailResponse`
- `HostApplicationListResponse`
- `AdminListingListResponse`
- `AdminReportResponse`
- `CityListResponse`
- `AmenityListResponse`
- `PaginatedResponse<T>` (Admin list endpointleri için)
- `BlogDetailDto`, `DestinationDetailDto`, `BeachDetailDto`, `MediaUploadResponse`

---

## 5) Frontend için Kritik Notlar

1. `Listings POST /api/listings` şu an geçici response dönüyor:
   - Beklenen `ListingResponse` yerine `{ message: "İlan oluşturuldu" }`.

2. `Reservations PUT /api/reservations/{id}` update logic TODO:
   - Şu an var olan rezervasyonu geri döndürür, gerçek update davranışı sınırlı.

3. `Reservations POST /{id}/check-in` dönüşü `204`:
   - `CheckInResponse` DTO mevcut ama endpoint bunu döndürmüyor.

4. `Reviews/Reservations` ID tiplerinde model/kod tutarsızlıkları görülebilir:
   - Route parametreleri çoğunlukla `Guid`, bazı DTO alanları `int` tanımlı.

5. Hata formatı tek tip değil:
   - Bazı endpointler `{ error }`, bazıları middleware `ErrorResponse` döndürüyor.

6. Route casing:
   - Kodda `[controller]` token’ı (`Auth`, `Users` vs.) kullanılıyor; pratikte routing case-insensitive.
   - Frontend’de lowercase path kullanımı önerilir (`/api/auth`, `/api/users`, ...).

---

## 6) Kaynak Dosyalar

- Controllerlar: `src/Swimago.API/Controllers` ve `src/Swimago.API/Controllers/Admin`
- DTOlar: `src/Swimago.Application/DTOs`
- Enumlar: `src/Swimago.Domain/Enums`
- Global hata modeli: `src/Swimago.API/Models/ErrorResponse.cs`

