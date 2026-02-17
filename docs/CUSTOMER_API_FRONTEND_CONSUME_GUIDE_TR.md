# Customer API Frontend Consume Guide (TR)

Bu dokuman, customer uygulamasi icin backend tarafinda eklenen/guncellenen API'leri frontend'in dogrudan consume etmesi icin hazirlanmistir.

- Son guncelleme: 2026-02-17
- Base URL: `/api`
- JSON naming: `camelCase`
- Tarih: ISO-8601 (`2026-02-17T10:00:00Z`)
- Para birimi: ISO-4217 (`USD`, `EUR`, `TRY`)

## 1) Auth ve Genel Kurallar

- Private endpointlerde header zorunlu:

```http
Authorization: Bearer <jwt>
```

- Customer policy ile korunan endpointler:
  - `/api/favorites/**`
  - `/api/reservations/**`
  - `/api/payment-methods/**`
- `Users` endpointleri genel `Authorize` ile korunur.

### 1.1 Hata Formati

Controller bazli islenen hatalarda genel olarak su format doner:

```json
{ "error": "..." }
```

Validation/istek dogrulama icin tipik kodlar: `400`, `401`, `403`, `404`.

---

## 2) Search + Explore

## 2.1 GET `/api/search/listings` (Customer Explore List)

### Query

- `viewType`: `Beach | Pool`
- `searchTerm`: string
- `city`: string
- `dateFrom`: ISO date-time
- `dateTo`: ISO date-time
- `guests`: number
- `minPrice`: number
- `maxPrice`: number
- `amenities`: comma-separated (`wifi,parking`)
- `sortBy`: `recommended | price | rating | distance`
- `sortOrder`: `asc | desc`
- `page`: number (default `1`)
- `pageSize`: number (default `20`, max `100`)

### Response

`PagedResult<CustomerSearchListingItemDto>`

```json
{
  "items": [
    {
      "id": "guid",
      "slug": "kaputas-beach-kas",
      "title": "Kaputas Beach",
      "imageUrl": "https://...",
      "imageAlt": "...",
      "type": "Beach",
      "typeLabel": "Beach",
      "typeIcon": "public",
      "locationText": "Kas, Turkey",
      "distanceKm": null,
      "rating": 4.8,
      "reviewCount": 124,
      "price": 25,
      "currency": "USD",
      "priceUnit": "person",
      "displayPrice": "$25",
      "displayPriceUnit": "/ person",
      "displayTotal": "$100",
      "badges": ["Featured"],
      "tags": ["WiFi", "Parking"],
      "isFavorite": false,
      "latitude": 36.2,
      "longitude": 29.4
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

### Notlar

- Kullanici authenticated ise `isFavorite` dolu gelir.
- `displayTotal` sadece `dateFrom + dateTo + guests` birlikte gelirse hesaplanir.

## 2.2 GET `/api/search/suggestions`

### Query

- `term`: string (min 2)

### Response

```json
["Kas, Antalya", "Kaputas Beach"]
```

## 2.3 GET `/api/explore` (Map Markers)

### Query

- `neLat`, `neLng`, `swLat`, `swLng` (opsiyonel)
- `type`: `Beach | Pool | Yacht | DayTrip` (opsiyonel)

### Response

```json
{
  "markers": [
    {
      "id": "guid",
      "slug": "...",
      "name": "...",
      "venueType": "Beach",
      "latitude": 36.2,
      "longitude": 29.4,
      "price": 25,
      "currency": "USD",
      "rating": 4.8,
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

---

## 3) Destinations

## 3.1 GET `/api/destinations`

### Query

- `featured`: boolean?
- `type`: `Beach | Pool`?
- `search`: string?
- `page`: number (default `1`)
- `pageSize`: number (default `20`)

### Response

```json
{
  "destinations": [
    {
      "id": "guid",
      "slug": "santa-monica",
      "name": "Santa Monica",
      "country": "USA",
      "imageUrl": "https://...",
      "spotCount": 12,
      "type": "Beach",
      "minPrice": 15,
      "maxPrice": 120,
      "averageRating": 4.8,
      "isFeatured": true
    }
  ],
  "totalCount": 64
}
```

## 3.2 GET `/api/destinations/{slug}`

Mevcut basic detail response (spot list odakli).

## 3.3 GET `/api/destinations/{slug}/detail` (Customer page model)

### Response

```json
{
  "id": "guid",
  "slug": "santa-monica",
  "type": "Beach",
  "hero": {
    "title": "Santa Monica",
    "subtitle": "...",
    "location": "California, USA",
    "imageUrl": "https://...",
    "spotCount": 12
  },
  "overview": {
    "description": "...",
    "avgWaterTemp": null,
    "sunnyDaysPerYear": null,
    "mapImageUrl": null
  },
  "features": [
    { "icon": "wb_sunny", "title": "Perfect Weather", "description": "..." }
  ],
  "spots": [
    {
      "id": "guid",
      "slug": "sunset-beach-club",
      "name": "Sunset Beach Club",
      "location": "Santa Monica",
      "imageUrl": "https://...",
      "rating": 4.9,
      "reviewCount": 128,
      "price": 45,
      "currency": "USD",
      "priceUnit": "day"
    }
  ],
  "cta": {
    "title": "Aradigini bulamadin mi?",
    "description": "...",
    "buttonText": "Yakinindaki Beach'leri Kesfet",
    "backgroundImageUrl": "https://..."
  }
}
```

---

## 4) Spots (Beach/Pool Detail)

## 4.1 GET `/api/spots/{slug}`

### Response (CustomerSpotDetailResponse)

```json
{
  "id": "guid",
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
      { "label": "Sunny Beach Cove", "link": null }
    ]
  },
  "gallery": [{ "url": "https://...", "alt": "...", "isPrimary": true }],
  "conditions": [{ "icon": "air", "label": "Wind", "value": "12 km/h" }],
  "description": "...",
  "amenities": [{ "icon": "wifi", "label": "WiFi", "available": true }],
  "location": {
    "name": "Sunny Beach Cove",
    "subtitle": "Crete, Greece",
    "latitude": 35.1,
    "longitude": 24.9,
    "mapImageUrl": null
  },
  "reviewsPreview": {
    "overallRating": 4.8,
    "totalReviews": 124,
    "breakdown": [{ "stars": 5, "percentage": 73 }],
    "categories": [{ "label": "Cleanliness", "score": 4.9 }],
    "reviews": [{ "id": "guid", "avatarUrl": "https://...", "name": "Sarah", "date": "2026-02-01T00:00:00Z", "text": "..." }]
  },
  "bookingDefaults": {
    "price": 25,
    "currency": "USD",
    "priceUnit": "day",
    "defaultDate": "2026-02-20T00:00:00Z",
    "defaultGuests": 2,
    "lineItems": [
      { "label": "Ticket (2x)", "amount": 50 },
      { "label": "Service fee", "amount": 5 }
    ],
    "total": 55,
    "rareFindMessage": "Usually fully booked"
  }
}
```

## 4.2 POST `/api/spots/{slug}/quote`

### Request

```json
{
  "date": "2026-02-20T00:00:00Z",
  "guests": { "adults": 2, "children": 0 },
  "selections": { "selectedAmenities": ["sunbed"] }
}
```

### Response

```json
{
  "currency": "USD",
  "lineItems": [
    { "label": "Entry fee (2x)", "amount": 50 },
    { "label": "Selected amenities", "amount": 7.5 },
    { "label": "Service fee", "amount": 5.75 }
  ],
  "total": 63.25,
  "isAvailable": true,
  "unavailableReason": null
}
```

---

## 5) Boat Tours

## 5.1 GET `/api/boat-tours/yacht/{slug}`

Response frontend card/section odakli zengin JSON doner.

Ana bloklar:
- `id`, `slug`, `title`, `location`, `rating`, `reviewCount`, `isSuperhost`
- `gallery[]`
- `specs[]`
- `about[]`
- `features[]`
- `accommodationOptions[]` (simdilik bos olabilir)
- `cateringItems[]` (simdilik bos olabilir)
- `activityItems[]` (simdilik bos olabilir)
- `cruisingRoute`
- `bookingDefaults`

## 5.2 GET `/api/boat-tours/day-trip/{slug}`

Ana bloklar:
- `id`, `slug`, `title`, `location`, `rating`, `reviewCount`
- `gallery[]`
- `infoBadges[]`
- `host`
- `description[]`
- `routeStops[]` (simdilik bos olabilir)
- `amenities[]`, `foodItems[]`, `activityTags[]`, `musicInfo`
- `bookingDefaults`

---

## 6) Blog Detail + Interaction

## 6.1 GET `/api/blog/{slug}/detail`

### Response

```json
{
  "id": "guid",
  "slug": "top-10-hidden-beaches-sardinia",
  "title": "Top 10 Hidden Beaches in Sardinia",
  "description": "...",
  "heroImageUrl": "https://...",
  "category": "Travel Tips",
  "tags": ["Sardinia", "BeachLife"],
  "readTime": 5,
  "publishedAt": "2026-02-10T09:00:00Z",
  "author": { "name": "Alex", "bio": "...", "avatarUrl": "https://..." },
  "tableOfContents": [{ "id": "intro", "title": "Introduction" }],
  "contentBlocks": [
    { "type": "heading", "id": "intro", "text": "Introduction", "imageUrl": null, "caption": null, "author": null },
    { "type": "paragraph", "id": null, "text": "...", "imageUrl": null, "caption": null, "author": null }
  ]
}
```

## 6.2 GET `/api/blog/{slug}/related?limit=3`

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

## 6.3 GET `/api/blog/{slug}/comments?page=1&pageSize=20`

```json
{
  "items": [
    {
      "id": "guid",
      "author": { "name": "Customer Name", "avatarUrl": "https://..." },
      "text": "Great article",
      "createdAt": "2026-02-17T10:20:00Z"
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalCount": 42,
  "totalPages": 3,
  "hasPrevious": false,
  "hasNext": true
}
```

## 6.4 POST `/api/blog/{slug}/comments` (Auth gerekli)

### Request

```json
{ "text": "Great article" }
```

### Response

`201 Created` + `BlogCommentDto`

---

## 7) Users / Profile

## 7.1 GET `/api/users/me/dashboard`

```json
{
  "stats": {
    "upcomingTrips": 2,
    "totalBookings": 15,
    "rewardPoints": 1250,
    "weeklyDelta": 1
  },
  "upcomingReservation": {
    "id": "guid",
    "resortName": "Sunny Beach Resort",
    "location": "Cancun, Mexico",
    "imageUrl": "https://...",
    "date": "2026-03-01T00:00:00Z",
    "time": "10:00",
    "guests": "2 Adults",
    "status": "confirmed",
    "isFavorite": false
  },
  "favoriteSpots": [
    {
      "id": "guid",
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

## 7.2 POST `/api/users/me/change-email`

### Request

```json
{
  "newEmail": "new@example.com",
  "password": "currentPassword"
}
```

### Response

```json
{
  "email": "new@example.com",
  "message": "Email change request received"
}
```

## 7.3 Avatar/Password Alias Endpointleri

Frontend uyumlulugu icin hem `PUT` hem `POST` desteklenir:

- Avatar: `PUT /api/users/me/avatar` ve `POST /api/users/me/avatar`
- Password: `PUT /api/users/me/password` ve `POST /api/users/me/password`

## 7.4 PUT `/api/users/me/settings` guncellendi

`dataSharing` alani eklendi:

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

## 8) Favorites

## 8.1 GET `/api/favorites`

### Query

- `type`: `Beach|Pool|Yacht|DayTrip`?
- `search`: string?
- `sortBy`: `rating|price|distance`?
- `page`: number (default `1`)
- `pageSize`: number (default `20`)

### Response

```json
{
  "favorites": [
    {
      "id": "guid",
      "venueId": "guid",
      "venueType": "Beach",
      "venueName": "Azure Coastline",
      "venueSlug": "azure-coastline",
      "venueImageUrl": "https://...",
      "venueCity": "San Diego",
      "distanceKm": null,
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

> Not: Bu response'ta `page/pageSize/hasNext` yok, sadece `favorites + totalCount` var.

## 8.2 POST `/api/favorites`

```json
{ "venueId": "guid", "venueType": "Beach" }
```

## 8.3 DELETE `/api/favorites/{venueId}`

---

## 9) Reservations

## 9.1 GET `/api/reservations?status=Pending|Confirmed|Completed|Cancelled&page=1&pageSize=20`

### Response

```json
{
  "items": [
    {
      "id": "guid",
      "venueName": "Azure Beach Club",
      "location": "Santa Monica, CA",
      "imageUrl": "https://...",
      "date": "2026-03-05T00:00:00Z",
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

## 9.2 PUT `/api/reservations/{id}`

### Request

```json
{
  "startTime": "2026-03-05T10:00:00Z",
  "endTime": "2026-03-05T16:00:00Z",
  "guests": { "adults": 2, "children": 0, "infants": 0 },
  "selections": { "sunbeds": true },
  "specialRequests": "Near water"
}
```

### Response

`200 OK` + `ReservationResponse`

## 9.3 POST `/api/reservations/{id}/cancel`

`204 No Content`

## 9.4 POST `/api/reservations/{id}/check-in`

`204 No Content`

## 9.5 POST `/api/reservations/{id}/review`

### Request

```json
{ "rating": 5, "comment": "Great experience" }
```

### Response

```json
{ "id": "guid", "message": "Degerlendirmeniz basariyla gonderildi" }
```

## 9.6 POST `/api/reservations/{id}/payment-intent`

### Response

```json
{
  "reservationId": "guid",
  "paymentId": "guid",
  "amount": 120,
  "currency": "USD",
  "status": "pending"
}
```

---

## 10) Payment Methods

## 10.1 GET `/api/payment-methods`

```json
{
  "items": [
    {
      "id": "guid",
      "brand": "visa",
      "lastFour": "4242",
      "expiryDate": "12/2027",
      "isDefault": true
    }
  ]
}
```

## 10.2 POST `/api/payment-methods`

### Request

```json
{
  "provider": "stripe",
  "paymentMethodToken": "pm_xxx",
  "setAsDefault": true,
  "brand": "visa",
  "lastFour": "4242",
  "expiryDate": "12/2027"
}
```

### Response

`201 Created` + `PaymentMethodResponse`

## 10.3 PATCH `/api/payment-methods/{id}`

### Request

```json
{
  "brand": "mastercard",
  "lastFour": "1111",
  "expiryDate": "11/2028"
}
```

### Response

`200 OK` + `PaymentMethodResponse`

## 10.4 PATCH `/api/payment-methods/{id}/default`

`204 No Content`

> `PUT /api/payment-methods/{id}/default` de alias olarak desteklenir.

## 10.5 DELETE `/api/payment-methods/{id}`

`204 No Content`

---

## 11) Listings (Become Host Flow)

## 11.1 POST `/api/listings`

### Request

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

### Response

```json
{
  "id": "guid",
  "slug": "sunny-oasis",
  "status": "Draft"
}
```

### Not

- `status` verilmezse backend default olarak `Draft` set eder.

## 11.2 POST `/api/listings/photos/upload`

### Content-Type

`multipart/form-data`

### Form fields

- `listingId`: GUID
- `photos`: file[]

### Response

```json
{ "urls": ["/listings/<id>/photos/<guid>.jpg"] }
```

## 11.3 POST `/api/listings/{id}/publish`

### Request

```json
{
  "coverPhotoUrl": "/listings/<id>/photos/<guid>.jpg",
  "termsAccepted": true
}
```

### Response

```json
{
  "id": "guid",
  "status": "PendingReview",
  "message": "Listing submitted for review"
}
```

---

## 12) Newsletter (Response Guncellemesi)

## POST `/api/newsletter/subscribe`

### Request

```json
{
  "email": "user@example.com",
  "name": "Alex",
  "language": "en"
}
```

### Response

```json
{
  "success": true,
  "message": "Subscribed",
  "email": "user@example.com",
  "isSubscribed": true
}
```

---

## 13) Frontend Integration Checklist

1. Eski static/mock modelleri, buradaki response shape'lerine gore guncelle.
2. `GET /api/search/listings` sonucundaki `PagedResult` alanlarini (`items`, `hasNext`, vb.) dogrudan paging state'e bagla.
3. Spot detail booking card'i icin ilk render'da `GET /api/spots/{slug}`, tarih/misafir degisiminde `POST /api/spots/{slug}/quote` cagir.
4. Profile dashboard sayfasinda tek sorgu olarak `GET /api/users/me/dashboard` kullan.
5. Reservations/Favorites listelerinde local sort yerine backend query params (`sortBy`, `status`, `page`, `pageSize`) kullan.
6. Payment method formunda artik ham kart yerine tokenized payload (`paymentMethodToken`) gonder.
7. Become host akisini `create -> photo upload -> publish` adimlariyla bagla.
8. Blog detail sayfasinda:
   - `GET /api/blog/{slug}/detail`
   - `GET /api/blog/{slug}/related`
   - `GET /api/blog/{slug}/comments`
   - authenticated ise `POST /api/blog/{slug}/comments`

