# Swimago Backend ve Veritabani Dokumani (Koddan Cikarim)

Son guncelleme: 2026-02-18
Kaynak: `src/Swimago.API`, `src/Swimago.Application`, `src/Swimago.Infrastructure`, `src/Swimago.Domain`

## 1) Projenin Amaci

Swimago; beach, pool, yacht ve day-trip odakli bir rezervasyon platformu API'sidir.
Backend su 3 ana islevi yurutur:
- Musteri tarafi: kesif, spot detay, teklif alma, rezervasyon, favori, odeme yontemi, yorum.
- Host panel: ilan yonetimi, rezervasyon operasyonu, takvim/fiyat override, analiz, isletme ayarlari.
- Admin panel: kullanici ve ilan moderasyonu, raporlama, destination/blog/media yonetimi.

## 2) Mimari ve Calisma Modeli

- Mimari: Clean Architecture yaklasimi.
  - Domain: entity, enum, repository interface.
  - Application: service, DTO, is kurallari.
  - Infrastructure: EF Core, repository implementasyonlari, dis servis adaptorleri.
  - API: controller, middleware, auth/policy.
- ORM: EF Core 8 + PostgreSQL + PostGIS (NetTopologySuite).
- Coklu dil: bir cok alanda JSONB (TR/EN vb. key-value).

### Runtime Akisi

1. HTTP request -> middleware zinciri.
2. JWT authenticate + policy authorize.
3. Controller input check + service cagrisi.
4. Service -> repository -> DbContext.
5. Response DTO donusu.

## 3) Guvenlik, Policy ve Cross-Cutting Davranislar

### Kimlik Dogrulama ve Yetki

- JWT Bearer aktif.
- Claim tipi:
  - User id: `ClaimTypes.NameIdentifier`
  - Role: `ClaimTypes.Role`
- Policy'ler:
  - `CustomerOnly`
  - `HostOnly`
  - `HostOrAdmin`
  - `AdminOnly`

### Middleware

- `SecurityHeadersMiddleware`:
  - HSTS, CSP, X-Frame-Options, X-Content-Type-Options, Referrer-Policy, Permissions-Policy.
- `RateLimitMiddleware`:
  - Kullanici bazli (auth ise user id, degilse IP) 100 request/dakika.
- `ExceptionMiddleware`:
  - `ValidationException -> 400`
  - `UnauthorizedAccessException -> 401`
  - `KeyNotFoundException -> 404`
  - `InvalidOperationException -> 400`
  - Digerleri -> 500

### Dikkat

- `appsettings.json` icinde gercek gorunen baglanti bilgileri ve JWT secret tutuluyor. Uretimde secret manager kullanilmasi gerekir.

## 4) Ana Is Akislari (Business Logic)

### 4.1 Auth Akisi

- Register:
  - Email unique kontrolu.
  - Sifre BCrypt hash.
  - Role default `Customer`.
  - Profile TR first/last name ile olusur.
  - Access + refresh token uretilir, refresh DB'ye yazilir.
- Login:
  - Email + password verify.
  - Role-spesifik login endpointlerinde role eslesme zorunlu.
- Refresh:
  - Refresh token DB'de bulunur ve expiry kontrol edilir.
- Logout:
  - Token blacklist yok, sadece 204 donuyor.
- Forgot/reset password:
  - Controller seviyesinde yanit veriliyor, gercek reset akisi TODO.

### 4.2 Musteri Profil Akisi

- `/api/users/me`:
  - Profil + rezervasyon/favori istatistikleri.
- Dashboard:
  - Yaklasan rezervasyon, favori preview, reward puanlari.
- Avatar:
  - max 5MB, `image/jpeg|png|webp` kontrolu.
- Change password:
  - Serviste yeni hash atama TODO (su an gercek sifre degisimi yok).
- Delete account:
  - Fiziksel silme degil, `UserStatus = Banned`.

### 4.3 Kesif ve Listing Akisi

- Public listing getirme: sadece `IsActive && Status=Active`.
- Nearby:
  - Lat/lng/radius validation.
  - PostGIS distance sorgusu (`Location.Distance(...)`).
- Musteri listing olusturma (`/api/listings` HostOrAdmin):
  - `title/type` zorunlu kontroller.
  - Slug unique uretilir.
  - Baslangicta draft/istenen status mapping.
  - Amenity label token eslestirme ile iliski kuruluyor.
- Publish (`/api/listings/{id}/publish`):
  - Ilan sahibi kontrolu.
  - `termsAccepted` ve `coverPhotoUrl` zorunlu.
  - Status -> `PendingReview`, aktiflik kapanir.

### 4.4 Spot, Destination, Boat Tour, Search Akisi

- Spot detail (`/api/spots/{slug}`):
  - Sadece Beach/Pool.
  - Galeri, condition, amenity, son yorumlar, rating breakdown, booking defaults olusturulur.
- Spot quote (`/api/spots/{slug}/quote`):
  - Fiyat = base + secili amenity ek ucret + service fee.
  - Müsaitlik overlap sorgusu ile kontrol edilir.
- Destination service:
  - Aktif beach/pool listing'ler sehre gore gruplanir.
  - Tip (Beach/Pool) listing dagilimina gore belirlenir.
- Boat tours:
  - Type `Yacht` ve `DayTrip` listinglerden model uretilir.
- Search:
  - POST `/api/search/listings`: gelismis filtre/siralama/facet.
  - GET `/api/search/listings`: customer kart modeli + favori bilgisi.
  - Not: customer search icinde `sortBy=distance` su an sehre gore siralama yapiyor (gercek geo mesafe degil).

### 4.5 Rezervasyon Akisi

- Create reservation:
  - Listing var/aktif kontrolu.
  - Max guest kontrolu.
  - Overlap kontrolu.
  - PricingService ile fiyat hesaplama.
  - `ReservationPayment` pending kaydi otomatik.
- Update reservation:
  - Sadece sahibi.
  - Cancelled/Completed ise update reddedilir.
  - Tarih degisirse overlap kontrolu tekrar yapilir.
- Cancel reservation:
  - Yetki ve state kontrolu.
- Check-in:
  - Sadece `Confirmed` rezervasyon check-in olur, sonra `Completed`.
- Submit review:
  - Sadece `Completed` rezervasyona bir kez.
- Payment intent:
  - Yoksa pending payment objesi olusturulur.
- Public availability:
  - `GET /api/reservations/check-availability`.

### 4.6 Review Akisi

- Yorum olusturma:
  - Kullanici ilgili listing icin `Completed` rezervasyona sahip olmali.
  - Ayni rezervasyon icin tekrar yorum yok.
  - Sonrasinda listing rating/reviewCount recalc.
- Host response:
  - Sadece listing sahibi host, bir kez cevap.
- Silme:
  - Sadece yorum sahibi silebilir.

### 4.7 Favorite, Payment Method, Newsletter

- Favorites:
  - Unique favorite kontrolu (user+venue).
  - Listeleme search/sort/pagination destekler.
- Payment methods:
  - Ilk kart veya `setAsDefault=true` ise default yapilir.
  - Brand/last4/expiry parse-normalize.
- Newsletter:
  - Subscribe idempotent benzeri davranis.
  - Unsubscribe token dogrulamasi TODO.

### 4.8 Host Panel Akisi

- Host listing CRUD + status update.
- Reservation list/filter (`status`, `source`, `listingId`).
- Reservation status update (`pending/confirmed/completed/cancelled/rejected`).
- Manual reservation:
  - `source` sadece `phone|walk-in`.
  - Gerekirse synthetic guest user olusturuyor.
  - Auto-confirm business setting'e bagli.
- Calendar:
  - Gunluk availability/custom price override (`DailyPricing`).
- Analytics:
  - week/month/year donemleri, revenue/reservation trend, occupancy, top listing.
- Business settings:
  - host bazli tek kayit, upsert mantigi.

### 4.9 Admin Panel Akisi

- Dashboard: kullanici/ilan/rezervasyon/revenue metrikleri.
- User management:
  - Listeleme filtre, detay, role/status update.
- Host applications:
  - Pending host list, reject -> user role customer + active.
- Listing moderation:
  - Approve -> `Active`, Reject -> `Rejected` + reason.
- Reports:
  - Tarih araligina gore current/previous period karsilastirma.
- City/Amenity CRUD.
- Admin alt moduller:
  - Destinations CRUD (slug unique, PostGIS point).
  - Blogs CRUD.
  - Media upload/delete.
- Kritik durum:
  - `AdminListingService` su an sadece Beach endpointlerini gercekliyor.
  - Pool/Yacht/DayTrip admin service metotlari `NotImplementedException`.

### 4.10 Blog Akisi

- Public endpointler publish edilmis bloglari doner.
- Slug detail okununca view count artar.
- Related posts: once category, yetmezse fallback published.
- Comment: authenticated user + bos olmayan text.
- Yetki notu:
  - Controller admin-only olsa da `BlogService.Update/Delete` authorId eslesmesi ariyor.
  - Pratikte admin, baska yazarin postunu duzenleyemez/silemez.

## 5) API Endpoint Envanteri (Tum Endpointler)

Toplam endpoint sayisi: 123

| Method | Path | Auth | Handler |
|---|---|---|---|
| DELETE | `/api/admin/beaches/{id}` | `Authorize(Policy = AuthorizationPolicies.AdminOnly)` | `AdminListings.DeleteBeach` |
| DELETE | `/api/admin/blogs/{id}` | `Authorize(Policy = AuthorizationPolicies.AdminOnly)` | `AdminBlogs.DeleteBlog` |
| DELETE | `/api/admin/destinations/{id}` | `Authorize(Policy = AuthorizationPolicies.AdminOnly)` | `AdminDestinations.DeleteDestination` |
| DELETE | `/api/admin/media/{filename}` | `Authorize(Policy = AuthorizationPolicies.AdminOnly)` | `AdminMedia.DeleteFile` |
| DELETE | `/api/blog/{id}` | `Authorize(Policy = AuthorizationPolicies.AdminOnly)` | `Blog.Delete` |
| DELETE | `/api/favorites/{venueid}` | `Authorize(Policy = AuthorizationPolicies.CustomerOnly)` | `Favorites.Remove` |
| DELETE | `/api/payment-methods/{id}` | `Authorize(Policy = AuthorizationPolicies.CustomerOnly)` | `PaymentMethods.Delete` |
| DELETE | `/api/reviews/{id}` | `Authorize(Policy = AuthorizationPolicies.CustomerOnly)` | `Reviews.Delete` |
| DELETE | `/api/users/me` | `Authorize` | `Users.DeleteAccount` |
| GET | `/api/admin/amenities` | `Authorize(Policy = AuthorizationPolicies.AdminOnly)` | `Admin.GetAmenities` |
| GET | `/api/admin/beaches/{id}` | `Authorize(Policy = AuthorizationPolicies.AdminOnly)` | `AdminListings.GetBeach` |
| GET | `/api/admin/beaches` | `Authorize(Policy = AuthorizationPolicies.AdminOnly)` | `AdminListings.GetBeaches` |
| GET | `/api/admin/blogs/{id}` | `Authorize(Policy = AuthorizationPolicies.AdminOnly)` | `AdminBlogs.GetBlog` |
| GET | `/api/admin/blogs` | `Authorize(Policy = AuthorizationPolicies.AdminOnly)` | `AdminBlogs.GetBlogs` |
| GET | `/api/admin/cities` | `Authorize(Policy = AuthorizationPolicies.AdminOnly)` | `Admin.GetCities` |
| GET | `/api/admin/dashboard` | `Authorize(Policy = AuthorizationPolicies.AdminOnly)` | `Admin.GetDashboard` |
| GET | `/api/admin/destinations/{id}` | `Authorize(Policy = AuthorizationPolicies.AdminOnly)` | `AdminDestinations.GetDestination` |
| GET | `/api/admin/destinations` | `Authorize(Policy = AuthorizationPolicies.AdminOnly)` | `AdminDestinations.GetDestinations` |
| GET | `/api/admin/host-applications` | `Authorize(Policy = AuthorizationPolicies.AdminOnly)` | `Admin.GetHostApplications` |
| GET | `/api/admin/listings` | `Authorize(Policy = AuthorizationPolicies.AdminOnly)` | `Admin.GetListings` |
| GET | `/api/admin/reports` | `Authorize(Policy = AuthorizationPolicies.AdminOnly)` | `Admin.GetReports` |
| GET | `/api/admin/users/{id}` | `Authorize(Policy = AuthorizationPolicies.AdminOnly)` | `Admin.GetUser` |
| GET | `/api/admin/users` | `Authorize(Policy = AuthorizationPolicies.AdminOnly)` | `Admin.GetUsers` |
| GET | `/api/blog/{slug}/comments` | `AllowAnonymous` | `Blog.GetComments` |
| GET | `/api/blog/{slug}/detail` | `AllowAnonymous` | `Blog.GetDetailBySlug` |
| GET | `/api/blog/{slug}/related` | `AllowAnonymous` | `Blog.GetRelated` |
| GET | `/api/blog/{slug}` | `AllowAnonymous` | `Blog.GetBySlug` |
| GET | `/api/blog` | `AllowAnonymous` | `Blog.GetPublished` |
| GET | `/api/boat-tours/day-trip/{slug}` | `Public` | `BoatTours.GetDayTripBySlug` |
| GET | `/api/boat-tours/yacht/{slug}` | `Public` | `BoatTours.GetYachtBySlug` |
| GET | `/api/boat-tours` | `Public` | `BoatTours.GetAll` |
| GET | `/api/destinations/{slug}/detail` | `Public` | `Destinations.GetPageDetailBySlug` |
| GET | `/api/destinations/{slug}` | `Public` | `Destinations.GetBySlug` |
| GET | `/api/destinations` | `Public` | `Destinations.GetAll` |
| GET | `/api/explore` | `Public` | `Explore.GetExploreData` |
| GET | `/api/favorites` | `Authorize(Policy = AuthorizationPolicies.CustomerOnly)` | `Favorites.GetAll` |
| GET | `/api/health/db` | `Public` | `Health.CheckDatabase` |
| GET | `/api/health` | `Public` | `Health.Get` |
| GET | `/api/host/analytics` | `Authorize(Policy = AuthorizationPolicies.HostOrAdmin)` | `Host.GetAnalytics` |
| GET | `/api/host/business-settings` | `Authorize(Policy = AuthorizationPolicies.HostOrAdmin)` | `Host.GetBusinessSettings` |
| GET | `/api/host/calendar` | `Authorize(Policy = AuthorizationPolicies.HostOrAdmin)` | `Host.GetCalendar` |
| GET | `/api/host/dashboard/stats` | `Authorize(Policy = AuthorizationPolicies.HostOrAdmin)` | `Host.GetDashboardStats` |
| GET | `/api/host/insights` | `Authorize(Policy = AuthorizationPolicies.HostOrAdmin)` | `Host.GetInsights` |
| GET | `/api/host/listings/{id:guid}` | `Authorize(Policy = AuthorizationPolicies.HostOrAdmin)` | `Host.GetListing` |
| GET | `/api/host/listings` | `Authorize(Policy = AuthorizationPolicies.HostOrAdmin)` | `Host.GetListings` |
| GET | `/api/host/reservations/recent` | `Authorize(Policy = AuthorizationPolicies.HostOrAdmin)` | `Host.GetRecentReservations` |
| GET | `/api/host/reservations` | `Authorize(Policy = AuthorizationPolicies.HostOrAdmin)` | `Host.GetReservations` |
| GET | `/api/listings/nearby` | `Public` | `Listings.SearchNearby` |
| GET | `/api/listings/type/{type}` | `Public` | `Listings.GetByType` |
| GET | `/api/listings/{id}` | `Public` | `Listings.GetById` |
| GET | `/api/listings` | `Public` | `Listings.GetAll` |
| GET | `/api/payment-methods` | `Authorize(Policy = AuthorizationPolicies.CustomerOnly)` | `PaymentMethods.GetAll` |
| GET | `/api/reservations/check-availability` | `AllowAnonymous` | `Reservations.CheckAvailability` |
| GET | `/api/reservations/{id}` | `Authorize(Policy = AuthorizationPolicies.CustomerOnly)` | `Reservations.GetById` |
| GET | `/api/reservations` | `Authorize(Policy = AuthorizationPolicies.CustomerOnly)` | `Reservations.GetAll` |
| GET | `/api/reviews/listing/{listingid}` | `AllowAnonymous` | `Reviews.GetListingReviews` |
| GET | `/api/reviews/{id}` | `AllowAnonymous` | `Reviews.GetById` |
| GET | `/api/search/listings` | `Public` | `Search.SearchCustomerListings` |
| GET | `/api/search/suggestions` | `Public` | `Search.GetSuggestions` |
| GET | `/api/spots/{slug}` | `Public` | `Spots.GetBySlug` |
| GET | `/api/users/me/dashboard` | `Authorize` | `Users.GetDashboard` |
| GET | `/api/users/me` | `Authorize` | `Users.GetProfile` |
| PATCH | `/api/host/listings/{id:guid}/status` | `Authorize(Policy = AuthorizationPolicies.HostOrAdmin)` | `Host.UpdateListingStatus` |
| PATCH | `/api/host/reservations/{id:guid}/status` | `Authorize(Policy = AuthorizationPolicies.HostOrAdmin)` | `Host.UpdateReservationStatus` |
| PATCH | `/api/payment-methods/{id}/default` | `Authorize(Policy = AuthorizationPolicies.CustomerOnly)` | `PaymentMethods.SetDefault` |
| PATCH | `/api/payment-methods/{id}` | `Authorize(Policy = AuthorizationPolicies.CustomerOnly)` | `PaymentMethods.Update` |
| POST | `/api/admin/amenities` | `Authorize(Policy = AuthorizationPolicies.AdminOnly)` | `Admin.CreateAmenity` |
| POST | `/api/admin/beaches` | `Authorize(Policy = AuthorizationPolicies.AdminOnly)` | `AdminListings.CreateBeach` |
| POST | `/api/admin/blogs` | `Authorize(Policy = AuthorizationPolicies.AdminOnly)` | `AdminBlogs.CreateBlog` |
| POST | `/api/admin/cities` | `Authorize(Policy = AuthorizationPolicies.AdminOnly)` | `Admin.CreateCity` |
| POST | `/api/admin/destinations` | `Authorize(Policy = AuthorizationPolicies.AdminOnly)` | `AdminDestinations.CreateDestination` |
| POST | `/api/admin/host-applications/{userid}/reject` | `Authorize(Policy = AuthorizationPolicies.AdminOnly)` | `Admin.RejectHostApplication` |
| POST | `/api/admin/listings/{id}/approve` | `Authorize(Policy = AuthorizationPolicies.AdminOnly)` | `Admin.ApproveListing` |
| POST | `/api/admin/listings/{id}/reject` | `Authorize(Policy = AuthorizationPolicies.AdminOnly)` | `Admin.RejectListing` |
| POST | `/api/admin/media/upload-multiple` | `Authorize(Policy = AuthorizationPolicies.AdminOnly)` | `AdminMedia.UploadFiles` |
| POST | `/api/admin/media/upload` | `Authorize(Policy = AuthorizationPolicies.AdminOnly)` | `AdminMedia.UploadFile` |
| POST | `/api/auth/forgot-password` | `Public` | `Auth.ForgotPassword` |
| POST | `/api/auth/login/admin` | `Public` | `Auth.LoginAdmin` |
| POST | `/api/auth/login/customer` | `Public` | `Auth.LoginCustomer` |
| POST | `/api/auth/login/host` | `Public` | `Auth.LoginHost` |
| POST | `/api/auth/login` | `Public` | `Auth.Login` |
| POST | `/api/auth/logout` | `Authorize` | `Auth.Logout` |
| POST | `/api/auth/refresh` | `Public` | `Auth.RefreshToken` |
| POST | `/api/auth/register` | `Public` | `Auth.Register` |
| POST | `/api/auth/reset-password` | `Public` | `Auth.ResetPassword` |
| POST | `/api/blog/{slug}/comments` | `Authorize` | `Blog.AddComment` |
| POST | `/api/blog` | `Authorize(Policy = AuthorizationPolicies.AdminOnly)` | `Blog.Create` |
| POST | `/api/favorites` | `Authorize(Policy = AuthorizationPolicies.CustomerOnly)` | `Favorites.Add` |
| POST | `/api/host/listings` | `Authorize(Policy = AuthorizationPolicies.HostOrAdmin)` | `Host.CreateListing` |
| POST | `/api/host/reservations/manual` | `Authorize(Policy = AuthorizationPolicies.HostOrAdmin)` | `Host.CreateManualReservation` |
| POST | `/api/listings/photos/upload` | `Authorize(Policy = AuthorizationPolicies.HostOrAdmin)` | `Listings.UploadPhotos` |
| POST | `/api/listings/{id}/publish` | `Authorize(Policy = AuthorizationPolicies.HostOrAdmin)` | `Listings.Publish` |
| POST | `/api/listings` | `Authorize(Policy = AuthorizationPolicies.HostOrAdmin)` | `Listings.Create` |
| POST | `/api/newsletter/subscribe` | `Public` | `Newsletter.Subscribe` |
| POST | `/api/newsletter/unsubscribe` | `Public` | `Newsletter.Unsubscribe` |
| POST | `/api/payment-methods` | `Authorize(Policy = AuthorizationPolicies.CustomerOnly)` | `PaymentMethods.Add` |
| POST | `/api/reservations/{id}/cancel` | `Authorize(Policy = AuthorizationPolicies.CustomerOnly)` | `Reservations.Cancel` |
| POST | `/api/reservations/{id}/check-in` | `Authorize(Policy = AuthorizationPolicies.CustomerOnly)` | `Reservations.CheckIn` |
| POST | `/api/reservations/{id}/payment-intent` | `Authorize(Policy = AuthorizationPolicies.CustomerOnly)` | `Reservations.CreatePaymentIntent` |
| POST | `/api/reservations/{id}/review` | `Authorize(Policy = AuthorizationPolicies.CustomerOnly)` | `Reservations.SubmitReview` |
| POST | `/api/reservations` | `Authorize(Policy = AuthorizationPolicies.CustomerOnly)` | `Reservations.Create` |
| POST | `/api/reviews/{id}/host-response` | `Authorize(Policy = AuthorizationPolicies.HostOnly)` | `Reviews.AddHostResponse` |
| POST | `/api/reviews` | `Authorize(Policy = AuthorizationPolicies.CustomerOnly)` | `Reviews.Create` |
| POST | `/api/search/listings` | `Public` | `Search.SearchListings` |
| POST | `/api/spots/{slug}/quote` | `Public` | `Spots.GetQuote` |
| POST | `/api/users/me/avatar` | `Authorize` | `Users.UpdateAvatarPost` |
| POST | `/api/users/me/change-email` | `Authorize` | `Users.ChangeEmail` |
| POST | `/api/users/me/password` | `Authorize` | `Users.ChangePasswordPost` |
| PUT | `/api/admin/beaches/{id}` | `Authorize(Policy = AuthorizationPolicies.AdminOnly)` | `AdminListings.UpdateBeach` |
| PUT | `/api/admin/blogs/{id}` | `Authorize(Policy = AuthorizationPolicies.AdminOnly)` | `AdminBlogs.UpdateBlog` |
| PUT | `/api/admin/destinations/{id}` | `Authorize(Policy = AuthorizationPolicies.AdminOnly)` | `AdminDestinations.UpdateDestination` |
| PUT | `/api/admin/users/{id}/role` | `Authorize(Policy = AuthorizationPolicies.AdminOnly)` | `Admin.UpdateUserRole` |
| PUT | `/api/admin/users/{id}/status` | `Authorize(Policy = AuthorizationPolicies.AdminOnly)` | `Admin.UpdateUserStatus` |
| PUT | `/api/blog/{id}` | `Authorize(Policy = AuthorizationPolicies.AdminOnly)` | `Blog.Update` |
| PUT | `/api/host/business-settings` | `Authorize(Policy = AuthorizationPolicies.HostOrAdmin)` | `Host.UpdateBusinessSettings` |
| PUT | `/api/host/calendar` | `Authorize(Policy = AuthorizationPolicies.HostOrAdmin)` | `Host.UpdateCalendar` |
| PUT | `/api/host/listings/{id:guid}` | `Authorize(Policy = AuthorizationPolicies.HostOrAdmin)` | `Host.UpdateListing` |
| PUT | `/api/payment-methods/{id}/default` | `Authorize(Policy = AuthorizationPolicies.CustomerOnly)` | `PaymentMethods.SetDefaultPut` |
| PUT | `/api/reservations/{id}` | `Authorize(Policy = AuthorizationPolicies.CustomerOnly)` | `Reservations.Update` |
| PUT | `/api/users/me/avatar` | `Authorize` | `Users.UpdateAvatar` |
| PUT | `/api/users/me/password` | `Authorize` | `Users.ChangePassword` |
| PUT | `/api/users/me/settings` | `Authorize` | `Users.UpdateSettings` |
| PUT | `/api/users/me` | `Authorize` | `Users.UpdateProfile` |

## 6) Veritabani Tasarimi

### 6.1 Teknoloji

- PostgreSQL
- PostGIS extension (`postgis`)
- EF Core code-first migration
- JSONB kolonlari ile coklu dil ve kompleks alt objeler

### 6.2 Ana Tablolar ve Amaclari

### Kimlik ve Hesap
- `Users`: kimlik, role, status, refresh token, ayarlar (JSONB).
- `UserProfiles`: ad/soyad/bio (JSONB), telefon, avatar.
- `PaymentMethods`: user kartlari, default secimi, provider token.
- `Notifications`: bildirim kayitlari.

### Icerik ve Listing
- `Listings`: ana ilan tablosu (tip, status, fiyat, lokasyon, i18n text).
- `ListingImages`: galeriler ve cover image.
- `Amenities`: amenity sozlugu (label JSONB).
- `ListingAmenities`: listing-amenity M:N baglantisi.
- `HostListingMetadata`: host panel ozel metadata (highlights, seating areas JSONB).
- `DailyPricings`: gunluk fiyat/musaitlik override.
- `AvailabilityBlocks`: tarih araligi bazli block.

### Rezervasyon ve Yorum
- `Reservations`: rezervasyon ana kaydi (fiyat, tarih, status, source).
- `ReservationPayments`: rezervasyona 1:1 odeme durumu.
- `Reviews`: rezervasyon/listing/guest baglantili yorumlar.
- `Favorites`: kullanici favorileri.

### Destination ve Blog
- `Cities`: sehir sozlugu.
- `Destinations`: destination landing datasi + postgis nokta + feature/tags JSONB.
- `BlogPosts`: blog icerikleri (title/desc/content JSONB).
- `BlogComments`: blog yorumlari.
- `NewsletterSubscribers`: newsletter abonelikleri.
- `HostBusinessSettings`: host panel isletme ayarlari (1:1 host).

### 6.3 Iliskiler (Ozet)

- `Users (1) - (1) UserProfiles`
- `Users (1) - (N) Listings` (host)
- `Users (1) - (N) Reservations` (guest)
- `Users (1) - (N) PaymentMethods`
- `Users (1) - (N) Favorites`
- `Users (1) - (N) Reviews`
- `Users (1) - (1) HostBusinessSettings`
- `Listings (1) - (N) ListingImages`
- `Listings (1) - (N) Reservations`
- `Listings (1) - (N) Reviews`
- `Listings (1) - (N) DailyPricings`
- `Listings (1) - (N) AvailabilityBlocks`
- `Listings (1) - (1) HostListingMetadata`
- `Listings (N) - (N) Amenities` via `ListingAmenities`
- `Reservations (1) - (1) ReservationPayments`
- `Reservations (1) - (1) Reviews`
- `BlogPosts (1) - (N) BlogComments`
- `Users (1) - (N) BlogComments`

## 7) Index, Constraint ve Performans Notlari

### 7.1 Onemli Indexler

- `Listings`: `HostId`, `Type`, `Status`, `Slug(unique)`, `City`, `IsFeatured`.
- `Reservations`: `GuestId`, `ListingId`, `Status`, `StartTime`, `ConfirmationNumber(unique)`, `Source`.
- `DailyPricing`: `(ListingId, Date)` composite.
- `Favorites`: `(UserId, VenueId, VenueType)` unique.
- `BlogPosts`: `Slug(unique)`, `IsPublished`, `IsFeatured`.
- `BlogComments`: `BlogPostId`, `CreatedAt`.
- `Cities`: `Country`.
- `HostBusinessSettings`: `HostId(unique)`.
- `HostListingMetadata`: `ListingId(unique)`.

### 7.2 JSONB/GIN

GIN index tanimli alanlar:
- `Listings.Title`
- `Listings.Description`
- `BlogPosts.Title`

### 7.3 Geo/PostGIS

- `Listings.Location`: `geography(point)` + GiST index.
- `Destinations.Location`: `geography(point)` + GiST index.

## 8) JSONB Kolonlari (Modelleme)

- `Users`: `NotificationSettings`, `LanguageSettings`, `PrivacySettings`
- `UserProfiles`: `FirstName`, `LastName`, `Bio`
- `Listings`: `Title`, `Description`, `Address`, `Conditions`, `Details`
- `Amenities`: `Label`, `ApplicableTo`
- `Reservations`: `SpecialRequests`, `Guests`, `Selections`, `PriceBreakdown`
- `Reviews`: `Categories`
- `BlogPosts`: `Title`, `Description`, `Content`, `Tags`
- `Cities`: `Name`
- `Destinations`: `Tags`, `Features`
- `HostListingMetadata`: `Highlights`, `SeatingAreas`

## 9) Enum ve State Modeli

- Listing:
  - `Pending`, `Active`, `Inactive`, `Rejected`, `Draft`, `PendingReview`
- Reservation:
  - `Pending`, `Confirmed`, `InProgress`, `Completed`, `Cancelled`, `NoShow`, `Rejected`
- Role:
  - `Admin`, `Host`, `Customer`
- UserStatus:
  - `Active`, `Banned`, `Pending`
- ReservationSource:
  - `Online`, `Phone`, `WalkIn`

## 10) Bilinen Bosluklar / Teknik Borc (Koddan Gozlenen)

- Auth reset/forgot flow gercek token-email akisi ile tamamlanmamis.
- User password degisimi TODO; hash update logic eksik.
- Logout token invalidation/blacklist yok.
- Cloudflare R2, email ve translation servisleri mock/TODO durumda.
- Admin listing tarafinda pool/yacht/day-trip metotlari implement degil.
- Blog update/delete admin-only endpoint olmasina ragmen service author eslesmesi istiyor.
- Customer search `distance` sort placeholder davranisinda.
- Host manual reservation olusturmada overlap kontrolu yok.
- Reservation overlap sorgusu `Cancelled` disindakileri cakisiyor sayiyor (`Rejected` de dahil).

## 11) Sonuc

Backend; customer + host + admin uc katmanli bir rezervasyon platformunu tek API'de topluyor.
Veritabani tasarimi JSONB ve PostGIS ile esnek/cok-dilli ve lokasyon odakli kurgulanmis.
Bununla birlikte birkac endpointte MVP/TODO izleri var ve uretim hardening asamasinda bu noktalarin tamamlanmasi gerekir.
