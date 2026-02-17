# Admin Panel API - Frontend Consume Dokumani (TR)

Bu dokuman, admin panel frontend'inin backend API'leri dogrudan ve tutarli sekilde consume etmesi icin hazirlandi.

- Guncelleme tarihi: `2026-02-17`
- API base URL: `https://<your-domain>/api`
- Admin endpoint base: `https://<your-domain>/api/admin`
- Auth: `Bearer JWT` (sadece `Role=Admin` tokeni)

## 1. Kimlik Dogrulama ve Header Standardi

### 1.1 Admin Login

- Method: `POST`
- Endpoint: `/api/auth/login/admin`
- Content-Type: `application/json`

Request:

```json
{
  "email": "admin@swimago.com",
  "password": "your-password"
}
```

Response (`200`):

```json
{
  "userId": "9dd0a4f5-f99a-45a5-a14b-6f32f6ea8f40",
  "email": "admin@swimago.com",
  "firstName": "Admin",
  "lastName": "User",
  "avatar": null,
  "role": "Admin",
  "token": "<jwt>",
  "refreshToken": "<refresh-token>",
  "tokenExpiry": "2026-02-17T10:30:00Z",
  "settings": null
}
```

### 1.2 Token Refresh

- Method: `POST`
- Endpoint: `/api/auth/refresh`

Request:

```json
{
  "refreshToken": "<refresh-token>"
}
```

### 1.3 Tum Admin Cagrilari Icin Header

```http
Authorization: Bearer <jwt>
Content-Type: application/json
```

Not:
- Token var ama `Role=Admin` degilse `403 Forbidden` doner.
- Token yok/hataliysa `401 Unauthorized` doner.

## 2. Enum Degerleri (Frontend icin)

### Role
- `Admin`
- `Host`
- `Customer`

### UserStatus
- `Active`
- `Banned`
- `Pending`

### ListingStatus
- `Pending`
- `Active`
- `Inactive`
- `Rejected`

### ListingType / VenueType
- `Beach`
- `Pool`
- `Yacht`
- `DayTrip`

## 3. Ortak Response Kaliplari

### 3.1 PaginatedResponse<T>

```json
{
  "items": [],
  "totalCount": 0,
  "page": 1,
  "pageSize": 10,
  "totalPages": 0
}
```

### 3.2 Hata Formatlari

Controller bazli islem hatalarinda sik gorulen:

```json
{
  "error": "Destination not found"
}
```

Global exception middleware formati:

```json
{
  "statusCode": 400,
  "message": "Validation failed.",
  "details": null,
  "validationErrors": {
    "fieldName": ["error text"]
  }
}
```

## 4. Frontend Service Modulleri ve Endpoint Haritasi

## `adminDashboardService`
- `GET /api/admin/dashboard`

## `adminUsersService`
- `GET /api/admin/users`
- `GET /api/admin/users/{id}`
- `PUT /api/admin/users/{id}/status`
- `PUT /api/admin/users/{id}/role`

## `adminHostApplicationsService`
- `GET /api/admin/host-applications`
- `POST /api/admin/host-applications/{userId}/reject`

## `adminListingModerationService`
- `GET /api/admin/listings`
- `POST /api/admin/listings/{id}/approve`
- `POST /api/admin/listings/{id}/reject`

## `adminReportsService`
- `GET /api/admin/reports?start=<iso>&end=<iso>`

## `adminMasterDataService`
- `GET /api/admin/cities`
- `POST /api/admin/cities`
- `GET /api/admin/amenities`
- `POST /api/admin/amenities`

## `adminDestinationsService`
- `GET /api/admin/destinations`
- `GET /api/admin/destinations/{id}`
- `POST /api/admin/destinations`
- `PUT /api/admin/destinations/{id}`
- `DELETE /api/admin/destinations/{id}`

## `adminBlogsService`
- `GET /api/admin/blogs`
- `GET /api/admin/blogs/{id}`
- `POST /api/admin/blogs`
- `PUT /api/admin/blogs/{id}`
- `DELETE /api/admin/blogs/{id}`

## `adminBeachesService`
- `GET /api/admin/beaches`
- `GET /api/admin/beaches/{id}`
- `POST /api/admin/beaches`
- `PUT /api/admin/beaches/{id}`
- `DELETE /api/admin/beaches/{id}`

## `adminMediaService`
- `POST /api/admin/media/upload` (multipart/form-data)
- `POST /api/admin/media/upload-multiple` (multipart/form-data)
- `DELETE /api/admin/media/{fileName}`

## 5. Endpoint Detaylari

## 5.1 Dashboard

### GET `/api/admin/dashboard`
- Response: `AdminDashboardResponse`
- Alanlar:
  - `stats`
  - `revenue`
  - `recentActivity[]`
  - `systemHealth`

Not: Su an backend bu endpointte mock veri donuyor.

## 5.2 Users

### GET `/api/admin/users`
Query:
- `role?: Admin|Host|Customer`
- `status?: Active|Banned|Pending`
- `search?: string`
- `page?: number` (default `1`)
- `pageSize?: number` (default `10`)

Response:
- `users[]`
- `totalCount`
- `counts { total, admins, hosts, customers, active, banned, pending }`

### GET `/api/admin/users/{id}`
Response: tek kullanici detayi + activity + recentReservations + recentReviews

### PUT `/api/admin/users/{id}/status`
Request:

```json
{
  "status": "Banned",
  "reason": "Policy violation"
}
```

Response: `204 No Content`

### PUT `/api/admin/users/{id}/role`
Request:

```json
{
  "role": "Host"
}
```

Response: `204 No Content`

## 5.3 Host Application Isleri

### GET `/api/admin/host-applications`
Response:
- `applications[]`
- `totalCount`
- `pendingCount`

### POST `/api/admin/host-applications/{userId}/reject`
Request:

```json
{
  "reason": "Missing required documents"
}
```

Response: `204 No Content`

## 5.4 Listing Moderation

### GET `/api/admin/listings`
Query:
- `status?: Pending|Active|Inactive|Rejected`
- `search?: string`
- `page?: number` (default `1`)
- `pageSize?: number` (default `10`)

Response:
- `listings[]`
- `totalCount`
- `counts { total, active, pending, inactive, rejected }`

### POST `/api/admin/listings/{id}/approve`
Response: `204 No Content`

### POST `/api/admin/listings/{id}/reject`
Request:

```json
{
  "reason": "Invalid pricing data"
}
```

Response: `204 No Content`

## 5.5 Reports

### GET `/api/admin/reports`
Query (zorunlu):
- `start` (ISO tarih/saat)
- `end` (ISO tarih/saat)

Ornek:
- `/api/admin/reports?start=2026-02-01T00:00:00Z&end=2026-02-17T23:59:59Z`

Response:
- `currentPeriod`
- `previousPeriod`
- `dailyData[]`
- `topVenues[]`
- `topHosts[]`

## 5.6 Master Data

### GET `/api/admin/cities`
Response:
- `cities[]`
- `totalCount`

### POST `/api/admin/cities`
Request:

```json
{
  "name": "Bodrum",
  "country": "Turkey",
  "slug": "bodrum",
  "isActive": true
}
```

Response: `204 No Content`

### GET `/api/admin/amenities`
Response:
- `amenities[]`
- `totalCount`

### POST `/api/admin/amenities`
Request:

```json
{
  "name": "Shower",
  "icon": "shower",
  "category": "facility",
  "isActive": true
}
```

Response: `204 No Content`

## 5.7 Destinations CRUD

### GET `/api/admin/destinations`
Query:
- `search?: string`
- `country?: string`
- `isFeatured?: boolean`
- `page?: number` (default `1`)
- `pageSize?: number` (default `10`)

Response: `PaginatedResponse<DestinationListItemDto>`

### GET `/api/admin/destinations/{id}`
Response: `DestinationDetailDto`

### POST `/api/admin/destinations`
Request (`CreateDestinationRequest`):

```json
{
  "name": "Bodrum",
  "slug": "bodrum",
  "country": "Turkey",
  "description": "Popular destination...",
  "subtitle": "Aegean coast",
  "imageUrl": "https://cdn.example.com/destination.jpg",
  "mapImageUrl": "https://cdn.example.com/map.jpg",
  "latitude": 37.0344,
  "longitude": 27.4305,
  "avgWaterTemp": "22C",
  "sunnyDaysPerYear": 290,
  "tags": ["summer", "beach"],
  "isFeatured": true,
  "features": [
    {
      "icon": "beach_access",
      "title": "Great Beaches",
      "description": "Crystal clear water"
    }
  ]
}
```

Response: `201 Created` + `DestinationDetailDto`

### PUT `/api/admin/destinations/{id}`
Request: `CreateDestinationRequest` ile ayni
Response: `200 OK` + `DestinationDetailDto`

### DELETE `/api/admin/destinations/{id}`
Response: `204 No Content`

## 5.8 Blogs CRUD

### GET `/api/admin/blogs`
Query:
- `search?: string`
- `category?: string`
- `isPublished?: boolean`
- `page?: number` (default `1`)
- `pageSize?: number` (default `10`)

Response: `PaginatedResponse<BlogListItemDto>`

### GET `/api/admin/blogs/{id}`
Response: `BlogDetailDto`

### POST `/api/admin/blogs`
Request (`CreateBlogRequest`):

```json
{
  "title": "Top 10 Swim Spots",
  "slug": "top-10-swim-spots",
  "description": "Guide content",
  "imageUrl": "https://cdn.example.com/cover.jpg",
  "heroImageUrl": "https://cdn.example.com/hero.jpg",
  "category": "guides",
  "tags": ["travel", "swim"],
  "author": {
    "name": "Admin Team",
    "bio": "Editorial",
    "avatarUrl": "https://cdn.example.com/avatar.jpg"
  },
  "readTime": "8 min read",
  "content": [
    {
      "type": "paragraph",
      "text": "Content body..."
    }
  ],
  "tableOfContents": [
    {
      "id": "intro",
      "title": "Introduction"
    }
  ],
  "isFeatured": false,
  "isPublished": true,
  "publishedAt": "2026-02-17T08:00:00Z"
}
```

Response: `201 Created` + `BlogDetailDto`

### PUT `/api/admin/blogs/{id}`
Request: `CreateBlogRequest` ile ayni
Response: `200 OK` + `BlogDetailDto`

### DELETE `/api/admin/blogs/{id}`
Response: `204 No Content`

## 5.9 Beaches CRUD (Admin Listings)

### GET `/api/admin/beaches`
Query:
- `search?: string`
- `city?: string`
- `isActive?: boolean`
- `minPrice?: number`
- `maxPrice?: number`
- `page?: number` (default `1`)
- `pageSize?: number` (default `10`)

Response: `PaginatedResponse<ListingListItemDto>`

### GET `/api/admin/beaches/{id}`
Response: `BeachDetailDto`

### POST `/api/admin/beaches`
Request (`CreateBeachRequest`):

```json
{
  "name": { "tr": "Gumbet", "en": "Gumbet" },
  "slug": "gumbet",
  "description": { "tr": "Aciklama", "en": "Description" },
  "city": "Bodrum",
  "country": "Turkey",
  "latitude": 37.03,
  "longitude": 27.40,
  "locationSubtitle": "Near center",
  "mapImageUrl": "https://cdn.example.com/map.jpg",
  "pricePerDay": 120,
  "currency": "USD",
  "priceUnit": "day",
  "images": [
    {
      "url": "https://cdn.example.com/beach1.jpg",
      "alt": "beach",
      "order": 0,
      "isPrimary": true
    }
  ],
  "conditions": {
    "windSpeed": "5-10 knots",
    "waterDepth": "1.5m",
    "groundType": "Sand",
    "waveStatus": "Calm"
  },
  "amenities": [
    {
      "icon": "pool",
      "label": { "tr": "Dus", "en": "Shower" },
      "available": true
    }
  ],
  "rareFindMessage": "Last spot",
  "isActive": true,
  "isFeatured": false,
  "breadcrumbs": [
    { "label": "Home", "link": "/" },
    { "label": "Beaches", "link": "/beaches" }
  ]
}
```

Response: `201 Created` + `BeachDetailDto`

### PUT `/api/admin/beaches/{id}`
Request: `CreateBeachRequest` ile ayni
Response: `200 OK` + `BeachDetailDto`

### DELETE `/api/admin/beaches/{id}`
Response: `204 No Content`

## 5.10 Media Upload

### POST `/api/admin/media/upload`
- Content-Type: `multipart/form-data`
- Form fields:
  - `file` (single file)
  - `folder` (ornek: `blogs`, `destinations`, `beaches`)

Response:

```json
{
  "url": "https://supabase-project.supabase.co/storage/v1/object/public/images/blogs/uuid_name.jpg",
  "fileName": "uuid_name.jpg",
  "fileSize": 123456
}
```

### POST `/api/admin/media/upload-multiple`
- Content-Type: `multipart/form-data`
- Form fields:
  - `files` (multi file)
  - `folder`

Response: `MediaUploadResponse[]`

### DELETE `/api/admin/media/{fileName}`
Response: `204 No Content`

## 6. Frontend icin TypeScript Tipleri (Onerilen)

```ts
export type Role = "Admin" | "Host" | "Customer";
export type UserStatus = "Active" | "Banned" | "Pending";
export type ListingStatus = "Pending" | "Active" | "Inactive" | "Rejected";
export type VenueType = "Beach" | "Pool" | "Yacht" | "DayTrip";

export interface AuthResponse {
  userId: string;
  email: string;
  firstName: string;
  lastName: string;
  avatar: string | null;
  role: Role;
  token: string;
  refreshToken: string;
  tokenExpiry: string;
}

export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}
```

## 7. Onerilen API Client Pattern

```ts
const API_BASE = `${import.meta.env.VITE_API_BASE_URL}/api`;

async function apiFetch<T>(path: string, init: RequestInit = {}): Promise<T> {
  const token = localStorage.getItem("admin_access_token");
  const headers = new Headers(init.headers);
  headers.set("Content-Type", headers.get("Content-Type") ?? "application/json");
  if (token) headers.set("Authorization", `Bearer ${token}`);

  const res = await fetch(`${API_BASE}${path}`, { ...init, headers });
  if (!res.ok) {
    const errorBody = await res.json().catch(() => ({}));
    throw new Error(errorBody?.error ?? errorBody?.message ?? `HTTP ${res.status}`);
  }

  if (res.status === 204) return undefined as T;
  return res.json() as Promise<T>;
}
```

## 8. Bilinen Durumlar / Backend Notlari

Frontend planlamasi icin kritik:

- `GET /api/admin/dashboard` su anda mock veri donuyor.
- `GET /api/admin/users`, `GET /api/admin/host-applications`, `GET /api/admin/listings`, `GET /api/admin/reports` su an cok sinirli/stub veri donuyor.
- `POST /api/admin/cities` endpointi `204` donse de backend implementation'i su an persistence yapmiyor (no-op).
- `Admin listings` tarafinda su an route olarak sadece `beaches` expose edildi. `pool/yacht/day-trip` icin admin endpoint route'u yok.
- `Admin media` su an mock URL uretimi yapiyor; gercek storage integrasyonu tamamlandiginda response URL davranisi degisebilir.

## 9. Kaynak Kod Referanslari

- Auth: `src/Swimago.API/Controllers/AuthController.cs`
- Admin genel: `src/Swimago.API/Controllers/AdminController.cs`
- Admin destinations: `src/Swimago.API/Controllers/Admin/AdminDestinationsController.cs`
- Admin blogs: `src/Swimago.API/Controllers/Admin/AdminBlogsController.cs`
- Admin beaches: `src/Swimago.API/Controllers/Admin/AdminListingsController.cs`
- Admin media: `src/Swimago.API/Controllers/Admin/AdminMediaController.cs`
- Admin DTO'lar: `src/Swimago.Application/DTOs/Admin/AdminDtos.cs`
- Destination DTO'lar: `src/Swimago.Application/DTOs/Admin/Destinations/DestinationDtos.cs`
- Blog DTO'lar: `src/Swimago.Application/DTOs/Admin/Blogs/BlogDtos.cs`
- Listing DTO'lar: `src/Swimago.Application/DTOs/Admin/Listings/ListingDtos.cs`
- Shared DTO'lar: `src/Swimago.Application/DTOs/Admin/Shared/SharedDtos.cs`

