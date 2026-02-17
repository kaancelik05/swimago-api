# Customer App API Gereksinimleri (Dummy Data -> Gercek API)

- Guncelleme tarihi: `2026-02-17`
- Kapsam: `apps/customer`
- Hedef: Customer frontend'te mock/hardcoded veri ile calisan bolumleri gercek API ile calistirmak.

Bu dokuman, customer uygulamasi kodu analiz edilerek backend ekibinin dogrudan endpoint ve DTO ekleyebilmesi icin hazirlandi.

---

## 1) Dummy Data ile Calisan Bolumler (Kod Referanslari)

| Modul | Kod referansi | Mevcut durum | API ihtiyaci |
|---|---|---|---|
| Explore list + map | `apps/customer/src/app/pages/explore/explore.component.ts:194`, `apps/customer/src/app/pages/explore/explore.component.ts:347` | Tum beach/pool kartlari ve marker verisi hardcoded | Arama + liste + marker endpointleri |
| Destination type ayrimi | `apps/customer/src/app/pages/destinations/destinations.component.ts:106`, `apps/customer/src/app/pages/landing/components/destinations-section/destinations-section.component.ts:58` | Destination tipi API'den gelmedigi icin frontend tahmin ediyor | Destination listesinde `type` alani zorunlu |
| Beach detail | `apps/customer/src/app/pages/beach-detail/beach-detail.component.ts:38` | Tum detay (galeri, kosullar, yorum, fiyat) static | Spot detail + reviews + quote + rezervasyon |
| Pool detail | `apps/customer/src/app/pages/pool-detail/pool-detail.component.ts:38` | Tum detay static | Spot detail + reviews + quote + rezervasyon |
| Beach destination detail | `apps/customer/src/app/pages/beach-destination-detail/beach-destination-detail.component.ts:31` | Hero/overview/features/spots static | Destination detail endpointi |
| Pool destination detail | `apps/customer/src/app/pages/pool-destination-detail/pool-destination-detail.component.ts:31` | Hero/overview/features/spots static | Destination detail endpointi |
| Yacht tour detail | `apps/customer/src/app/pages/boat-tours/yacht-tour-detail/yacht-tour-detail.component.ts:33` | Slug var ama API cagrisi yok, veri static | Yacht detail endpointi |
| Day-trip detail | `apps/customer/src/app/pages/boat-tours/day-trip-detail/day-trip-detail.component.ts:37` | Slug var ama API cagrisi yok, veri static | Day-trip detail endpointi |
| Blog detail + yorumlar | `apps/customer/src/app/pages/blog-detail/blog-detail.component.ts:63`, `apps/customer/src/app/pages/blog-detail/blog-detail.component.ts:133` | Makale icerigi, ilgili yazilar, yorumlar static | Blog detail + related + comments endpointleri |
| Newsletter aksiyonu | `apps/customer/src/app/pages/blog/blog.component.ts:129` | UI var, API entegrasyonu TODO | Newsletter subscribe endpointi |
| Boat tour favorite toggle | `apps/customer/src/app/pages/boat-tours/boat-tours.component.ts:115` | UI toggle var, persistence yok | Favorites add/remove |
| Profile dashboard | `apps/customer/src/app/pages/user-profile/user-profile.component.ts:41`, `apps/customer/src/app/pages/user-profile/user-profile.component.ts:69` | Stats/upcoming/favorites static | Profile summary endpointi |
| My Reservations | `apps/customer/src/app/pages/user-profile/pages/my-reservations/my-reservations.component.ts:48` | Liste ve aksiyonlar static/console | Reservations list + action endpointleri |
| My Favorites | `apps/customer/src/app/pages/user-profile/pages/my-favorites/my-favorites.component.ts:61` | Liste static | Favorites list/filter/sort endpointi |
| Payment Methods | `apps/customer/src/app/pages/user-profile/pages/payment-methods/payment-methods.component.ts:42` | Kartlar static | Payment methods CRUD endpointleri |
| Account Settings save | `apps/customer/src/app/pages/user-profile/pages/account-settings/account-settings.component.ts:156` | Save islemi simulate ediliyor | Users profile/settings/avatar/password/email endpointleri |
| Become host create listing | `apps/customer/src/app/pages/become-host/pages/create-listing/create-listing.component.ts:124` | Publish sadece console+alert | Listing draft/create + photo upload + publish |

---

## 2) Sozlesme Kurallari

- Base URL: `/api`
- Auth: Private endpointlerde `Authorization: Bearer <jwt>`
- Rol: Customer ekranlari icin `Role=Customer` (listing publish akisi host rolune gecis kuraliyla)
- Tarih formati: ISO-8601 UTC (`2026-02-17T10:00:00Z`)
- Para birimi: ISO-4217 (`USD`, `EUR`, `TRY`)
- Pagination standardi:

```json
{
  "items": [],
  "page": 1,
  "pageSize": 10,
  "totalCount": 0,
  "totalPages": 0,
  "hasPrevious": false,
  "hasNext": false
}
```

- Hata formati (onerilen):

```json
{
  "statusCode": 400,
  "message": "Validation failed",
  "validationErrors": {
    "field": ["error message"]
  }
}
```

---

## 3) Endpoint Gereksinimleri (Backend'e Eklenmesi / Genisletilmesi Gerekenler)

## 3.1 Explore + Search

### 3.1.1 `GET /api/Search/listings` (Mevcut endpoint, response genisletme gerekli)

Amac: Explore list kartlari ve landing/search sonuc listeleri.

Query:
- `viewType`: `Beach|Pool`
- `searchTerm`: string
- `city`: string
- `dateFrom`: ISO date
- `dateTo`: ISO date
- `guests`: number
- `minPrice`, `maxPrice`: number
- `amenities`: `a,b,c`
- `sortBy`: `recommended|price|rating|distance`
- `sortOrder`: `asc|desc`
- `page`, `pageSize`

Response:

```json
{
  "items": [
    {
      "id": "spot_123",
      "slug": "kaputas-beach-kas",
      "title": "Kaputas Beach",
      "imageUrl": "https://...",
      "imageAlt": "Kaputas Beach",
      "type": "Beach",
      "typeLabel": "Public",
      "typeIcon": "public",
      "locationText": "Kas, Turkey",
      "distanceKm": 12.0,
      "rating": 4.96,
      "reviewCount": 512,
      "price": 8,
      "currency": "USD",
      "priceUnit": "person",
      "displayPrice": "$8",
      "displayPriceUnit": "/ person",
      "displayTotal": null,
      "badges": ["Blue Flag"],
      "tags": ["Sandy", "Scenic"],
      "isFavorite": false,
      "latitude": 36.23,
      "longitude": 29.45
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalCount": 248,
  "totalPages": 13,
  "hasPrevious": false,
  "hasNext": true
}
```

### 3.1.2 `GET /api/Explore` (Mevcut endpoint, marker ihtiyacina gore)

Amac: Harita markerlari.

Query:
- `neLat`, `neLng`, `swLat`, `swLng`
- `type`: `Beach|Pool`
- `dateFrom`, `dateTo`, `guests` (opsiyonel; fiyat/uygunluk icin)

Response:

```json
{
  "markers": [
    {
      "id": "spot_123",
      "slug": "kaputas-beach-kas",
      "name": "Kaputas Beach",
      "venueType": "Beach",
      "latitude": 36.23,
      "longitude": 29.45,
      "price": 8,
      "currency": "USD",
      "rating": 4.96,
      "thumbnailUrl": "https://..."
    }
  ],
  "bounds": {
    "northEastLat": 41.4,
    "northEastLng": 30.2,
    "southWestLat": 36.0,
    "southWestLng": 27.0
  }
}
```

### 3.1.3 `GET /api/Search/suggestions` (Mevcut endpoint)

Query:
- `term`: string

Response:

```json
[
  "Kas, Antalya",
  "Kalkan, Antalya",
  "Kaputas Beach"
]
```

---

## 3.2 Destinations

### 3.2.1 `GET /api/Destinations` (Mevcut endpoint, alan ve filtre genisletme gerekli)

Su an frontend destination tipini API'den alamadigi icin tahmin ediyor.

Query:
- `featured`: bool
- `type`: `Beach|Pool`
- `search`: string
- `page`, `pageSize`

Response:

```json
{
  "destinations": [
    {
      "id": "dest_1",
      "slug": "santa-monica",
      "name": "Santa Monica",
      "country": "USA",
      "imageUrl": "https://...",
      "spotCount": 12,
      "type": "Beach",
      "isFeatured": true,
      "minPrice": 15,
      "maxPrice": 120,
      "averageRating": 4.8
    }
  ],
  "totalCount": 64
}
```

### 3.2.2 `GET /api/Destinations/{slug}/detail` (Yeni endpoint onerisi)

Amac: `/beaches/:slug` ve `/pools/:slug` detail sayfalarindaki hero+overview+features+spots.

Response:

```json
{
  "id": "dest_1",
  "slug": "santa-monica",
  "type": "Beach",
  "hero": {
    "title": "Santa Monica",
    "subtitle": "Experience the perfect blend...",
    "location": "California, USA",
    "imageUrl": "https://...",
    "spotCount": 12
  },
  "overview": {
    "description": "...",
    "avgWaterTemp": "68F",
    "sunnyDaysPerYear": 280,
    "mapImageUrl": "https://..."
  },
  "features": [
    { "icon": "wb_sunny", "title": "Perfect Weather", "description": "..." }
  ],
  "spots": [
    {
      "id": "spot_1",
      "slug": "sunset-beach-club",
      "name": "Sunset Beach Club",
      "location": "North Pier, Santa Monica",
      "imageUrl": "https://...",
      "rating": 4.9,
      "reviewCount": 128,
      "price": 45,
      "currency": "USD",
      "priceUnit": "day"
    }
  ],
  "cta": {
    "title": "Not finding what you're looking for?",
    "description": "...",
    "buttonText": "Explore Nearby Beaches",
    "backgroundImageUrl": "https://..."
  }
}
```

---

## 3.3 Spot Detail (Beach/Pool)

### 3.3.1 `GET /api/Spots/{slug}` (Mevcut endpoint, full-detail response gerekli)

Amac: `/beach/:slug` ve `/pool/:slug` ekranlarindaki tum bloklar.

Response:

```json
{
  "id": "spot_1",
  "slug": "sunny-beach-cove",
  "type": "Beach",
  "header": {
    "title": "Sunny Beach Cove",
    "rating": 4.8,
    "reviewCount": 124,
    "location": "Crete, Greece",
    "breadcrumbs": [
      { "label": "Greece", "link": "/explore" },
      { "label": "Crete", "link": "/explore" },
      { "label": "Sunny Beach Cove" }
    ]
  },
  "gallery": [
    { "url": "https://...", "alt": "...", "isPrimary": true }
  ],
  "conditions": [
    { "icon": "air", "label": "Wind Speed", "value": "12 km/h" }
  ],
  "description": "...",
  "amenities": [
    { "icon": "wifi", "label": "Free Wi-Fi", "available": true }
  ],
  "location": {
    "name": "Sunny Beach Cove",
    "subtitle": "Crete, Greece",
    "latitude": 35.1,
    "longitude": 24.9,
    "mapImageUrl": "https://..."
  },
  "reviewsPreview": {
    "overallRating": 4.8,
    "totalReviews": 124,
    "breakdown": [
      { "stars": 5, "percentage": 73 }
    ],
    "categories": [
      { "label": "Cleanliness", "score": 4.9 }
    ],
    "reviews": [
      {
        "id": "r1",
        "avatarUrl": "https://...",
        "name": "Sarah",
        "date": "2026-02-01",
        "text": "..."
      }
    ]
  },
  "bookingDefaults": {
    "price": 25,
    "currency": "USD",
    "priceUnit": "day",
    "defaultDate": "2026-02-20",
    "defaultGuests": 2,
    "lineItems": [
      { "label": "Ticket", "amount": 50 },
      { "label": "Service fee", "amount": 5 }
    ],
    "total": 55,
    "rareFindMessage": "Usually fully booked"
  }
}
```

### 3.3.2 `POST /api/Spots/{slug}/quote` (Yeni endpoint onerisi)

Amac: Booking card'da tarih/misafir degisince fiyat kirilimini dinamik almak.

Request:

```json
{
  "date": "2026-02-20",
  "guests": {
    "adults": 2,
    "children": 0
  },
  "selections": {
    "selectedAmenities": ["sunbed"]
  }
}
```

Response:

```json
{
  "currency": "USD",
  "lineItems": [
    { "label": "Entry fee (2x)", "amount": 50 },
    { "label": "Sunbed", "amount": 10 },
    { "label": "Service fee", "amount": 6 }
  ],
  "total": 66,
  "isAvailable": true,
  "unavailableReason": null
}
```

### 3.3.3 Rezervasyon/Favori baglantilari

- `POST /api/Reservations` (mevcut)
- `GET /api/Reservations/check-availability` (mevcut)
- `POST /api/Favorites` (mevcut)
- `DELETE /api/Favorites/{venueId}` (mevcut)

---

## 3.4 Boat Tours

### 3.4.1 `GET /api/boat-tours/yacht/{slug}` (Mevcut endpoint, response zenginlestirme)

Response en az su bloklari kapsamalidir:

```json
{
  "id": "y_1",
  "slug": "azure-horizon",
  "title": "The Azure Horizon",
  "location": "Bodrum, Turkey",
  "rating": 4.98,
  "reviewCount": 124,
  "isSuperhost": true,
  "gallery": [{ "url": "https://...", "alt": "..." }],
  "specs": [
    { "icon": "directions_boat", "label": "Type", "value": "Motor Yacht" }
  ],
  "about": ["paragraph1", "paragraph2"],
  "features": [
    { "icon": "wifi", "label": "High-speed Wi-Fi" }
  ],
  "accommodationOptions": [
    { "imageUrl": "https://...", "name": "Master Suite", "description": "..." }
  ],
  "cateringItems": [
    { "icon": "restaurant_menu", "text": "Breakfast included" }
  ],
  "activityItems": [
    { "icon": "scuba_diving", "text": "Snorkeling" }
  ],
  "cruisingRoute": {
    "name": "Blue Cave Express",
    "stops": "Bodrum Marina - Black Island",
    "totalRoutes": 4,
    "mapImageUrl": "https://..."
  },
  "bookingDefaults": {
    "price": 1200,
    "priceUnit": "day",
    "showTripTypeToggle": true,
    "lineItems": [
      { "label": "Yacht rental", "amount": 1200 },
      { "label": "Service fee", "amount": 150 }
    ],
    "total": 1350
  }
}
```

### 3.4.2 `GET /api/boat-tours/day-trip/{slug}` (Mevcut endpoint, response zenginlestirme)

Response en az:

```json
{
  "id": "d_1",
  "slug": "turquoise-coast-day-cruise",
  "title": "Turquoise Coast Luxury Day Cruise",
  "location": "Kas, Turkey",
  "rating": 4.96,
  "reviewCount": 124,
  "gallery": [{ "url": "https://...", "alt": "..." }],
  "infoBadges": [
    { "icon": "schedule", "label": "Duration", "value": "7 Hours" }
  ],
  "host": {
    "avatarUrl": "https://...",
    "name": "Captain Murat",
    "title": "Professional Captain",
    "experience": "10 years"
  },
  "description": ["paragraph1", "paragraph2"],
  "routeStops": [
    { "time": "10:00", "icon": "sailing", "title": "Departure", "description": "..." }
  ],
  "amenities": [
    { "icon": "wifi", "label": "Wi-Fi" }
  ],
  "foodItems": [
    { "icon": "check_circle", "iconColor": "green", "text": "Lunch included" }
  ],
  "activityTags": [{ "label": "Snorkeling" }],
  "musicInfo": { "text": "Bluetooth available" },
  "bookingDefaults": {
    "price": 650,
    "priceUnit": "tour",
    "lineItems": [
      { "label": "Private Charter Base", "amount": 650 }
    ],
    "total": 650
  }
}
```

Not: Boat tour list ekranindaki favorileme icin `POST/DELETE /api/Favorites` endpointleri kullanilacak.

---

## 3.5 Blog Detail + Etkilesim

### 3.5.1 `GET /api/Blog/{slug}` (Mevcut endpoint, detail mode genisletme) veya `GET /api/Blog/{slug}/detail` (Yeni)

Response:

```json
{
  "id": "b_1",
  "slug": "top-10-hidden-beaches-sardinia",
  "title": "Top 10 Hidden Beaches in Sardinia",
  "description": "...",
  "heroImageUrl": "https://...",
  "category": "Travel Tips",
  "tags": ["Sardinia", "BeachLife"],
  "readTime": 5,
  "publishedAt": "2026-02-10T09:00:00Z",
  "author": {
    "name": "Alex Marlin",
    "bio": "...",
    "avatarUrl": "https://..."
  },
  "tableOfContents": [
    { "id": "intro", "title": "Introduction" }
  ],
  "contentBlocks": [
    { "type": "paragraph", "text": "..." },
    { "type": "heading", "id": "cala-goloritz", "text": "Cala Goloritze" },
    { "type": "image", "imageUrl": "https://...", "caption": "..." },
    { "type": "quote", "text": "...", "author": "Traveler Review" }
  ]
}
```

### 3.5.2 `GET /api/Blog/{slug}/related?limit=3` (Yeni)

```json
{
  "items": [
    {
      "slug": "best-sunset-spots-bali",
      "title": "Best Sunset Spots in Bali",
      "description": "...",
      "imageUrl": "https://...",
      "category": "Destinations"
    }
  ]
}
```

### 3.5.3 Yorumlar

- `GET /api/Blog/{slug}/comments?page=1&pageSize=20`
- `POST /api/Blog/{slug}/comments`

POST request:

```json
{
  "text": "Great article"
}
```

POST response:

```json
{
  "id": "c_101",
  "author": {
    "name": "Customer Name",
    "avatarUrl": "https://..."
  },
  "text": "Great article",
  "createdAt": "2026-02-17T10:20:00Z"
}
```

### 3.5.4 Newsletter

- `POST /api/Newsletter/subscribe` (mevcut)

Request:

```json
{
  "email": "user@example.com"
}
```

Response:

```json
{
  "message": "Subscribed",
  "email": "user@example.com",
  "isSubscribed": true
}
```

---

## 3.6 Profile, Reservations, Favorites

### 3.6.1 `GET /api/Users/me/dashboard` (Yeni composite endpoint onerisi)

Amac: `/profile` ekranindaki stats + upcoming + favorites preview.

```json
{
  "stats": {
    "upcomingTrips": 2,
    "totalBookings": 15,
    "rewardPoints": 1250,
    "weeklyDelta": 1
  },
  "upcomingReservation": {
    "id": "res_1",
    "resortName": "Sunny Beach Resort",
    "location": "Cancun, Mexico",
    "imageUrl": "https://...",
    "date": "2026-03-01",
    "time": "10:00",
    "guests": "2 Adults",
    "status": "confirmed",
    "isFavorite": false
  },
  "favoriteSpots": [
    {
      "id": "spot_1",
      "title": "Blue Lagoon",
      "location": "Bali",
      "imageUrl": "https://...",
      "rating": 4.9,
      "price": "$45",
      "priceUnit": "perPerson",
      "isFavorite": true
    }
  ]
}
```

### 3.6.2 Rezervasyonlar

- `GET /api/Reservations?status=Pending|Confirmed|Completed|Cancelled&page=1&pageSize=20`
- `PUT /api/Reservations/{id}` (modify)
- `POST /api/Reservations/{id}/cancel`
- `POST /api/Reservations/{id}/check-in`
- `POST /api/Reviews` (reservation review)
- `POST /api/Reservations/{id}/payment-intent` (Yeni; odeme tamamlama aksiyonu)

`GET /api/Reservations` response (frontend card uyumlu):

```json
{
  "items": [
    {
      "id": "res_1",
      "venueName": "Azure Beach Club",
      "location": "Santa Monica, CA",
      "imageUrl": "https://...",
      "date": "2026-03-05",
      "time": "10:00 - 16:00",
      "selection": "2 Sunbeds, 1 Cabana",
      "price": 120,
      "status": "confirmed",
      "guests": "2 Adults"
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalCount": 35,
  "totalPages": 2,
  "hasPrevious": false,
  "hasNext": true
}
```

### 3.6.3 Favoriler

- `GET /api/Favorites?type=Beach|Pool&search=...&sortBy=rating|price|distance&page=1&pageSize=20`
- `POST /api/Favorites`
- `DELETE /api/Favorites/{venueId}`

`GET /api/Favorites` response:

```json
{
  "favorites": [
    {
      "id": "fav_1",
      "venueId": "spot_1",
      "venueType": "Beach",
      "venueName": "Azure Coastline",
      "venueSlug": "azure-coastline",
      "venueImageUrl": "https://...",
      "venueCity": "San Diego",
      "distanceKm": 5,
      "venuePrice": 25,
      "currency": "USD",
      "priceUnit": "person",
      "venueRating": 4.8,
      "venueReviewCount": 120,
      "statusBadge": "Open Today",
      "addedAt": "2026-02-10T08:00:00Z"
    }
  ],
  "totalCount": 6
}
```

---

## 3.7 Payment Methods

Frontendte endpoint tanimi var ama servis baglanmamis; backendte su sozlesme gereklidir:

- `GET /api/payment-methods`
- `POST /api/payment-methods`
- `PATCH /api/payment-methods/{id}`
- `PATCH /api/payment-methods/{id}/default`
- `DELETE /api/payment-methods/{id}`

`GET /api/payment-methods` response:

```json
{
  "items": [
    {
      "id": "pm_1",
      "brand": "visa",
      "lastFour": "4242",
      "expiryDate": "12/2027",
      "isDefault": true
    }
  ]
}
```

`POST /api/payment-methods` request (PCI icin tokenized):

```json
{
  "provider": "stripe",
  "paymentMethodToken": "pm_xxx",
  "setAsDefault": true
}
```

---

## 3.8 Account Settings

Mevcut user endpointleriyle uyumlu olacak sekilde:

- `GET /api/Users/me`
- `PUT /api/Users/me`
- `PUT /api/Users/me/settings`
- `POST /api/Users/me/avatar` (multipart/form-data)
- `POST /api/Users/me/password`
- `DELETE /api/Users/me`
- `POST /api/Users/me/change-email` (Yeni onerisi)

`PUT /api/Users/me` request:

```json
{
  "firstName": "Alex",
  "lastName": "Johnson",
  "phoneNumber": "+15551234567",
  "bio": "..."
}
```

`PUT /api/Users/me/settings` request:

```json
{
  "emailNotifications": true,
  "pushNotifications": false,
  "language": "en-US",
  "currency": "USD",
  "profilePublic": true,
  "dataSharing": false
}
```

---

## 3.9 Become Host - Create Listing Akisi

### 3.9.1 `POST /api/Listings` (Mevcut endpoint, request genisletme gerekli)

Create listing ekraninda toplanan alanlar:
- `propertyType`, `propertyName`, `location`, `description`
- `amenities[]`, `standardPrice`, `childSeniorPrice`, `sunbedPrice`, `sunbedQuantity`
- `photos[]`, `coverPhotoId`, `termsAccepted`

Request (onerilen):

```json
{
  "title": "Sunny Oasis",
  "description": "...",
  "type": "Pool",
  "city": "Miami",
  "country": "USA",
  "latitude": 25.76,
  "longitude": -80.19,
  "pricing": {
    "standardPrice": 50,
    "childSeniorPrice": 35,
    "sunbedEnabled": true,
    "sunbedPrice": 15,
    "sunbedQuantity": 25,
    "currency": "USD"
  },
  "amenities": ["showers", "restrooms", "wifi"],
  "status": "Draft"
}
```

Response:

```json
{
  "id": "listing_1001",
  "slug": "sunny-oasis",
  "status": "Draft"
}
```

### 3.9.2 `POST /api/Listings/photos/upload` (Mevcut endpoint)

- Content-Type: `multipart/form-data`
- FormData:
  - `listingId`: string
  - `photos`: file[]

Response:

```json
{
  "urls": ["https://cdn/.../1.jpg", "https://cdn/.../2.jpg"]
}
```

### 3.9.3 `POST /api/Listings/{id}/publish` (Yeni endpoint onerisi)

Request:

```json
{
  "coverPhotoUrl": "https://cdn/.../1.jpg",
  "termsAccepted": true
}
```

Response:

```json
{
  "id": "listing_1001",
  "status": "PendingReview",
  "message": "Listing submitted for review"
}
```

---

## 4) Kritik Uyum Notlari

1. `DestinationItemDto` icerisine `type` alani eklenmezse destination ekranlari dogru ayrismaz.
2. Explore ekraninin tamami (liste + harita) su an static; backend bu akis icin `Search/listings` + `Explore` cevabini UI kart alanlarini kapsayacak sekilde dondurmelidir.
3. Spot/Boat/Blog detail endpointleri list endpointlerinden daha zengin response gerektirir; `detail` odakli DTO ayrimi onerilir.
4. Profile/Favorites/Reservations ekranlari API servisleri tanimli olmasina ragmen UI tarafi mock ile calisiyor; backendte action endpointleri (cancel, review, payment, set-default card) tamamlanmadan ekranlar tam canlanmaz.
5. Create listing akisi icin `Draft -> Publish` adimi backendte ayrilmali; tek `POST /Listings` genelde yeterli olmaz.

