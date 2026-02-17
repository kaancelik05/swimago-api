# Host Panel API Frontend Consume Guide (AI-Friendly)

Bu doküman `apps/host-panel` tarafında mock servis yerine gerçek API kullanımına geçiş için hazırlanmıştır.

## 1) Genel Bilgiler

- Base URL: `https://<your-api-domain>/api/host`
- Auth: `Authorization: Bearer <JWT>`
- Yetki: `Host` veya `Admin` rolü gerekli
- Content-Type: `application/json`

### 1.1 Enum Değerleri (Frontend ile birebir)

```ts
type ListingType = 'beach' | 'pool' | 'yacht' | 'day-trip';
type ListingStatus = 'active' | 'pending' | 'inactive' | 'rejected';
type ReservationStatus = 'pending' | 'confirmed' | 'completed' | 'cancelled' | 'rejected';
type ReservationSource = 'online' | 'phone' | 'walk-in';
type Currency = 'USD' | 'EUR' | 'TRY' | 'GBP';
type AnalyticsPeriod = 'week' | 'month' | 'year';
```

### 1.2 Standart Hata Formatı

API hatalarında gövde:

```json
{ "error": "<mesaj>" }
```

Olası kodlar: `400`, `401`, `403`, `404`, `500`.

---

## 2) DTO Sözleşmeleri

### 2.1 Listing

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

interface HostListingsResponse {
  items: HostListing[];
  totalCount: number;
  page: number;
  pageSize: number;
}
```

### 2.2 Reservation

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

interface HostReservationsResponse {
  items: HostReservation[];
  totalCount: number;
  page: number;
  pageSize: number;
}
```

### 2.3 Dashboard/Insights

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
  descriptionParams?: Record<string, string | number | boolean>;
  level: 'info' | 'warning' | 'success';
}
```

### 2.4 Calendar

```ts
interface CalendarDay {
  date: string; // YYYY-MM-DD
  isAvailable: boolean;
  reservationCount: number;
  customPrice: number | null;
}
```

### 2.5 Analytics

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
```

### 2.6 Business Settings

```ts
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

## 3) Endpoint Listesi

## 3.1 Listings

### `GET /api/host/listings`
Query:
- `status=all|active|pending|inactive|rejected`
- `type=all|beach|pool|yacht|day-trip`
- `page` (default `1`)
- `pageSize` (default `20`)

Response: `HostListingsResponse`

### `GET /api/host/listings/{id}`
Response: `HostListing`

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

### `PUT /api/host/listings/{id}`
Request: `POST` ile aynı

Response: `200` + `HostListing`

### `PATCH /api/host/listings/{id}/status`
Request:

```json
{ "status": "inactive" }
```

Response: `204`

---

## 3.2 Dashboard

### `GET /api/host/dashboard/stats`
Response: `DashboardStats`

### `GET /api/host/reservations/recent?limit=7`
Response: `HostReservation[]`

### `GET /api/host/insights`
Response: `HostInsight[]`

---

## 3.3 Reservations

### `GET /api/host/reservations`
Query:
- `status=all|pending|confirmed|completed|cancelled|rejected`
- `source=all|online|phone|walk-in`
- `listingId=all|{guid}`
- `page` (default `1`)
- `pageSize` (default `20`)

Response: `HostReservationsResponse`

### `PATCH /api/host/reservations/{id}/status`
Request:

```json
{ "status": "confirmed" }
```

Response: `204`

### `POST /api/host/reservations/manual`
Request:

```json
{
  "listingId": "00000000-0000-0000-0000-000000000000",
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

Notlar:
- `source` sadece `phone` veya `walk-in` olabilir.
- status business settings’e göre otomatik atanır.

Response: `201` + `HostReservation`

---

## 3.4 Calendar

### `GET /api/host/calendar?listingId={guid}&month=2&year=2026`
Response: `CalendarDay[]`

### `PUT /api/host/calendar`
Request:

```json
{
  "listingId": "00000000-0000-0000-0000-000000000000",
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

---

## 3.5 Analytics

### `GET /api/host/analytics?period=month&listingId=all`
Query:
- `period=week|month|year`
- `listingId=all|{guid}`

Response: `HostAnalytics`

---

## 3.6 Business Settings

### `GET /api/host/business-settings`
Response: `BusinessSettings`

### `PUT /api/host/business-settings`
Request:

```json
{
  "autoConfirmReservations": false,
  "allowSameDayBookings": true,
  "minimumNoticeHours": 2,
  "cancellationWindowHours": 24,
  "dynamicPricingEnabled": false,
  "smartOverbookingProtection": true,
  "whatsappNotifications": false,
  "emailNotifications": true
}
```

Response: `204`

---

## 4) Frontend Entegrasyon Akışları

### 4.1 Dashboard
1. `GET /api/host/dashboard/stats`
2. `GET /api/host/reservations/recent?limit=7`
3. `GET /api/host/insights`
4. `GET /api/host/analytics?period=month&listingId=all`

### 4.2 Listings
1. `GET /api/host/listings`
2. `PATCH /api/host/listings/{id}/status`

### 4.3 Listing Editor
1. Edit: `GET /api/host/listings/{id}`
2. Create: `POST /api/host/listings`
3. Update: `PUT /api/host/listings/{id}`

### 4.4 Reservations
1. `GET /api/host/listings`
2. `GET /api/host/reservations?...`
3. `PATCH /api/host/reservations/{id}/status`

### 4.5 Quick Reservation
1. `GET /api/host/listings`
2. `POST /api/host/reservations/manual`
3. `GET /api/host/reservations?status=all&source=all&listingId=all`

### 4.6 Calendar
1. `GET /api/host/listings`
2. `GET /api/host/calendar?...`
3. `PUT /api/host/calendar`

### 4.7 Analytics
1. `GET /api/host/listings`
2. `GET /api/host/analytics?...`

### 4.8 Business Settings
1. `GET /api/host/business-settings`
2. `PUT /api/host/business-settings`

---

## 5) Örnek Fetch Katmanı

```ts
const API_BASE = '/api/host';

async function api<T>(path: string, init?: RequestInit): Promise<T> {
  const token = localStorage.getItem('accessToken');
  const res = await fetch(`${API_BASE}${path}`, {
    ...init,
    headers: {
      'Content-Type': 'application/json',
      Authorization: `Bearer ${token}`,
      ...(init?.headers || {})
    }
  });

  if (!res.ok) {
    let message = `HTTP ${res.status}`;
    try {
      const err = await res.json();
      if (err?.error) message = err.error;
    } catch {}
    throw new Error(message);
  }

  if (res.status === 204) return undefined as T;
  return res.json() as Promise<T>;
}
```

---

## 6) Backend Tarafında Eklenen DB Yapısı

Host panel için eklenenler:

1. `Reservations.Source` (integer enum)
- `Online=0`, `Phone=1`, `WalkIn=2`

2. `ReservationStatus.Rejected` enum değeri
- mevcut statülere ek olarak desteklendi

3. `HostBusinessSettings` tablosu
- host bazlı business setting alanları

4. `HostListingMetadata` tablosu
- `ListingId` bazlı `highlights`, `seatingAreas`, `availabilityNotes`

Migration dosyası:
- `src/Swimago.Infrastructure/Migrations/20260217042000_AddHostPanelApiEntities.cs`

