# Host Panel API Gereksinimleri (Mock'tan Gerçek API'ye Geçiş)

Bu doküman `apps/host-panel` kodu analiz edilerek, **mock data yerine gerçek veritabanı + gerçek API** ile çalışacak şekilde hazırlanmıştır.

Hedef: Bu dokümanı backend projesine verdiğinizde AI, host panelin tüm akışını doğru endpointlerle kodlayabilsin.

---

## 1) Kritik Not: Bu Doküman API-First'tür

- Mock servis davranışı (`HostDataService`) referans alınmıştır ama endpointler gerçek backend için tanımlanmıştır.
- Veriler kalıcı olmalıdır (PostgreSQL/Supabase).
- Tüm host endpointleri JWT + rol kontrolü (`Host`/`Admin`) ile korunmalıdır.
- Response formatları frontendin kolay entegrasyonu için doğrudan model döner (isteğe bağlı wrapper kullanılacaksa frontend mapper gerekir).

---

## 2) Mevcut Backend Guide ile Uyum Zorunlulukları

`backend_development_guide_final.md` ile host panel arasında şu şema farkları var:

1. `ListingType`
- Host panel beklediği değerler: `beach | pool | yacht | day-trip`
- Guide'daki değerler: `Beach | Pool | BoatTour`
- Çözüm: Ya enumu genişletin (`Yacht`, `DayTrip`) ya da `BoatTour + subType` map'i ekleyin.

2. `ReservationStatus`
- Host panel: `pending | confirmed | completed | cancelled | rejected`
- Guide: `Pending | Confirmed | InProgress | Completed | Cancelled | NoShow`
- Çözüm: `Rejected` statüsünü ekleyin veya backendde map katmanı oluşturun.

3. `ReservationSource`
- Host panel: `online | phone | walk-in`
- Guide modelinde yok.
- Çözüm: rezervasyona `Source` alanı ekleyin.

4. Host listing kartı alanları
- Frontend doğrudan şu alanları bekler: `basePrice`, `currency`, `capacity`, `seatingAreas`, `reservationCount`, `revenue`.
- Bunlar DTO seviyesinde mutlaka üretilmelidir (ham entity farklı olabilir).

---

## 3) Tam Çalışma İçin Zorunlu Endpoint Listesi

> Bu liste, host-panelin mevcut ekranlarını API ile çalıştırmak için minimum zorunlu settir.

| # | Method | Endpoint | Amaç |
|---|---|---|---|
| 1 | `GET` | `/api/host/listings` | Listing listeleme |
| 2 | `GET` | `/api/host/listings/{id}` | Listing detay (edit) |
| 3 | `POST` | `/api/host/listings` | Listing oluşturma |
| 4 | `PUT` | `/api/host/listings/{id}` | Listing güncelleme |
| 5 | `PATCH` | `/api/host/listings/{id}/status` | Listing aktif/pasif/diğer status |
| 6 | `GET` | `/api/host/dashboard/stats` | Dashboard üst metrikler |
| 7 | `GET` | `/api/host/reservations/recent?limit=7` | Dashboard son rezervasyonlar |
| 8 | `GET` | `/api/host/insights` | Dashboard insight kartları |
| 9 | `GET` | `/api/host/reservations` | Rezervasyon ekranı + filtre |
| 10 | `PATCH` | `/api/host/reservations/{id}/status` | Rezervasyon status güncelleme |
| 11 | `POST` | `/api/host/reservations/manual` | Quick reservation oluşturma |
| 12 | `GET` | `/api/host/calendar?listingId=...&month=...&year=...` | Takvim ay görünümü |
| 13 | `PUT` | `/api/host/calendar` | Takvim gün override güncelleme |
| 14 | `GET` | `/api/host/analytics?period=...&listingId=...` | Analytics verisi |
| 15 | `GET` | `/api/host/business-settings` | İşletme ayarları getir |
| 16 | `PUT` | `/api/host/business-settings` | İşletme ayarları kaydet |

### Production için kuvvetle önerilen ek endpointler

- `POST /api/auth/login`
- `POST /api/auth/refresh`
- `GET /api/auth/me`

---

## 4) DTO Sözleşmeleri (Frontend Uyumlu)

```ts
type ListingType = 'beach' | 'pool' | 'yacht' | 'day-trip';
type ListingStatus = 'active' | 'pending' | 'inactive' | 'rejected';
type ReservationStatus = 'pending' | 'confirmed' | 'completed' | 'cancelled' | 'rejected';
type ReservationSource = 'online' | 'phone' | 'walk-in';
type Currency = 'USD' | 'EUR' | 'TRY' | 'GBP';
```

```ts
interface SeatingArea {
  id: string;
  name: string;
  capacity: number;
  priceMultiplier: number;
  isVip: boolean;
  minSpend?: number;
}

interface HostListing {
  id: string;
  name: string;
  slug: string;
  type: ListingType;
  city: string;
  imageUrl: string;
  status: ListingStatus;
  rating: number;
  reviewCount: number;
  reservationCount: number;
  revenue: number;
  basePrice: number;
  currency: Currency;
  capacity: number;
  seatingAreas: SeatingArea[];
  highlights: string[];
  availabilityNotes?: string;
}
```

```ts
interface HostReservation {
  id: string;
  listingId: string;
  listingName: string;
  guestName: string;
  guestPhone: string;
  date: string;      // YYYY-MM-DD
  time: string;      // HH:mm
  guests: number;
  totalAmount: number;
  status: ReservationStatus;
  source: ReservationSource;
  specialRequests?: string;
  createdAt: string; // ISO
}
```

```ts
interface DashboardStats {
  totalListings: number;
  activeListings: number;
  pendingReservations: number;
  upcomingReservations: number;
  totalRevenue: number;
  monthlyRevenue: number;
}

interface HostInsight {
  id: string;
  titleKey: string;
  descriptionKey: string;
  descriptionParams?: Record<string, string | number>;
  level: 'info' | 'warning' | 'success';
}

interface CalendarDay {
  date: string;
  isAvailable: boolean;
  reservationCount: number;
  customPrice: number | null;
}
```

```ts
interface RevenuePoint { label: string; amount: number; }
interface TopListingMetric {
  listingId: string;
  name: string;
  revenue: number;
  bookings: number;
  occupancyRate: number;
}

interface SourceBreakdown {
  source: ReservationSource;
  count: number;
}

interface HostAnalytics {
  totalRevenue: number;
  revenueTrendPercent: number;
  totalReservations: number;
  reservationTrendPercent: number;
  averageRating: number;
  reviewCount: number;
  occupancyRate: number;
  revenueSeries: RevenuePoint[];
  topListings: TopListingMetric[];
  sourceBreakdown: SourceBreakdown[];
  noShowRate: number;
  cancellationRate: number;
}

interface BusinessSettings {
  autoConfirmReservations: boolean;
  allowSameDayBookings: boolean;
  minimumNoticeHours: number;
  cancellationWindowHours: number;
  dynamicPricingEnabled: boolean;
  smartOverbookingProtection: boolean;
  whatsappNotifications: boolean;
  emailNotifications: boolean;
}
```

---

## 5) Endpoint Bazlı Request/Response ve İş Mantığı

## 5.1 Listings

### `GET /api/host/listings`

Query (opsiyonel):
- `status`, `type`, `page`, `pageSize`

Response: `HostListing[]` (veya paginate gerekiyorsa `items[] + totalCount`)

İş mantığı:
- Sadece giriş yapan hostun listingleri dönmeli.
- `reservationCount` ve `revenue` alanları DB'den aggregate edilip DTO'ya yazılmalı.

### `GET /api/host/listings/{id}`

Response: `HostListing`

Hatalar:
- `404` listing yok
- `403` listing başka hosta ait

### `POST /api/host/listings`

Request:

```json
{
  "name": "Bodrum Azure Beach Club",
  "type": "beach",
  "city": "Bodrum",
  "status": "active",
  "basePrice": 85,
  "currency": "USD",
  "capacity": 120,
  "highlights": ["Blue Flag", "DJ Nights"],
  "seatingAreas": [
    {
      "id": "seat_1",
      "name": "VIP",
      "capacity": 10,
      "priceMultiplier": 2.0,
      "isVip": true,
      "minSpend": 300
    }
  ],
  "availabilityNotes": "Yoğun sezon",
  "imageUrl": "https://..."
}
```

Response: `201` + `HostListing`

İş mantığı:
- `slug` backendde üretilmeli.
- `highlights`: trim + unique + boşları at.
- `seatingAreas`: capacity min 1; priceMultiplier <= 0 ise 1'e çek.
- `imageUrl` boşsa fallback görsel.

### `PUT /api/host/listings/{id}`

Request: create ile aynı

Response: `HostListing`

### `PATCH /api/host/listings/{id}/status`

Request:

```json
{ "status": "inactive" }
```

Response: `204` (veya güncel listing)

---

## 5.2 Dashboard

### `GET /api/host/dashboard/stats`

Response: `DashboardStats`

Hesaplama kuralları:
- `monthlyRevenue`: bu ay `confirmed|completed`
- `totalRevenue`: tüm zaman `confirmed|completed`
- `pendingReservations`: `pending`
- `upcomingReservations`: `date >= today` ve `cancelled|rejected` olmayanlar

### `GET /api/host/reservations/recent?limit=7`

Response: `HostReservation[]`

Kural:
- `createdAt DESC`

### `GET /api/host/insights`

Response: `HostInsight[]`

Kural:
- `waitlist`: pending sayısına göre warning/info
- `offline-demand`: `(phone + walk-in) / total`
- `churn`: cancellation rate threshold (örn. >12 warning)

---

## 5.3 Reservations

### `GET /api/host/reservations`

Query:
- `status=all|pending|confirmed|completed|cancelled|rejected`
- `source=all|online|phone|walk-in`
- `listingId=all|{id}`
- (opsiyonel) `page`, `pageSize`

Response: `HostReservation[]`

Kural:
- `date + time DESC` sıralama.

### `PATCH /api/host/reservations/{id}/status`

Request:

```json
{ "status": "confirmed" }
```

Response: `204`

Kural:
- Status değişiminden sonra listing KPI'ları yeniden hesaplanmalı (`reservationCount`, `revenue`).

### `POST /api/host/reservations/manual`

Request:

```json
{
  "listingId": "lst_beach_1",
  "guestName": "Mert Yılmaz",
  "guestPhone": "+90 532 000 00 00",
  "date": "2026-02-22",
  "time": "12:00",
  "guests": 4,
  "totalAmount": 390,
  "source": "phone",
  "specialRequests": "Gölge alan"
}
```

Response: `201` + `HostReservation`

Kural:
- `source` sadece `phone|walk-in` kabul edilmeli.
- Status otomatik atanmalı:
  - `autoConfirmReservations=true` -> `confirmed`
  - değilse -> `pending`
- `createdAt` backend UTC zamanı.

---

## 5.4 Calendar

### `GET /api/host/calendar?listingId=...&month=2&year=2026`

Response: `CalendarDay[]`

Kural:
- İstenen ayın tüm günleri dönmeli.
- `reservationCount` gün bazlı hesaplanmalı.
- `isAvailable` override varsa override'dan; yoksa kapasite kuralından üretilmeli.

### `PUT /api/host/calendar`

Request:

```json
{
  "listingId": "lst_beach_1",
  "updates": [
    {
      "date": "2026-02-22",
      "isAvailable": false,
      "customPrice": 120
    }
  ]
}
```

Response: `204`

Kural:
- Upsert (varsa update, yoksa insert) mantığı kullanılmalı.

---

## 5.5 Analytics

### `GET /api/host/analytics?period=month&listingId=all`

Query:
- `period=week|month|year`
- `listingId=all|{id}`

Response: `HostAnalytics`

Kural:
- Filtre period + listing'e göre uygulanmalı.
- `totalRevenue`: `confirmed|completed` toplamı.
- `totalReservations`: `confirmed|completed` adedi.
- `cancellationRate = cancelled / totalFiltered * 100`.
- `sourceBreakdown`: her zaman 3 source dönmeli (`online`, `phone`, `walk-in`) 0 olsa da.
- `revenueTrendPercent`, `reservationTrendPercent` mock'taki gibi sabit değil, önceki dönemle kıyasla hesaplanmalı.

---

## 5.6 Business Settings

### `GET /api/host/business-settings`

Response: `BusinessSettings`

### `PUT /api/host/business-settings`

Request: `BusinessSettings`

Response: `204` (veya güncel `BusinessSettings`)

---

## 6) Ekran Bazlı Çağrı Akışları (Gerçek API Modu)

### Dashboard `/dashboard`

1. `GET /api/host/dashboard/stats`
2. `GET /api/host/reservations/recent?limit=7`
3. `GET /api/host/insights`
4. `GET /api/host/analytics?period=month&listingId=all`

### Listings `/listings`

1. `GET /api/host/listings`
2. `PATCH /api/host/listings/{id}/status`

### Listing Editor `/listings/new` & `/listings/:id/edit`

1. Edit modunda `GET /api/host/listings/{id}`
2. Create `POST /api/host/listings`
3. Update `PUT /api/host/listings/{id}`

### Reservations `/reservations`

1. `GET /api/host/listings`
2. `GET /api/host/reservations?...`
3. `PATCH /api/host/reservations/{id}/status`

### Quick Reservation `/quick-reservation`

1. `GET /api/host/listings`
2. `POST /api/host/reservations/manual`
3. `GET /api/host/reservations?status=all&source=all&listingId=all`

### Calendar `/calendar`

1. `GET /api/host/listings`
2. `GET /api/host/calendar?...`
3. `PUT /api/host/calendar`

### Analytics `/analytics`

1. `GET /api/host/listings`
2. `GET /api/host/analytics?...`

### Business Settings `/business-settings`

1. `GET /api/host/business-settings`
2. `PUT /api/host/business-settings`

---

## 7) Hata Modeli ve HTTP Kodları

Önerilen error response:

```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "capacity must be >= 1",
    "details": {
      "capacity": ["Must be at least 1"]
    }
  }
}
```

Kodlar:
- `400` validation
- `401` auth
- `403` yetki
- `404` kaynak yok
- `409` iş kuralı çakışması
- `500` beklenmeyen hata

---

## 8) Backend Implementasyon Checklist (AI İçin)

1. Host endpointlerinde kullanıcı scope'u zorunlu (`hostId` filtreleme).
2. DTO'lar frontend model isimleriyle birebir hizalanmalı.
3. `rejected`, `walk-in`, `day-trip`, `yacht` değerleri backendde açıkça desteklenmeli (veya map katmanı eklenmeli).
4. Analytics trendleri statik değil dönemsel hesaplanmalı.
5. Quick reservation yaratımında status, business settings'e göre otomatik atanmalı.
6. Takvimde ayın tüm günleri dönmeli; eksik gün bırakılmamalı.
7. Tarih/saat formatları bozulmamalı: `date=YYYY-MM-DD`, `time=HH:mm`, `createdAt=ISO`.
8. Para alanları numeric dönmeli, string format dönülmemeli.

