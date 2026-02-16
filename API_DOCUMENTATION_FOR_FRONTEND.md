# Swimago API - Frontend Entegrasyon Dokümantasyonu

Bu dokümantasyon, Swimago API'sinin frontend projesine entegre edilmesi için hazırlanmıştır. AI araçları bu dokümantasyonu kullanarak API tüketimini kolayca gerçekleştirebilir.

## 📋 Genel Bilgiler

**Base URL:** `http://localhost:5000` (Development) veya Production URL  
**Content-Type:** `application/json`  
**Swagger URL:** `http://localhost:5000/swagger`

### Kimlik Doğrulama

Çoğu endpoint JWT Bearer token gerektirir. Login sonrası dönen `token` değerini kullanın:

```
Authorization: Bearer {token}
```

---

## 🔐 1. Authentication (`/api/Auth`)

### 1.1 Kullanıcı Kaydı

**Endpoint:** `POST /api/Auth/register`  
**Authentication:** Gerekli değil  
**Content-Type:** `application/json`

**Request Body:**
```json
{
  "email": "user@example.com",
  "password": "SecurePass123!",
  "firstName": "Ahmet",
  "lastName": "Yılmaz",
  "phoneNumber": "+905551234567",
  "role": "Customer"
}
```

**Request Model:**
- `email` (string, required): E-posta adresi
- `password` (string, required): Şifre (min 8 karakter)
- `firstName` (string, required): Ad
- `lastName` (string, required): Soyad
- `phoneNumber` (string, optional): Telefon numarası
- `role` (string, optional): Kullanıcı rolü. Değerler: `"Customer"`, `"Host"`, `"Admin"`. Default: `"Customer"`

**Response (200 OK):**
```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "email": "user@example.com",
  "firstName": "Ahmet",
  "lastName": "Yılmaz",
  "avatar": null,
  "role": "Customer",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "refresh_token_here",
  "tokenExpiry": "2026-02-09T03:10:00Z",
  "settings": {
    "emailNotifications": true,
    "smsNotifications": true,
    "pushNotifications": true,
    "language": "tr",
    "currency": "TRY",
    "profilePublic": true
  }
}
```

**Error Response (400 Bad Request):**
```json
{
  "error": "E-posta adresi zaten kullanımda"
}
```

---

### 1.2 Giriş Yapma

**Endpoint:** `POST /api/Auth/login`  
**Authentication:** Gerekli değil

**Request Body:**
```json
{
  "email": "user@example.com",
  "password": "SecurePass123!"
}
```

**Response (200 OK):**
```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "email": "user@example.com",
  "firstName": "Ahmet",
  "lastName": "Yılmaz",
  "avatar": "https://storage.example.com/avatars/user.jpg",
  "role": "Customer",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "refresh_token_here",
  "tokenExpiry": "2026-02-09T03:10:00Z",
  "settings": {
    "emailNotifications": true,
    "smsNotifications": true,
    "pushNotifications": true,
    "language": "tr",
    "currency": "TRY",
    "profilePublic": true
  }
}
```

**Error Response (401 Unauthorized):**
```json
{
  "error": "E-posta veya şifre hatalı"
}
```

---

### 1.3 Token Yenileme

**Endpoint:** `POST /api/Auth/refresh`  
**Authentication:** Gerekli değil

**Request Body:**
```json
{
  "refreshToken": "refresh_token_here"
}
```

**Response (200 OK):** (Aynı AuthResponse formatında)

---

### 1.4 Çıkış Yapma

**Endpoint:** `POST /api/Auth/logout`  
**Authentication:** ✅ Required (Bearer Token)

**Response (204 No Content):** Boş response

---

### 1.5 Şifremi Unuttum

**Endpoint:** `POST /api/Auth/forgot-password`  
**Authentication:** Gerekli değil

**Request Body:**
```json
{
  "email": "user@example.com"
}
```

**Response (200 OK):**
```json
{
  "message": "Şifre sıfırlama linki e-posta adresinize gönderildi"
}
```

---

### 1.6 Şifre Sıfırlama

**Endpoint:** `POST /api/Auth/reset-password`  
**Authentication:** Gerekli değil

**Request Body:**
```json
{
  "token": "reset_token_from_email",
  "newPassword": "NewSecurePass123!",
  "confirmPassword": "NewSecurePass123!"
}
```

**Response (200 OK):**
```json
{
  "message": "Şifreniz başarıyla güncellendi"
}
```

---

## 🏖️ 2. İlanlar (`/api/Listings`)

### 2.1 Tüm İlanları Listele

**Endpoint:** `GET /api/Listings?page=1&pageSize=20`  
**Authentication:** Gerekli değil

**Query Parameters:**
- `page` (int, optional): Sayfa numarası. Default: 1
- `pageSize` (int, optional): Sayfa başına öğe sayısı. Default: 20

**Response (200 OK):**
```json
{
  "items": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "title": {
        "tr": "Sahil Plajı",
        "en": "Sahil Beach"
      },
      "description": {
        "tr": "Mavi bayraklı temiz plaj",
        "en": "Blue flag clean beach"
      },
      "type": "Beach",
      "pricePerDay": 150.00,
      "city": {
        "tr": "İzmir",
        "en": "Izmir"
      },
      "country": "Turkey",
      "latitude": 38.4192,
      "longitude": 27.1287,
      "photos": [
        "https://storage.example.com/listings/photo1.jpg"
      ],
      "amenities": ["Parking", "Shower", "Umbrella"],
      "rating": 4.5,
      "reviewCount": 42,
      "isActive": true,
      "createdAt": "2026-01-15T10:30:00Z"
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalCount": 150
}
```

---

### 2.2 İlan Detaylarını Getir

**Endpoint:** `GET /api/Listings/{id}`  
**Authentication:** Gerekli değil

**Path Parameters:**
- `id` (guid, required): İlan ID'si

**Response (200 OK):** (Yukarıdaki listing objesi formatında, daha detaylı bilgilerle)

**Error Response (404 Not Found):**
```json
{
  "error": "Listing not found"
}
```

---

### 2.3 Tür Bazlı İlan Listesi

**Endpoint:** `GET /api/Listings/type/{type}`  
**Authentication:** Gerekli değil

**Path Parameters:**
- `type` (string, required): İlan türü. Değerler: `"Beach"`, `"Pool"`, `"Yacht"`, `"DayTrip"`

**Response (200 OK):** Liste formatında ilan array'i

---

### 2.4 Yakınımdaki İlanlar

**Endpoint:** `GET /api/Listings/nearby?latitude=38.4192&longitude=27.1287&radius=10&type=Beach`  
**Authentication:** Gerekli değil

**Query Parameters:**
- `latitude` (decimal, required): Enlem (-90 ile 90 arası)
- `longitude` (decimal, required): Boylam (-180 ile 180 arası)
- `radius` (decimal, optional): Yarıçap (km). Default: 10, Max: 100
- `type` (string, optional): İlan türü filtresi

**Response (200 OK):**
```json
{
  "searchCenter": {
    "latitude": 38.4192,
    "longitude": 27.1287
  },
  "radiusKm": 10,
  "type": "Beach",
  "count": 5,
  "results": [
    {
      "id": "...",
      "title": {...},
      "distance": 2.5
    }
  ]
}
```

**Error Response (400 Bad Request):**
```json
{
  "error": "Geçersiz enlem. -90 ile 90 arasında olmalıdır."
}
```

---

### 2.5 İlan Oluşturma

**Endpoint:** `POST /api/Listings`  
**Authentication:** ✅ Required (Bearer Token - Host veya Admin)

**Request Body:**
```json
{
  "title": "Yeni Plaj",
  "description": "Açıklama",
  "type": "Beach",
  "pricePerDay": 200.00,
  "city": "İzmir",
  "country": "Turkey",
  "latitude": 38.4192,
  "longitude": 27.1287
}
```

**Response (201 Created):**
```json
{
  "message": "İlan oluşturuldu"
}
```

---

### 2.6 Fotoğraf Yükleme

**Endpoint:** `POST /api/Listings/photos/upload`  
**Authentication:** ✅ Required (Bearer Token - Host veya Admin)  
**Content-Type:** `multipart/form-data`

**Form Data:**
- `files`: Birden fazla dosya (IFormFile[])

**Response (200 OK):**
```json
[
  "/listings/photos/3fa85f64.jpg",
  "/listings/photos/4ga96g75.jpg"
]
```

---

## 📅 3. Rezervasyonlar (`/api/Reservations`)

### 3.1 Kullanıcının Rezervasyonları

**Endpoint:** `GET /api/Reservations?status=Pending&page=1&pageSize=20`  
**Authentication:** ✅ Required (Bearer Token)

**Query Parameters:**
- `status` (string, optional): Rezervasyon durumu. Değerler: `"Pending"`, `"Confirmed"`, `"Cancelled"`, `"Completed"`, `"CheckedIn"`
- `page` (int, optional): Sayfa numarası
- `pageSize` (int, optional): Sayfa başına öğe sayısı

**Response (200 OK):**
```json
{
  "items": [
    {
      "id": "reservation-guid",
      "listingId": "listing-guid",
      "listingTitle": {
        "tr": "Sahil Plajı",
        "en": "Sahil Beach"
      },
      "startDate": "2026-07-01",
      "endDate": "2026-07-05",
      "numberOfGuests": 2,
      "totalPrice": 600.00,
      "status": "Confirmed",
      "createdAt": "2026-06-01T10:00:00Z",
      "canCancel": true
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalCount": 5
}
```

---

### 3.2 Rezervasyon Oluşturma

**Endpoint:** `POST /api/Reservations`  
**Authentication:** ✅ Required (Bearer Token)

**Request Body:**
```json
{
  "listingId": "listing-guid",
  "startDate": "2026-07-01",
  "endDate": "2026-07-05",
  "numberOfGuests": 2,
  "specialRequests": "Deniz manzaralı alan lütfen"
}
```

**Response (201 Created):**
```json
{
  "id": "new-reservation-guid",
  "listingId": "listing-guid",
  "startDate": "2026-07-01",
  "endDate": "2026-07-05",
  "numberOfGuests": 2,
  "totalPrice": 600.00,
  "status": "Pending",
  "message": "Rezervasyonunuz oluşturuldu. Ev sahibinin onayı bekleniyor."
}
```

---

### 3.3 Rezervasyon Detayı

**Endpoint:** `GET /api/Reservations/{id}`  
**Authentication:** ✅ Required (Bearer Token)

**Response (200 OK):** Detaylı rezervasyon bilgisi

---

### 3.4 Rezervasyon Güncelleme

**Endpoint:** `PUT /api/Reservations/{id}`  
**Authentication:** ✅ Required (Bearer Token)

**Request Body:**
```json
{
  "startDate": "2026-07-02",
  "endDate": "2026-07-06",
  "numberOfGuests": 3
}
```

**Response (200 OK):** Güncellenmiş rezervasyon

---

### 3.5 Rezervasyon İptali

**Endpoint:** `POST /api/Reservations/{id}/cancel`  
**Authentication:** ✅ Required (Bearer Token)

**Request Body:**
```json
{
  "reason": "Planlarım değişti"
}
```

**Response (200 OK):**
```json
{
  "message": "Rezervasyon iptal edildi",
  "refundAmount": 540.00,
  "refundPercentage": 90
}
```

---

### 3.6 Check-In

**Endpoint:** `POST /api/Reservations/{id}/check-in`  
**Authentication:** ✅ Required (Bearer Token)

**Response (200 OK):**
```json
{
  "message": "Check-in başarılı. İyi eğlenceler!",
  "checkedInAt": "2026-07-01T09:30:00Z"
}
```

---

### 3.7 Müsaitlik Kontrolü

**Endpoint:** `GET /api/Reservations/check-availability?listingId={guid}&startDate=2026-07-01&endDate=2026-07-05`  
**Authentication:** Gerekli değil

**Query Parameters:**
- `listingId` (guid, required): İlan ID
- `startDate` (date, required): Başlangıç tarihi (YYYY-MM-DD)
- `endDate` (date, required): Bitiş tarihi (YYYY-MM-DD)

**Response (200 OK):**
```json
{
  "isAvailable": true,
  "listingId": "listing-guid",
  "startDate": "2026-07-01",
  "endDate": "2026-07-05",
  "priceBreakdown": {
    "basePrice": 150.00,
    "numberOfDays": 4,
    "subtotal": 600.00,
    "serviceFee": 60.00,
    "total": 660.00
  },
  "unavailableDates": []
}
```

---

## 🔍 4. Arama ve Keşif

### 4.1 Gelişmiş Arama

**Endpoint:** `POST /api/Search/listings`  
**Authentication:** Gerekli değil

**Request Body:**
```json
{
  "searchTerm": "plaj",
  "type": "Beach",
  "latitude": 38.4192,
  "longitude": 27.1287,
  "radiusKm": 50,
  "minPrice": 100,
  "maxPrice": 500,
  "amenities": ["Parking", "Shower"],
  "sortBy": "price",
  "sortOrder": "asc",
  "page": 1,
  "pageSize": 20
}
```

**Request Model:**
- `searchTerm` (string, optional): Arama terimi
- `type` (string, optional): İlan türü
- `latitude` (decimal, optional): Konum enlem
- `longitude` (decimal, optional): Konum boylam
- `radiusKm` (decimal, optional): Arama yarıçapı (max 500 km)
- `minPrice` (decimal, optional): Minimum fiyat
- `maxPrice` (decimal, optional): Maksimum fiyat
- `amenities` (string[], optional): İstenen özellikler
- `sortBy` (string, optional): Sıralama kriteri (`"price"`, `"rating"`, `"distance"`)
- `sortOrder` (string, optional): Sıralama yönü (`"asc"`, `"desc"`)
- `page` (int, optional): Sayfa numarası
- `pageSize` (int, optional): Sayfa boyutu

**Response (200 OK):**
```json
{
  "results": [
    {
      "id": "...",
      "title": {...},
      "type": "Beach",
      "pricePerDay": 150.00,
      "rating": 4.5
    }
  ],
  "metadata": {
    "totalResults": 42,
    "page": 1,
    "pageSize": 20,
    "totalPages": 3,
    "appliedFilters": {
      "type": "Beach",
      "priceRange": "100-500"
    }
  }
}
```

---

### 4.2 Arama Önerileri (Autocomplete)

**Endpoint:** `GET /api/Search/suggestions?term=izmir`  
**Authentication:** Gerekli değil

**Query Parameters:**
- `term` (string, required): Arama terimi (min 2 karakter)

**Response (200 OK):**
```json
[
  "İzmir Plajları",
  "İzmir Havuzları",
  "İzmir Tekne Turları"
]
```

---

## 👤 5. Kullanıcı Profili (`/api/Users`)

### 5.1 Profil Bilgilerini Getir

**Endpoint:** `GET /api/Users/me`  
**Authentication:** ✅ Required (Bearer Token)

**Response (200 OK):**
```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "email": "user@example.com",
  "firstName": "Ahmet",
  "lastName": "Yılmaz",
  "phoneNumber": "+905551234567",
  "avatar": "https://storage.example.com/avatars/user.jpg",
  "bio": "Deniz ve güneş severim",
  "joinedDate": "2025-01-01T00:00:00Z",
  "role": "Customer",
  "isVerified": true,
  "settings": {
    "emailNotifications": true,
    "smsNotifications": true,
    "pushNotifications": true,
    "language": "tr",
    "currency": "TRY",
    "profilePublic": true
  }
}
```

---

### 5.2 Profil Güncelleme

**Endpoint:** `PUT /api/Users/me`  
**Authentication:** ✅ Required (Bearer Token)

**Request Body:**
```json
{
  "firstName": "Mehmet",
  "lastName": "Demir",
  "phoneNumber": "+905559876543",
  "bio": "Yeni bio"
}
```

**Response (200 OK):** Güncellenmiş profil bilgisi

---

### 5.3 Avatar Güncelleme

**Endpoint:** `PUT /api/Users/me/avatar`  
**Authentication:** ✅ Required (Bearer Token)  
**Content-Type:** `multipart/form-data`

**Form Data:**
- `file`: Resim dosyası (IFormFile)

**Response (200 OK):**
```json
{
  "avatarUrl": "https://storage.example.com/avatars/new-user.jpg"
}
```

---

### 5.4 Ayarları Güncelleme

**Endpoint:** `PUT /api/Users/me/settings`  
**Authentication:** ✅ Required (Bearer Token)

**Request Body:**
```json
{
  "emailNotifications": false,
  "smsNotifications": true,
  "pushNotifications": true,
  "language": "en",
  "currency": "USD",
  "profilePublic": false
}
```

**Response (204 No Content)**

---

### 5.5 Şifre Değiştirme

**Endpoint:** `PUT /api/Users/me/password`  
**Authentication:** ✅ Required (Bearer Token)

**Request Body:**
```json
{
  "currentPassword": "OldPass123!",
  "newPassword": "NewPass456!",
  "confirmPassword": "NewPass456!"
}
```

**Response (204 No Content)**

**Error Response (400 Bad Request):**
```json
{
  "error": "Mevcut şifre hatalı"
}
```

---

### 5.6 Hesap Silme

**Endpoint:** `DELETE /api/Users/me`  
**Authentication:** ✅ Required (Bearer Token)

**Request Body:**
```json
{
  "password": "CurrentPass123!",
  "reason": "Artık kullanmıyorum"
}
```

**Response (204 No Content)**

---

## ⭐ 6. Favoriler (`/api/Favorites`)

### 6.1 Favori Listesini Getir

**Endpoint:** `GET /api/Favorites?type=Beach`  
**Authentication:** ✅ Required (Bearer Token)

**Query Parameters:**
- `type` (string, optional): Mekan türü filtresi. Değerler: `"Beach"`, `"Pool"`, `"Yacht"`

**Response (200 OK):**
```json
{
  "items": [
    {
      "favoriteId": "favorite-guid",
      "venueId": "venue-guid",
      "venueType": "Beach",
      "title": {
        "tr": "Sahil Plajı",
        "en": "Sahil Beach"
      },
      "photo": "https://storage.example.com/venues/photo.jpg",
      "pricePerDay": 150.00,
      "rating": 4.5,
      "city": "İzmir",
      "addedAt": "2026-01-15T10:00:00Z"
    }
  ],
  "totalCount": 5
}
```

---

### 6.2 Favorilere Ekle

**Endpoint:** `POST /api/Favorites`  
**Authentication:** ✅ Required (Bearer Token)

**Request Body:**
```json
{
  "venueId": "venue-guid",
  "venueType": "Beach"
}
```

**Response (201 Created):**
```json
{
  "favoriteId": "new-favorite-guid",
  "venueId": "venue-guid",
  "venueType": "Beach",
  "addedAt": "2026-02-09T01:15:00Z"
}
```

**Error Responses:**

404 Not Found:
```json
{
  "error": "Venue not found"
}
```

400 Bad Request (zaten favoride):
```json
{
  "error": "Bu mekan zaten favorilerinizde"
}
```

---

### 6.3 Favorilerden Çıkar

**Endpoint:** `DELETE /api/Favorites/{venueId}`  
**Authentication:** ✅ Required (Bearer Token)

**Path Parameters:**
- `venueId` (guid, required): Mekan ID'si

**Response (204 No Content)**

**Error Response (404 Not Found):**
```json
{
  "error": "Favori bulunamadı"
}
```

---

## 📝 7. Yorumlar (`/api/Reviews`)

### 7.1 Yorum Ekleme

**Endpoint:** `POST /api/Reviews`  
**Authentication:** ✅ Required (Bearer Token)

**Request Body:**
```json
{
  "reservationId": "reservation-guid",
  "rating": 5,
  "comment": "Harika bir deneyim! Kesinlikle tavsiye ederim.",
  "photos": [
    "https://storage.example.com/reviews/photo1.jpg"
  ]
}
```

**Request Model:**
- `reservationId` (guid, required): Tamamlanmış rezervasyon ID
- `rating` (int, required): Puan (1-5 arası)
- `comment` (string, optional): Yorum metni
- `photos` (string[], optional): Fotoğraf URL'leri

**Response (201 Created):**
```json
{
  "reviewId": "review-guid",
  "listingId": "listing-guid",
  "rating": 5,
  "comment": "Harika bir deneyim!",
  "photos": [...],
  "createdAt": "2026-07-06T10:00:00Z",
  "userName": "Ahmet Y.",
  "userAvatar": "https://..."
}
```

---

### 7.2 İlan Yorumlarını Getir

**Endpoint:** `GET /api/Reviews/listing/{listingId}?page=1&pageSize=10`  
**Authentication:** Gerekli değil

**Path Parameters:**
- `listingId` (guid, required): İlan ID

**Query Parameters:**
- `page` (int, optional): Sayfa numarası
- `pageSize` (int, optional): Sayfa boyutu

**Response (200 OK):**
```json
{
  "items": [
    {
      "reviewId": "review-guid",
      "rating": 5,
      "comment": "Harika!",
      "photos": [],
      "createdAt": "2026-07-06T10:00:00Z",
      "userName": "Ahmet Y.",
      "userAvatar": "https://...",
      "hostResponse": {
        "comment": "Teşekkürler!",
        "respondedAt": "2026-07-07T12:00:00Z"
      }
    }
  ],
  "averageRating": 4.7,
  "totalCount": 42,
  "page": 1,
  "pageSize": 10
}
```

---

### 7.3 Ev Sahibi Yanıtı

**Endpoint:** `POST /api/Reviews/{id}/host-response`  
**Authentication:** ✅ Required (Bearer Token - Host)

**Request Body:**
```json
{
  "comment": "Yorumunuz için teşekkür ederiz!"
}
```

**Response (200 OK):** Güncellenmiş review objesi

---

## 🏠 8. Ev Sahibi Paneli (`/api/host`)

> **Not:** Bu endpoint'ler sadece `Host` veya `Admin` rolüne sahip kullanıcılar tarafından kullanılabilir.

### 8.1 Dashboard İstatistikleri

**Endpoint:** `GET /api/host/dashboard`  
**Authentication:** ✅ Required (Bearer Token - Host/Admin)

**Response (200 OK):**
```json
{
  "totalListings": 5,
  "activeReservations": 12,
  "pendingReservations": 3,
  "totalRevenue": 15000.00,
  "thisMonthRevenue": 2500.00,
  "upcomingCheckIns": [
    {
      "reservationId": "...",
      "guestName": "Mehmet D.",
      "listingTitle": "Sahil Plajı",
      "checkInDate": "2026-07-01"
    }
  ]
}
```

---

### 8.2 Ev Sahibinin İlanları

**Endpoint:** `GET /api/host/listings?status=active`  
**Authentication:** ✅ Required (Bearer Token - Host/Admin)

**Query Parameters:**
- `status` (string, optional): İlan durumu (`"active"`, `"inactive"`, `"pending"`)

**Response (200 OK):** İlan listesi

---

### 8.3 Fiyatlandırma Güncelleme

**Endpoint:** `PUT /api/host/listings/{id}/pricing`  
**Authentication:** ✅ Required (Bearer Token - Host/Admin)

**Request Body:**
```json
{
  "basePrice": 200.00,
  "weekendPrice": 250.00,
  "customPrices": [
    {
      "date": "2026-07-15",
      "price": 300.00
    }
  ]
}
```

**Response (200 OK):** Güncellenmiş fiyatlandırma bilgisi

---

### 8.4 Takvim Görünümü

**Endpoint:** `GET /api/host/calendar?listingId={guid}&month=2026-07`  
**Authentication:** ✅ Required (Bearer Token - Host/Admin)

**Response (200 OK):**
```json
{
  "month": "2026-07",
  "days": [
    {
      "date": "2026-07-01",
      "isAvailable": false,
      "price": 150.00,
      "reservationId": "...",
      "guestName": "Ahmet Y."
    },
    {
      "date": "2026-07-02",
      "isAvailable": true,
      "price": 150.00
    }
  ]
}
```

---

### 8.5 Kazanç Analitikleri

**Endpoint:** `GET /api/host/analytics?startDate=2026-01-01&endDate=2026-12-31`  
**Authentication:** ✅ Required (Bearer Token - Host/Admin)

**Response (200 OK):**
```json
{
  "totalRevenue": 50000.00,
  "totalReservations": 85,
  "averageReservationValue": 588.24,
  "occupancyRate": 72.5,
  "monthlyBreakdown": [
    {
      "month": "2026-01",
      "revenue": 3500.00,
      "reservations": 8
    }
  ]
}
```

---

## 🛠️ 9. Admin Paneli (`/api/admin`)

> **Not:** Bu endpoint'ler sadece `Admin` rolüne sahip kullanıcılar tarafından kullanılabilir.

### 9.1 Kullanıcı Yönetimi

**Endpoint:** `GET /api/admin/users?role=Host&page=1&pageSize=20`  
**Authentication:** ✅ Required (Bearer Token - Admin)

**Response (200 OK):** Kullanıcı listesi

---

### 9.2 Ev Sahibi Başvuruları

**Endpoint:** `GET /api/admin/host-applications`  
**Authentication:** ✅ Required (Bearer Token - Admin)

**Response (200 OK):** Başvuru listesi

---

### 9.3 İlan Onaylama

**Endpoint:** `POST /api/admin/listings/{id}/approve`  
**Authentication:** ✅ Required (Bearer Token - Admin)

**Response (200 OK):**
```json
{
  "message": "İlan onaylandı ve yayınlandı"
}
```

---

### 9.4 Şehir Yönetimi

**Endpoint:** `POST /api/admin/cities`  
**Authentication:** ✅ Required (Bearer Token - Admin)

**Request Body:**
```json
{
  "name": {
    "tr": "Antalya",
    "en": "Antalya"
  },
  "countryCode": "TR",
  "isActive": true
}
```

**Response (201 Created):** Oluşturulan şehir objesi

---

### 9.5 Özellik Yönetimi

**Endpoint:** `POST /api/admin/amenities`  
**Authentication:** ✅ Required (Bearer Token - Admin)

**Request Body:**
```json
{
  "name": {
    "tr": "Wi-Fi",
    "en": "Wi-Fi"
  },
  "icon": "wifi-icon",
  "category": "Technology"
}
```

**Response (201 Created):** Oluşturulan özellik objesi

---

## 💳 10. Ödeme Yöntemleri (`/api/payment-methods`)

### 10.1 Ödeme Yöntemlerini Listele

**Endpoint:** `GET /api/payment-methods`  
**Authentication:** ✅ Required (Bearer Token)

**Response (200 OK):**
```json
[
  {
    "id": "payment-method-guid",
    "type": "CreditCard",
    "cardBrand": "Visa",
    "last4Digits": "4242",
    "expiryMonth": 12,
    "expiryYear": 2027,
    "cardholderName": "AHMET YILMAZ",
    "isDefault": true,
    "createdAt": "2026-01-01T00:00:00Z"
  }
]
```

---

### 10.2 Ödeme Yöntemi Ekleme

**Endpoint:** `POST /api/payment-methods`  
**Authentication:** ✅ Required (Bearer Token)

**Request Body:**
```json
{
  "cardNumber": "4242424242424242",
  "expiryMonth": 12,
  "expiryYear": 2027,
  "cvv": "123",
  "cardholderName": "AHMET YILMAZ",
  "setAsDefault": true
}
```

**Response (201 Created):** Oluşturulan ödeme yöntemi objesi

---

### 10.3 Ödeme Yöntemi Silme

**Endpoint:** `DELETE /api/payment-methods/{id}`  
**Authentication:** ✅ Required (Bearer Token)

**Response (204 No Content)**

---

### 10.4 Varsayılan Ödeme Yöntemi Belirleme

**Endpoint:** `PUT /api/payment-methods/{id}/default`  
**Authentication:** ✅ Required (Bearer Token)

**Response (200 OK):**
```json
{
  "message": "Varsayılan ödeme yöntemi güncellendi"
}
```

---

## 🌍 11. Diğer Servisler

### 11.1 Sağlık Kontrolü

**Endpoint:** `GET /api/Health`  
**Authentication:** Gerekli değil

**Response (200 OK):**
```json
{
  "status": "Healthy",
  "timestamp": "2026-02-09T01:10:14Z"
}
```

---

### 11.2 Veritabanı Sağlık Kontrolü

**Endpoint:** `GET /api/Health/db`  
**Authentication:** Gerekli değil

**Response (200 OK):**
```json
{
  "status": "Healthy",
  "database": "Connected",
  "responseTime": "45ms"
}
```

---

### 11.3 E-Bültene Kayıt

**Endpoint:** `POST /api/Newsletter/subscribe`  
**Authentication:** Gerekli değil

**Request Body:**
```json
{
  "email": "user@example.com",
  "language": "tr"
}
```

**Response (200 OK):**
```json
{
  "message": "E-bültene başarıyla abone oldunuz"
}
```

---

### 11.4 E-Bülten Aboneliğini İptal

**Endpoint:** `POST /api/Newsletter/unsubscribe`  
**Authentication:** Gerekli değil

**Request Body:**
```json
{
  "email": "user@example.com"
}
```

**Response (200 OK):**
```json
{
  "message": "E-bülten aboneliğiniz iptal edildi"
}
```

---

### 11.5 Destinasyonlar

**Endpoint:** `GET /api/Destinations`  
**Authentication:** Gerekli değil

**Response (200 OK):**
```json
[
  {
    "id": "destination-guid",
    "name": {
      "tr": "İzmir",
      "en": "Izmir"
    },
    "slug": "izmir",
    "photo": "https://storage.example.com/destinations/izmir.jpg",
    "totalVenues": 42,
    "averagePrice": 175.00
  }
]
```

---

### 11.6 Destinasyon Detayı

**Endpoint:** `GET /api/Destinations/{slug}`  
**Authentication:** Gerekli değil

**Path Parameters:**
- `slug` (string, required): Destinasyon slug'ı (örn: "izmir")

**Response (200 OK):**
```json
{
  "id": "destination-guid",
  "name": {
    "tr": "İzmir",
    "en": "Izmir"
  },
  "slug": "izmir",
  "description": {
    "tr": "Ege'nin incisi...",
    "en": "Pearl of the Aegean..."
  },
  "photo": "https://...",
  "venues": [
    {
      "id": "...",
      "title": {...},
      "type": "Beach",
      "pricePerDay": 150.00
    }
  ],
  "totalVenues": 42
}
```

---

## 🚨 Hata Kodları ve Anlamları

| HTTP Kodu | Anlam | Örnek Durum |
|-----------|-------|-------------|
| 200 | OK | İstek başarılı |
| 201 | Created | Kaynak başarıyla oluşturuldu |
| 204 | No Content | İşlem başarılı, dönecek içerik yok |
| 400 | Bad Request | Geçersiz istek parametreleri |
| 401 | Unauthorized | Token geçersiz veya eksik |
| 403 | Forbidden | Yetkisiz erişim denemesi |
| 404 | Not Found | Kaynak bulunamadı |
| 409 | Conflict | Çakışma (örn: e-posta zaten kayıtlı) |
| 500 | Internal Server Error | Sunucu hatası |

---

## 📌 Enum Değerleri

### ListingType
```
- Beach
- Pool
- Yacht
- DayTrip
```

### Role
```
- Customer
- Host
- Admin
```

### ReservationStatus
```
- Pending (Onay bekliyor)
- Confirmed (Onaylandı)
- CheckedIn (Giriş yapıldı)
- Completed (Tamamlandı)
- Cancelled (İptal edildi)
```

### VenueType (Favoriler için)
```
- Beach
- Pool
- Yacht
```

---

## 🔧 Çoklu Dil Desteği

API'de çoklu dil desteklenen alanlar şu formatta döner:

```json
{
  "title": {
    "tr": "Sahil Plajı",
    "en": "Sahil Beach",
    "de": "Sahil Strand"
  }
}
```

Frontend'de kullanıcının seçili diline göre gösterim yapabilirsiniz:
```typescript
// Örnek kullanım
const title = listing.title[currentLanguage] || listing.title.tr;
```

---

## 🎯 Pagination Pattern

Sayfalama destekleyen endpoint'ler şu formatta geri döner:

```json
{
  "items": [...],
  "page": 1,
  "pageSize": 20,
  "totalCount": 150,
  "totalPages": 8
}
```

---

## ⚡ Rate Limiting

API rate limiting middleware'i aktiftir. Her IP için:
- **100 istek / dakika** limiti vardır
- Limit aşımında `429 Too Many Requests` hatası döner

---

## 🔒 Güvenlik Notları

1. **HTTPS Kullanımı:** Production'da her zaman HTTPS kullanın
2. **Token Saklama:** Token'ları güvenli şekilde saklayın (localStorage yerine httpOnly cookie tercih edilebilir)
3. **Token Yenileme:** Token süresi dolmadan önce refresh endpoint'ini kullanarak yenileyin
4. **CORS:** API, frontend domain'inize izin verecek şekilde yapılandırılmalıdır

---

## 📞 Destek

Herhangi bir sorunla karşılaşırsanız:
- **API Documentation:** `/swagger` endpoint'ini ziyaret edin
- **Support Email:** support@swimago.com

---

**Son Güncelleme:** 2026-02-09
