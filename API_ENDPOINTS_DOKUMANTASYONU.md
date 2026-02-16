# Swimago API Endpoints Dokümantasyonu

Bu doküman, Swimago platformunun üç uygulaması (Customer, Host Panel, Admin Panel) için gerekli API endpoint'lerini, request ve response modellerini içermektedir.

---

## 📋 İçindekiler

1. [Kimlik Doğrulama (Auth)](#1-kimlik-doğrulama-auth)
2. [Kullanıcı Yönetimi](#2-kullanıcı-yönetimi)
3. [Destinasyonlar (Beaches & Pools)](#3-destinasyonlar-beaches--pools)
4. [Tekne Turları (Boat Tours)](#4-tekne-turları-boat-tours)
5. [Rezervasyonlar](#5-rezervasyonlar)
6. [Favoriler](#6-favoriler)
7. [Blog](#7-blog)
8. [Ödeme Yöntemleri](#8-ödeme-yöntemleri)
9. [İlan Oluşturma (Host)](#9-i̇lan-oluşturma-host)
10. [Host Panel Endpoints](#10-host-panel-endpoints)
11. [Admin Panel Endpoints](#11-admin-panel-endpoints)
12. [Ortak Modeller](#12-ortak-modeller)

---

## 1. Kimlik Doğrulama (Auth)

### `POST /api/auth/login`
Kullanıcı girişi yapar.

**Request:**
```json
{
  "email": "string",
  "password": "string"
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "user": {
      "id": "uuid",
      "name": "string",
      "email": "string",
      "avatar": "string | null",
      "role": "customer | host | admin",
      "membershipLevel": "string"
    },
    "token": "string",
    "refreshToken": "string",
    "expiresAt": "datetime"
  }
}
```

---

### `POST /api/auth/register`
Yeni kullanıcı kaydı oluşturur.

**Request:**
```json
{
  "firstName": "string",
  "lastName": "string",
  "email": "string",
  "password": "string",
  "phone": "string | null"
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "userId": "uuid",
    "message": "string"
  }
}
```

---

### `POST /api/auth/logout`
Kullanıcı çıkışı yapar.

**Headers:** `Authorization: Bearer {token}`

**Response:**
```json
{
  "success": true
}
```

---

### `POST /api/auth/refresh`
Token yenileme işlemi.

**Request:**
```json
{
  "refreshToken": "string"
}
```

**Response:**
```json
{
  "token": "string",
  "refreshToken": "string",
  "expiresAt": "datetime"
}
```

---

### `POST /api/auth/forgot-password`
Şifre sıfırlama e-postası gönderir.

**Request:**
```json
{
  "email": "string"
}
```

---

### `POST /api/auth/reset-password`
Yeni şifre belirler.

**Request:**
```json
{
  "token": "string",
  "newPassword": "string"
}
```

---

## 2. Kullanıcı Yönetimi

### `GET /api/users/me`
Mevcut kullanıcı bilgilerini getirir.

**Headers:** `Authorization: Bearer {token}`

**Response:**
```json
{
  "id": "uuid",
  "firstName": "string",
  "lastName": "string",
  "email": "string",
  "phone": "string | null",
  "avatar": "string | null",
  "membershipLevel": "string",
  "stats": {
    "upcomingTrips": "number",
    "totalBookings": "number",
    "rewardPoints": "number"
  },
  "notificationSettings": {
    "emailNotifications": "boolean",
    "pushNotifications": "boolean"
  },
  "languageSettings": {
    "language": "string",
    "currency": "string"
  },
  "privacySettings": {
    "profileVisibility": "boolean",
    "dataSharing": "boolean"
  },
  "createdAt": "datetime"
}
```

---

### `PUT /api/users/me`
Kullanıcı profilini günceller.

**Request:**
```json
{
  "firstName": "string",
  "lastName": "string",
  "phone": "string | null"
}
```

---

### `PUT /api/users/me/avatar`
Profil fotoğrafını günceller.

**Request:** `multipart/form-data`
- `avatar`: File

**Response:**
```json
{
  "avatarUrl": "string"
}
```

---

### `PUT /api/users/me/settings`
Kullanıcı ayarlarını günceller.

**Request:**
```json
{
  "notificationSettings": {
    "emailNotifications": "boolean",
    "pushNotifications": "boolean"
  },
  "languageSettings": {
    "language": "string",
    "currency": "string"
  },
  "privacySettings": {
    "profileVisibility": "boolean",
    "dataSharing": "boolean"
  }
}
```

---

### `PUT /api/users/me/password`
Şifre değiştirir.

**Request:**
```json
{
  "currentPassword": "string",
  "newPassword": "string"
}
```

---

### `DELETE /api/users/me`
Kullanıcı hesabını siler.

---

## 3. Destinasyonlar (Beaches & Pools)

### `GET /api/destinations`
Tüm destinasyonları listeler (plajlar ve havuzlar).

**Query Parameters:**
- `type`: `beach | pool | all` (varsayılan: all)
- `city`: `string`
- `country`: `string`
- `search`: `string`
- `amenities`: `string[]` (virgülle ayrılmış)
- `minPrice`: `number`
- `maxPrice`: `number`
- `minRating`: `number`
- `page`: `number` (varsayılan: 1)
- `limit`: `number` (varsayılan: 10)
- `sortBy`: `rating | price | distance`
- `lat`: `number` (konum bazlı sıralama için)
- `lng`: `number`

**Response:**
```json
{
  "data": [
    {
      "id": "uuid",
      "slug": "string",
      "name": "string",
      "type": "beach | pool",
      "description": "string",
      "city": "string",
      "country": "string",
      "location": {
        "lat": "number",
        "lng": "number",
        "address": "string"
      },
      "imageUrl": "string",
      "rating": "number",
      "reviewCount": "number",
      "spotCount": "number",
      "priceRange": {
        "min": "number",
        "max": "number",
        "currency": "string"
      },
      "isOpen": "boolean",
      "isFeatured": "boolean"
    }
  ],
  "pagination": {
    "page": "number",
    "limit": "number",
    "total": "number",
    "totalPages": "number"
  }
}
```

---

### `GET /api/destinations/{slug}`
Tek bir destinasyonun detaylarını getirir.

**Response:**
```json
{
  "id": "uuid",
  "slug": "string",
  "name": "string",
  "type": "beach | pool",
  "description": "string",
  "location": {
    "city": "string",
    "country": "string",
    "lat": "number",
    "lng": "number",
    "address": "string"
  },
  "images": [
    {
      "url": "string",
      "alt": "string"
    }
  ],
  "rating": "number",
  "reviewCount": "number",
  "breadcrumbs": [
    {
      "label": "string",
      "link": "string | null"
    }
  ],
  "conditions": [
    {
      "icon": "string",
      "label": "string",
      "value": "string"
    }
  ],
  "amenities": [
    {
      "icon": "string",
      "label": "string",
      "available": "boolean"
    }
  ],
  "spots": [
    {
      "id": "uuid",
      "slug": "string",
      "name": "string",
      "imageUrl": "string",
      "rating": "number",
      "reviewCount": "number",
      "price": "number",
      "priceUnit": "string"
    }
  ],
  "reviews": {
    "overallRating": "number",
    "totalReviews": "number",
    "breakdown": [
      { "stars": 5, "percentage": "number" }
    ],
    "categories": [
      { "label": "string", "score": "number" }
    ],
    "items": [
      {
        "id": "uuid",
        "userId": "uuid",
        "userName": "string",
        "userAvatar": "string | null",
        "rating": "number",
        "text": "string",
        "date": "datetime"
      }
    ]
  }
}
```

---

### `GET /api/spots/{slug}`
Belirli bir spot (beach veya pool) detayını getirir.

**Response:**
```json
{
  "id": "uuid",
  "slug": "string",
  "name": "string",
  "type": "beach | pool",
  "destinationId": "uuid",
  "description": "string",
  "headerData": {
    "title": "string",
    "rating": "number",
    "reviewCount": "number",
    "location": "string",
    "breadcrumbs": [
      { "label": "string", "link": "string | null" }
    ]
  },
  "images": [
    { "url": "string", "alt": "string" }
  ],
  "conditions": [
    {
      "icon": "string",
      "iconColor": "string",
      "bgColor": "string",
      "label": "string",
      "value": "string"
    }
  ],
  "amenities": [
    {
      "icon": "string",
      "label": "string",
      "available": "boolean"
    }
  ],
  "location": {
    "name": "string",
    "subtitle": "string",
    "lat": "number",
    "lng": "number",
    "mapImageUrl": "string"
  },
  "bookingInfo": {
    "price": "number",
    "priceUnit": "day | hour | person",
    "checkInDate": "string",
    "checkOutDate": "string",
    "guests": "string",
    "breakdown": [
      { "label": "string", "amount": "number" }
    ],
    "total": "number",
    "rareFindMessage": "string | null"
  },
  "reviews": {
    "overallRating": "number",
    "totalReviews": "number",
    "breakdown": [
      { "stars": "number", "percentage": "number" }
    ],
    "categories": [
      { "label": "string", "score": "number" }
    ],
    "items": [
      {
        "id": "uuid",
        "avatarUrl": "string | null",
        "name": "string",
        "date": "string",
        "text": "string"
      }
    ]
  },
  "hostId": "uuid",
  "isActive": "boolean"
}
```

---

### `GET /api/explore`
Harita görünümü için spot'ları getirir.

**Query Parameters:**
- `type`: `beach | pool`
- `bounds`: `{north, south, east, west}` (harita sınırları)
- `amenities`: `string[]`
- `priceMin`: `number`
- `priceMax`: `number`

**Response:**
```json
{
  "data": [
    {
      "id": "uuid",
      "slug": "string",
      "name": "string",
      "type": "beach | pool",
      "lat": "number",
      "lng": "number",
      "imageUrl": "string",
      "rating": "number",
      "price": "number",
      "priceUnit": "string",
      "tags": [
        { "text": "string", "class": "string" }
      ]
    }
  ]
}
```

---

## 4. Tekne Turları (Boat Tours)

### `GET /api/boat-tours`
Tekne turlarını listeler.

**Query Parameters:**
- `category`: `all | day-trip | yacht`
- `search`: `string`
- `minPrice`: `number`
- `maxPrice`: `number`
- `duration`: `string`
- `maxGuests`: `number`
- `page`: `number`
- `limit`: `number`

**Response:**
```json
{
  "data": [
    {
      "id": "uuid",
      "slug": "string",
      "title": "string",
      "description": "string",
      "imageUrl": "string",
      "imageAlt": "string",
      "category": "day-trip | yacht",
      "categoryLabel": "string",
      "rating": "number",
      "reviewCount": "number",
      "maxGuests": "string",
      "duration": "string",
      "price": "number",
      "priceLabel": "string",
      "priceUnit": "string | null",
      "location": "string",
      "isFavorite": "boolean"
    }
  ],
  "pagination": {
    "page": "number",
    "limit": "number",
    "total": "number"
  }
}
```

---

### `GET /api/boat-tours/yacht/{slug}`
Yat turu detaylarını getirir.

**Response:**
```json
{
  "id": "uuid",
  "slug": "string",
  "title": "string",
  "rating": "number",
  "reviewCount": "number",
  "location": "string",
  "isSuperhost": "boolean",
  "images": [
    { "url": "string", "alt": "string" }
  ],
  "specs": [
    { "icon": "string", "label": "string", "value": "string" }
  ],
  "description": "string",
  "features": [
    { "icon": "string", "label": "string" }
  ],
  "accommodations": [
    {
      "imageUrl": "string",
      "name": "string",
      "description": "string"
    }
  ],
  "catering": [
    { "icon": "string", "text": "string" }
  ],
  "activities": [
    { "icon": "string", "text": "string" }
  ],
  "cruisingRoute": {
    "name": "string",
    "stops": "string",
    "totalRoutes": "number",
    "mapImageUrl": "string"
  },
  "bookingInfo": {
    "price": "number",
    "priceUnit": "day | tour",
    "showTripTypeToggle": "boolean",
    "breakdown": [
      { "label": "string", "amount": "number" }
    ],
    "total": "number",
    "luxuryPromise": "string | null"
  },
  "hostId": "uuid"
}
```

---

### `GET /api/boat-tours/day-trip/{slug}`
Günlük tur detaylarını getirir.

**Response:**
```json
{
  "id": "uuid",
  "slug": "string",
  "title": "string",
  "location": "string",
  "rating": "number",
  "reviewCount": "number",
  "images": [
    { "url": "string", "alt": "string" }
  ],
  "infoBadges": [
    { "icon": "string", "label": "string", "value": "string" }
  ],
  "host": {
    "id": "uuid",
    "avatarUrl": "string",
    "name": "string",
    "title": "string",
    "experience": "string"
  },
  "description": ["string"],
  "routeStops": [
    {
      "time": "string",
      "icon": "string",
      "title": "string",
      "description": "string"
    }
  ],
  "amenities": [
    { "icon": "string", "label": "string" }
  ],
  "foodItems": [
    {
      "icon": "string",
      "iconColor": "green | gray",
      "text": "string"
    }
  ],
  "activityTags": [
    { "label": "string" }
  ],
  "musicInfo": {
    "text": "string"
  },
  "bookingInfo": {
    "price": "number",
    "priceUnit": "tour | person",
    "isPrivateCharter": "boolean",
    "breakdown": [
      { "label": "string", "amount": "number" }
    ],
    "total": "number",
    "rareFindMessage": "string | null",
    "rareFindSubtitle": "string | null"
  }
}
```

---

## 5. Rezervasyonlar

### `GET /api/reservations`
Kullanıcının rezervasyonlarını listeler.

**Query Parameters:**
- `status`: `upcoming | past | cancelled | all`
- `page`: `number`
- `limit`: `number`

**Response:**
```json
{
  "data": [
    {
      "id": "uuid",
      "venueName": "string",
      "venueType": "beach | pool | boat-tour",
      "location": "string",
      "imageUrl": "string",
      "date": "string",
      "time": "string",
      "selection": "string | null",
      "guests": "string",
      "price": "number",
      "currency": "string",
      "status": "confirmed | pending | completed | cancelled"
    }
  ],
  "counts": {
    "upcoming": "number",
    "past": "number",
    "cancelled": "number"
  },
  "pagination": {
    "page": "number",
    "limit": "number",
    "total": "number"
  }
}
```

---

### `GET /api/reservations/{id}`
Rezervasyon detayını getirir.

**Response:**
```json
{
  "id": "uuid",
  "venue": {
    "id": "uuid",
    "name": "string",
    "type": "beach | pool | boat-tour",
    "slug": "string",
    "imageUrl": "string",
    "location": "string"
  },
  "date": "string",
  "time": "string",
  "selection": "string | null",
  "guests": "string",
  "pricing": {
    "breakdown": [
      { "label": "string", "amount": "number" }
    ],
    "total": "number",
    "currency": "string"
  },
  "status": "confirmed | pending | completed | cancelled",
  "paymentStatus": "paid | pending | refunded",
  "checkInCode": "string | null",
  "specialRequests": "string | null",
  "createdAt": "datetime",
  "updatedAt": "datetime"
}
```

---

### `POST /api/reservations`
Yeni rezervasyon oluşturur.

**Request:**
```json
{
  "venueId": "uuid",
  "venueType": "beach | pool | yacht | day-trip",
  "date": "string",
  "time": "string",
  "guests": {
    "adults": "number",
    "children": "number",
    "seniors": "number"
  },
  "selections": {
    "sunbeds": "number",
    "cabanas": "number"
  },
  "specialRequests": "string | null",
  "paymentMethodId": "uuid"
}
```

**Response:**
```json
{
  "id": "uuid",
  "status": "pending | confirmed",
  "paymentIntent": "string | null",
  "confirmationNumber": "string"
}
```

---

### `PUT /api/reservations/{id}`
Rezervasyonu günceller (tarih/saat değişikliği).

**Request:**
```json
{
  "date": "string",
  "time": "string",
  "guests": {
    "adults": "number",
    "children": "number"
  }
}
```

---

### `POST /api/reservations/{id}/cancel`
Rezervasyonu iptal eder.

**Request:**
```json
{
  "reason": "string | null"
}
```

---

### `POST /api/reservations/{id}/check-in`
Check-in işlemi yapar.

**Response:**
```json
{
  "success": true,
  "checkInTime": "datetime"
}
```

---

### `POST /api/reservations/{id}/review`
Rezervasyon için yorum bırakır.

**Request:**
```json
{
  "rating": "number (1-5)",
  "text": "string",
  "categories": {
    "cleanliness": "number",
    "facilities": "number",
    "service": "number"
  }
}
```

---

## 6. Favoriler

### `GET /api/favorites`
Kullanıcının favorilerini listeler.

**Query Parameters:**
- `type`: `beach | pool | boat-tour | all`
- `sortBy`: `rating | price_low | price_high | distance`
- `search`: `string`
- `page`: `number`
- `limit`: `number`

**Response:**
```json
{
  "data": [
    {
      "id": "uuid",
      "venueId": "uuid",
      "venueType": "beach | pool | boat-tour",
      "name": "string",
      "location": "string",
      "distance": "string | null",
      "imageUrl": "string",
      "rating": "number",
      "reviewCount": "number",
      "price": "number",
      "priceUnit": "string",
      "priceLabel": "string",
      "statusBadge": "string | null",
      "addedAt": "datetime"
    }
  ],
  "totalCount": "number",
  "pagination": {
    "page": "number",
    "limit": "number",
    "total": "number"
  }
}
```

---

### `POST /api/favorites`
Favorilere ekler.

**Request:**
```json
{
  "venueId": "uuid",
  "venueType": "beach | pool | yacht | day-trip"
}
```

---

### `DELETE /api/favorites/{venueId}`
Favorilerden çıkarır.

---

## 7. Blog

### `GET /api/blog`
Blog yazılarını listeler.

**Query Parameters:**
- `category`: `all | beaches | pools | tips | health | destinations`
- `search`: `string`
- `page`: `number`
- `limit`: `number`

**Response:**
```json
{
  "featured": {
    "id": "uuid",
    "slug": "string",
    "title": "string",
    "description": "string",
    "imageUrl": "string",
    "readTime": "number"
  },
  "articles": [
    {
      "id": "uuid",
      "slug": "string",
      "title": "string",
      "description": "string",
      "imageUrl": "string",
      "category": "string",
      "readTime": "number",
      "publishedAt": "string",
      "author": {
        "name": "string",
        "avatarUrl": "string | null",
        "initials": "string | null"
      }
    }
  ],
  "categories": [
    { "id": "string", "label": "string" }
  ],
  "pagination": {
    "currentPage": "number",
    "totalPages": "number"
  }
}
```

---

### `GET /api/blog/{slug}`
Blog yazısı detayını getirir.

**Response:**
```json
{
  "id": "uuid",
  "slug": "string",
  "title": "string",
  "content": "string (HTML/Markdown)",
  "imageUrl": "string",
  "category": "string",
  "readTime": "number",
  "publishedAt": "datetime",
  "author": {
    "name": "string",
    "avatarUrl": "string | null",
    "bio": "string | null"
  },
  "tags": ["string"],
  "relatedArticles": [
    {
      "id": "uuid",
      "slug": "string",
      "title": "string",
      "imageUrl": "string"
    }
  ]
}
```

---

### `POST /api/newsletter/subscribe`
Bülten aboneliği oluşturur.

**Request:**
```json
{
  "email": "string"
}
```

---

## 8. Ödeme Yöntemleri

### `GET /api/payment-methods`
Kullanıcının ödeme yöntemlerini listeler.

**Response:**
```json
{
  "data": [
    {
      "id": "uuid",
      "type": "card",
      "brand": "visa | mastercard | amex",
      "last4": "string",
      "expiryMonth": "number",
      "expiryYear": "number",
      "isDefault": "boolean"
    }
  ]
}
```

---

### `POST /api/payment-methods`
Yeni ödeme yöntemi ekler.

**Request:**
```json
{
  "paymentMethodToken": "string (Stripe/Payment provider token)"
}
```

---

### `DELETE /api/payment-methods/{id}`
Ödeme yöntemini siler.

---

### `PUT /api/payment-methods/{id}/default`
Varsayılan ödeme yöntemini ayarlar.

---

## 9. İlan Oluşturma (Host)

### `POST /api/listings`
Yeni ilan oluşturur.

**Request:**
```json
{
  "propertyType": "pool | beach | yacht | day-trip",
  "propertyName": "string",
  "location": {
    "address": "string",
    "city": "string",
    "country": "string",
    "lat": "number",
    "lng": "number"
  },
  "description": "string",
  "amenities": [
    {
      "id": "string",
      "enabled": "boolean"
    }
  ],
  "pricing": {
    "standardPrice": "number",
    "childSeniorPrice": "number | null",
    "childSeniorEnabled": "boolean",
    "sunbedEnabled": "boolean",
    "sunbedPrice": "number | null",
    "sunbedQuantity": "number | null"
  },
  "photos": [
    {
      "url": "string",
      "isCover": "boolean"
    }
  ]
}
```

**Response:**
```json
{
  "id": "uuid",
  "slug": "string",
  "status": "pending_approval",
  "message": "string"
}
```

---

### `POST /api/listings/photos/upload`
İlan fotoğrafı yükler.

**Request:** `multipart/form-data`
- `photo`: File
- `isCover`: boolean

**Response:**
```json
{
  "id": "uuid",
  "url": "string"
}
```

---

## 10. Host Panel Endpoints

> **Not:** Bu endpoint'ler `host` rolüne sahip kullanıcılar için geçerlidir.

### `GET /api/host/dashboard`
Host dashboard verilerini getirir.

**Response:**
```json
{
  "stats": {
    "totalListings": "number",
    "activeListings": "number",
    "pendingReservations": "number",
    "upcomingReservations": "number",
    "totalRevenue": "number",
    "monthlyRevenue": "number"
  },
  "recentReservations": [
    {
      "id": "uuid",
      "guestName": "string",
      "listingName": "string",
      "date": "string",
      "guests": "number",
      "amount": "number",
      "status": "string"
    }
  ],
  "upcomingReservations": [...]
}
```

---

### `GET /api/host/listings`
Host'un ilanlarını listeler.

**Response:**
```json
{
  "data": [
    {
      "id": "uuid",
      "slug": "string",
      "name": "string",
      "type": "beach | pool | yacht | day-trip",
      "imageUrl": "string",
      "status": "active | pending | inactive | rejected",
      "rating": "number",
      "reviewCount": "number",
      "reservationCount": "number",
      "revenue": "number"
    }
  ]
}
```

---

### `GET /api/host/listings/{id}`
İlan detayını getirir.

---

### `PUT /api/host/listings/{id}`
İlanı günceller.

---

### `DELETE /api/host/listings/{id}`
İlanı siler.

---

### `GET /api/host/reservations`
Host'a gelen rezervasyonları listeler.

**Query Parameters:**
- `status`: `pending | confirmed | completed | cancelled`
- `listingId`: `uuid`
- `dateFrom`: `date`
- `dateTo`: `date`

**Response:**
```json
{
  "data": [
    {
      "id": "uuid",
      "listing": {
        "id": "uuid",
        "name": "string"
      },
      "guest": {
        "id": "uuid",
        "name": "string",
        "avatar": "string | null",
        "phone": "string"
      },
      "date": "string",
      "time": "string",
      "guests": "number",
      "totalAmount": "number",
      "status": "pending | confirmed | completed | cancelled",
      "specialRequests": "string | null",
      "createdAt": "datetime"
    }
  ]
}
```

---

### `PUT /api/host/reservations/{id}/status`
Rezervasyon durumunu günceller (onay/red).

**Request:**
```json
{
  "status": "confirmed | rejected",
  "message": "string | null"
}
```

---

### `GET /api/host/calendar`
Müsaitlik takvimini getirir.

**Query Parameters:**
- `listingId`: `uuid`
- `month`: `number`
- `year`: `number`

**Response:**
```json
{
  "listingId": "uuid",
  "availability": [
    {
      "date": "string",
      "isAvailable": "boolean",
      "reservationCount": "number",
      "capacity": "number"
    }
  ]
}
```

---

### `PUT /api/host/calendar`
Müsaitlik ayarlarını günceller.

**Request:**
```json
{
  "listingId": "uuid",
  "dates": [
    {
      "date": "string",
      "isAvailable": "boolean",
      "customPrice": "number | null"
    }
  ]
}
```

---

### `GET /api/host/analytics`
Analitik verilerini getirir.

**Query Parameters:**
- `period`: `week | month | year`
- `listingId`: `uuid | null`

**Response:**
```json
{
  "revenue": {
    "total": "number",
    "trend": "number (percentage)",
    "chartData": [
      { "date": "string", "amount": "number" }
    ]
  },
  "reservations": {
    "total": "number",
    "trend": "number"
  },
  "reviews": {
    "average": "number",
    "total": "number"
  },
  "topListings": [
    {
      "id": "uuid",
      "name": "string",
      "revenue": "number",
      "bookings": "number"
    }
  ]
}
```

---

### `PUT /api/host/listings/{id}/pricing`
Fiyatlandırma ayarlarını günceller.

**Request:**
```json
{
  "standardPrice": "number",
  "childSeniorPrice": "number | null",
  "seasonalPricing": [
    {
      "startDate": "string",
      "endDate": "string",
      "price": "number",
      "label": "string"
    }
  ]
}
```

---

## 11. Admin Panel Endpoints

> **Not:** Bu endpoint'ler sadece `admin` rolüne sahip kullanıcılar için erişilebilir.

### `GET /api/admin/dashboard`
Admin dashboard verilerini getirir.

**Response:**
```json
{
  "stats": {
    "totalUsers": "number",
    "totalHosts": "number",
    "totalListings": "number",
    "pendingApprovals": "number",
    "totalReservations": "number",
    "totalRevenue": "number"
  },
  "recentActivity": [
    {
      "type": "user_registered | listing_created | reservation_made",
      "description": "string",
      "timestamp": "datetime"
    }
  ],
  "pendingApprovals": [
    {
      "id": "uuid",
      "type": "host | listing",
      "name": "string",
      "submittedAt": "datetime"
    }
  ]
}
```

---

### `GET /api/admin/users`
Tüm kullanıcıları listeler.

**Query Parameters:**
- `role`: `customer | host | admin | all`
- `status`: `active | banned | pending`
- `search`: `string`
- `page`: `number`
- `limit`: `number`

**Response:**
```json
{
  "data": [
    {
      "id": "uuid",
      "name": "string",
      "email": "string",
      "role": "customer | host | admin",
      "status": "active | banned | pending",
      "createdAt": "datetime",
      "lastLoginAt": "datetime",
      "reservationCount": "number",
      "listingCount": "number"
    }
  ],
  "pagination": {...}
}
```

---

### `GET /api/admin/users/{id}`
Kullanıcı detayını getirir.

---

### `PUT /api/admin/users/{id}/status`
Kullanıcı durumunu günceller.

**Request:**
```json
{
  "status": "active | banned",
  "reason": "string | null"
}
```

---

### `PUT /api/admin/users/{id}/role`
Kullanıcı rolünü günceller.

**Request:**
```json
{
  "role": "customer | host | admin"
}
```

---

### `GET /api/admin/hosts`
Host başvurularını/listesini getirir.

**Query Parameters:**
- `status`: `pending | approved | rejected`

---

### `PUT /api/admin/hosts/{id}/approve`
Host başvurusunu onaylar.

---

### `PUT /api/admin/hosts/{id}/reject`
Host başvurusunu reddeder.

**Request:**
```json
{
  "reason": "string"
}
```

---

### `GET /api/admin/listings`
Tüm ilanları listeler.

**Query Parameters:**
- `status`: `pending | active | rejected | inactive`
- `type`: `beach | pool | yacht | day-trip`
- `hostId`: `uuid`
- `search`: `string`

---

### `PUT /api/admin/listings/{id}/approve`
İlanı onaylar.

---

### `PUT /api/admin/listings/{id}/reject`
İlanı reddeder.

**Request:**
```json
{
  "reason": "string"
}
```

---

### `DELETE /api/admin/listings/{id}`
İlanı siler.

---

### `GET /api/admin/reservations`
Tüm rezervasyonları listeler.

**Query Parameters:**
- `status`: `all | pending | confirmed | completed | cancelled`
- `dateFrom`: `date`
- `dateTo`: `date`
- `userId`: `uuid`
- `hostId`: `uuid`

---

### `GET /api/admin/reports`
Raporları getirir.

**Query Parameters:**
- `type`: `revenue | users | listings | reservations`
- `period`: `daily | weekly | monthly | yearly`
- `dateFrom`: `date`
- `dateTo`: `date`

**Response:**
```json
{
  "type": "string",
  "period": "string",
  "data": [
    {
      "date": "string",
      "value": "number",
      "label": "string"
    }
  ],
  "summary": {
    "total": "number",
    "average": "number",
    "trend": "number"
  }
}
```

---

### `GET /api/admin/master-data/cities`
Şehirleri listeler.

---

### `POST /api/admin/master-data/cities`
Yeni şehir ekler.

**Request:**
```json
{
  "name": { "tr": "string", "en": "string", "de": "string", "ru": "string" },
  "country": "string",
  "lat": "number",
  "lng": "number",
  "isActive": "boolean"
}
```

---

### `GET /api/admin/master-data/amenities`
Olanak listesini getirir.

---

### `POST /api/admin/master-data/amenities`
Yeni olanak ekler.

**Request:**
```json
{
  "icon": "string",
  "label": { "tr": "string", "en": "string", "de": "string", "ru": "string" },
  "category": "string",
  "applicableTo": ["beach", "pool", "yacht", "day-trip"]
}
```

---

### `GET /api/admin/master-data/categories`
Kategorileri listeler.

---

### `POST /api/admin/master-data/categories`
Yeni kategori ekler.

---

## 12. Ortak Modeller

### Pagination Response
```json
{
  "page": "number",
  "limit": "number",
  "total": "number",
  "totalPages": "number"
}
```

### Error Response
```json
{
  "success": false,
  "error": {
    "code": "string",
    "message": "string",
    "details": "object | null"
  }
}
```

### Multi-Language Field
```json
{
  "tr": "string",
  "en": "string",
  "de": "string",
  "ru": "string"
}
```

### Location Object
```json
{
  "lat": "number",
  "lng": "number",
  "address": "string",
  "city": "string",
  "country": "string"
}
```

### User Roles
- `customer`: Normal kullanıcı
- `host`: İşletme sahibi
- `admin`: Platform yöneticisi

### Venue Types
- `beach`: Plaj
- `pool`: Havuz
- `yacht`: Yat turu
- `day-trip`: Günlük tekne turu

### Reservation Statuses
- `pending`: Beklemede
- `confirmed`: Onaylandı
- `completed`: Tamamlandı
- `cancelled`: İptal edildi

### Listing Statuses
- `pending`: Onay bekliyor
- `active`: Aktif
- `inactive`: Pasif
- `rejected`: Reddedildi

---

## 📌 Notlar

1. Tüm endpoint'ler `/api` prefix'i ile başlar.
2. Tarih formatı: ISO 8601 (`2024-01-15T10:00:00Z`)
3. Para birimi varsayılan olarak USD, kullanıcı tercihine göre değişir.
4. Çoklu dil desteği için `Accept-Language` header'ı kullanılır (`tr`, `en`, `de`, `ru`).
5. Sayfalama varsayılan `limit`: 10, maksimum: 50.
6. Tüm authenticated endpoint'ler `Authorization: Bearer {token}` header'ı gerektirir.
7. Rate limiting: 100 request/dakika (authentication endpoint'leri için 10/dakika).

---

*Bu doküman, frontend uygulamalarının ihtiyaçlarına göre hazırlanmıştır. Backend geliştirme sırasında ek endpoint'ler veya model değişiklikleri gerekebilir.*
