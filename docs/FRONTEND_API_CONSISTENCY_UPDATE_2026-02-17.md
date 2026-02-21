# Frontend API Consistency Update (Customer + Host Panel + Admin)

- Tarih: 2026-02-17
- Kapsam: `customer`, `host-panel`, `admin` endpointlerinin tutarlı çalışması için backend refactor çıktısı
- Base URL: `/api`

## 1) Özet

Bu güncelleme ile:
- Admin endpointleri mock/boş dönme davranışından çıkarılıp gerçek veriye bağlandı.
- Admin tarafındaki mutasyon endpointlerinde kalıcı kayıt (`SaveChanges`) garanti altına alındı.
- Host panel endpointleri `Host` + `Admin` token ile erişilebilir hale getirildi.
- Host listing status akışı, `Draft/PendingReview` gibi durumları kırmadan `pending` ile normalize edecek şekilde güncellendi.
- Customer token ile listing yazma akışları engellendi (sadece host/admin).

## 2) Değişen Davranışlar

## 2.1 Auth / Policy

### Değişiklik
- Yeni policy: `HostOrAdmin` (`Role=Host` veya `Role=Admin`)
- `GET/POST/PUT/PATCH /api/host/**` artık `HostOrAdmin` ile korunuyor.
- `POST /api/listings`
- `POST /api/listings/photos/upload`
- `POST /api/listings/{id}/publish`
  artık `HostOrAdmin` korumasında.

### Frontend Etkisi
- Customer token ile listing create/publish/photo upload artık `403` döner.
- Bu akışlar host onboarding sonrası host/admin token ile çağrılmalı.

## 2.2 Host Listing Status Normalize

### Değişiklik
Host servisinde status parse/map genişletildi:
- Kabul edilen ek inputlar: `draft`, `pendingreview`, `pending-review`
- Response tarafında `Draft` ve `PendingReview` => `pending`
- `GET /api/host/listings?status=pending` artık DB’deki `Pending + Draft + PendingReview` kayıtlarını birlikte döner.

### Frontend Etkisi
- Host panel tarafında mevcut status union (`active|pending|inactive|rejected`) korunabilir.
- Ek status map yazmaya gerek yok; backend normalize ediyor.

## 2.3 Host Request Toleransı

### Değişiklik
- Listing type parse: `daytrip` alias desteği eklendi (`day-trip` ile birlikte).
- Reservation source parse: `walkin` alias desteği eklendi (`walk-in` ile birlikte).

### Frontend Etkisi
- Eski payload varyasyonları daha toleranslı çalışır.
- Öneri: standart formatta kal (`day-trip`, `walk-in`).

## 2.4 Admin Core Endpoints (Artık Gerçek Veri)

Aşağıdaki endpointlerde mock/boş response yerine gerçek repository verisi dönülür:
- `GET /api/admin/dashboard`
- `GET /api/admin/users`
- `GET /api/admin/users/{id}`
- `GET /api/admin/host-applications`
- `GET /api/admin/listings`
- `GET /api/admin/reports`

### Frontend Etkisi
- Liste ekranlarında artık gerçek `totalCount`, `counts`, `items` dolu gelir.
- Boş-state fallback’leri korunmalı ama artık normal senaryo dolu veri olacaktır.

## 2.5 Admin Mutasyonları Kalıcı Hale Geldi

Aşağıdaki endpointler artık DB’ye kalıcı yazıyor:
- `PUT /api/admin/users/{id}/status`
- `PUT /api/admin/users/{id}/role`
- `POST /api/admin/host-applications/{userId}/reject`
- `POST /api/admin/listings/{id}/approve`
- `POST /api/admin/listings/{id}/reject`
- `POST /api/admin/cities`
- `POST /api/admin/amenities`

### Frontend Etkisi
- Optimistic update sonrası refetch yapılırsa artık değişiklikler kalıcı görünür.
- Eski “başarılı görünüp geri dönünce kaybolma” problemi kapanır.

## 2.6 Admin Host Application Reject Hata Semantiği

### Değişiklik
`POST /api/admin/host-applications/{userId}/reject` için:
- `204`: başarılı
- `400`: geçersiz iş akışı (örn. kullanıcı host başvurusu değil)
- `404`: kullanıcı bulunamadı

Hata formatı:
```json
{ "error": "..." }
```

### Frontend Etkisi
- Bu endpoint için `400` ve `404` ayrı mesajlanmalı.

## 3) Frontend Uygulama Checklist

1. Host panel API client’ında host endpointleri için `Host` ve `Admin` tokenını destekleyin.
2. Customer uygulamasında listing create/publish/upload çağrıları customer token ile yapılıyorsa host flow’a taşıyın.
3. Admin dashboard/users/listings/reports ekranlarında mock bağımlılığı varsa kaldırın; doğrudan API response’u bağlayın.
4. Admin mutasyonlarından sonra (status/role/approve/reject/create city/create amenity) liste refetch zorunlu yapın.
5. `host-applications/reject` için `400` ve `404` error branch’lerini ayrı handle edin.
6. Host listing filtrelerinde `pending` sonucu artık `Draft/PendingReview` kayıtlarını da içereceği için ekstra frontend merge logic’i kaldırın.

## 4) Kontrat Notları

- JSON hata gövdesi standartı bu güncellemede de korunmuştur:
```json
{ "error": "..." }
```
- Var olan endpoint path’lerinde breaking URL değişikliği yoktur.
- Esas farklar: auth policy sıkılaştırması + status normalize + admin data/persistence davranışı.
