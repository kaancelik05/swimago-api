# Swimago Genel Teknik ve Kullanim Dokumani

Son guncelleme: **2026-02-18**  
Bu dokuman, asagidaki iki kaynagin birlestirilmis ve normalize edilmis halidir:
- `docs/SWIMAGO_FRONTEND_ANALIZI.md` (analiz tarihi: 2026-02-17)
- `docs/BACKEND_DB_DOKUMANI_TR.md` (son guncelleme: 2026-02-18)

Amac:
- Swimago urununu tek dokumanda hem **kullanim (is akislar)** hem de **teknik mimari** acisindan anlatmak
- Customer, Host ve Admin panellerinin backend/DB ile iliskisini netlestirmek
- Ekip onboarding, gelistirme, entegrasyon ve hardening sureclerine ortak bir referans saglamak

---

## 1) Swimago Nedir?

Swimago; beach, pool, yacht ve day-trip odakli bir rezervasyon platformudur. Platform 3 farkli uygulama yuzunden olusur:

1. **Customer uygulamasi**: son kullanicinin kesif, rezervasyon, favori ve profil islemleri
2. **Host panel**: isletmenin listing, rezervasyon, takvim, fiyat ve operasyon yonetimi
3. **Admin panel**: platform yonetimi, icerik/master data ve moderasyon

Tek backend API ve ortak PostgreSQL/PostGIS veritabani uzerinden calisir.

---

## 2) Sistem Topolojisi (Ust Seviye)

### 2.1 Frontend katmani

- Monorepo: Nx tabanli Angular repo
- Uygulamalar:
  - `apps/customer` (SSR + Express)
  - `apps/host-panel` (SPA)
  - `apps/admin-panel` (SPA)
- Temel stack:
  - Angular 21, Nx 22, TypeScript 5.9, RxJS 7.8
  - TailwindCSS (ozellikle customer)
  - Transloco (customer + host)
  - Leaflet (customer explore/map)

Calistirma:
- `npm run start:customer` -> `http://localhost:4200`
- `npm run start:host` -> `http://localhost:4201`
- `npm run start:admin` -> `http://localhost:4202`
- `npm run start:all` -> uc uygulamayi paralel acar

### 2.2 Backend katmani

- Mimari: Clean Architecture
  - Domain
  - Application
  - Infrastructure
  - API
- Runtime:
  - JWT authentication + policy authorization
  - Exception, security headers ve rate limit middleware'leri
- API root: `http://localhost:5088/api` (mevcut kod gercegi)

### 2.3 Veritabani katmani

- PostgreSQL + PostGIS
- EF Core code-first migration
- JSONB kolonlarla cok dilli ve esnek modelleme
- GiST/GIN index kullanimi ile geo ve text sorgu optimizasyonu

---

## 3) Roller, Yetkiler ve Erisim Modeli

Roller:
- `Customer`
- `Host`
- `Admin`

Backend policy'leri:
- `CustomerOnly`
- `HostOnly`
- `HostOrAdmin`
- `AdminOnly`

Frontend guard davranisi:
- Customer app: customer route'lari customer role ile korunur
- Host panel: host/admin role kabul eder
- Admin panel: admin role zorunlu

Not: Frontend ve backend'de role tabanli model genel olarak uyumludur.

---

## 4) Uygulama Bazli Kullanim Senaryolari

### 4.1 Customer (B2C) akislari

Ana amac: kesif -> detay -> teklif/fiyat -> rezervasyon -> profil/favori/odeme.

Ana route gruplari:
- Kesif: `/`, `/destinations`, `/explore`
- Spot detay: `/beach/:slug`, `/pool/:slug`
- Boat tours: `/boat-tours`, detay route'lari
- Blog: `/blog`, `/blog/:slug`
- Auth: `/login`, `/signup`, `/forgot-password`, `/reset-password`
- Profil: `/profile/*`
- Host olma/listing olusturma wizard'i: `/become-host/*`

Kritik kullanim adimlari:
1. Kullanici destination/spot kesfeder
2. Spot detayinda quote alir (`/spots/{slug}/quote`)
3. Rezervasyon olusturur (`/reservations`)
4. Profilde rezervasyon/favori/odeme islemleri yapar

### 4.2 Host panel akislari

Ana amac: listing ve rezervasyon operasyonunu yonetmek.

Ana route gruplari:
- `/dashboard`
- `/listings`, `/listings/new`, `/listings/:id/edit`
- `/reservations`
- `/quick-reservation`
- `/calendar`
- `/analytics`
- `/business-settings`

Kritik kullanim adimlari:
1. Host listing olusturur/gunceller
2. Rezervasyonlari filtreler, status degistirir
3. Manuel rezervasyon girer (phone/walk-in)
4. Takvim ve gunluk fiyat/musaitlik ayari yapar
5. Analitik metrikleri takip eder

### 4.3 Admin panel akislari

Ana amac: platform yonetimi ve icerik/master data operasyonu.

Ana route gruplari:
- Destination CRUD
- Beach CRUD
- Pool CRUD
- Blog CRUD
- Boat tour formlari (kismi/mock)

Kritik kullanim adimlari:
1. Destination, beach, pool ve blog iceriklerini yonetir
2. Moderasyon ve raporlama endpointlerini backend uzerinden kullanir
3. Admin auth ile platform yonetimine girer

Not: UI olgunlugu backend kapsamina gore daha dardir; bazi admin servisleri backend'de mevcut olsa da panelde aktif sayfasi yoktur.

---

## 5) Frontend-Backend Entegrasyon Haritasi

| Domain | Customer | Host | Admin | Ana Endpoint Prefixleri |
|---|---|---|---|---|
| Auth | Evet | Evet | Evet | `/auth/*` |
| Destinations | Evet | - | Evet | `/destinations`, `/admin/destinations` |
| Listings/Search | Evet | Evet | Kismen | `/search/listings`, `/listings`, `/host/listings`, `/admin/listings` |
| Spots/Quote | Evet | - | - | `/spots/*` |
| Reservations | Evet | Evet | Dolayli | `/reservations*`, `/host/reservations*` |
| Favorites | Evet | - | - | `/favorites*` |
| Users/Profile | Evet | - | Admin user mgmt backend'de | `/users/me*`, `/admin/users*` |
| Payment methods | Evet | - | - | `/payment-methods*` |
| Blog | Evet | - | Evet | `/blog*`, `/admin/blogs*` |
| Analytics/Reports | Profil dashboard | Evet | Evet (backend) | `/host/analytics`, `/admin/reports` |

Not:
- Frontend analizinde bazi endpointler `/Auth/*` olarak geciyor; backend kodunda pratikte `/api/auth/*` kucuk harf standardi var.
- Uretimde tek bir endpoint naming standardina sabitlenmesi gerekir.

---

## 6) API Ozeti (Kullanim + Teknik)

### 6.1 Public ve customer endpointleri

- Auth: register/login/refresh/logout/forgot/reset
- Kesif: destinations, spots, boat-tours, search, explore
- Islem: reservations, favorites, payment-methods, profile
- Icerik: blog, newsletter

### 6.2 Host endpointleri

- Host listing CRUD + status
- Host reservation list + status update + manual create
- Host calendar + business settings + analytics + dashboard cards

### 6.3 Admin endpointleri

- User management (liste/detay/role/status)
- Host applications moderation
- Listing moderation approve/reject
- Destination, beach, blog CRUD
- City/amenity CRUD
- Reports + dashboard
- Media upload/delete

Toplam backend endpoint sayisi (kaynak dokumana gore): **123**

---

## 7) Veritabani Modeli (Domain Bazli)

### 7.1 Kimlik ve hesap

- `Users`
- `UserProfiles`
- `PaymentMethods`
- `Notifications`

### 7.2 Listing ve icerik

- `Listings`
- `ListingImages`
- `Amenities`
- `ListingAmenities`
- `HostListingMetadata`
- `DailyPricings`
- `AvailabilityBlocks`

### 7.3 Rezervasyon ve yorum

- `Reservations`
- `ReservationPayments`
- `Reviews`
- `Favorites`

### 7.4 Destination ve blog

- `Cities`
- `Destinations`
- `BlogPosts`
- `BlogComments`
- `NewsletterSubscribers`
- `HostBusinessSettings`

Temel teknik noktalar:
- Geo alanlar: PostGIS (`Location`, GiST index)
- Coklu dil ve semistructured veri: JSONB
- Kritik unique/index:
  - slug alanlari
  - confirmation number
  - favorites composite unique
  - host settings ve metadata 1:1 unique

---

## 8) Uctan Uca Is Akislari (Ornekler)

### 8.1 Spot rezervasyonu (customer)

1. `GET /destinations` veya `GET /search/listings`
2. `GET /spots/{slug}`
3. `POST /spots/{slug}/quote`
4. Login (gerekirse)
5. `POST /reservations`
6. `POST /reservations/{id}/payment-intent`
7. Profil ekranlarinda rezervasyon ve odeme takibi

### 8.2 Listing yayinlama (host)

1. Host login
2. `POST /host/listings` veya customer become-host akisi ile `/listings`
3. Fotograf yukleme (`/listings/photos/upload`)
4. `POST /listings/{id}/publish`
5. Admin moderasyonundan sonra listing aktif duruma gecis

### 8.3 Admin icerik yonetimi

1. Admin login
2. Destination/beach/pool/blog CRUD endpointleri
3. Gerekirse media upload
4. Platform geneli kalite/moderasyon raporlari

---

## 9) Guvenlik, Dayaniklilik ve Operasyon

Aktif mekanizmalar:
- JWT Bearer auth
- Role/policy authorization
- Rate limit: 100 req/dk (user veya IP bazli)
- Security headers middleware
- Merkezi exception middleware

Dikkat edilmesi gerekenler:
- Konfigte gorunen gercek secret/connection string benzeri degerler production secret manager'a alinmali
- Logout token invalidation/blacklist yok
- Auth forgot/reset ve bazi account guvenlik akislarinda TODO izleri var

---

## 10) Bilinen Bosluklar ve Teknik Borc

### 10.1 Frontend tarafi

- Customer'da bazi UI aksiyonlari placeholder/TODO
- Route tutarsizligi: `/profile/settings` vs `/profile/account-settings`
- Admin boat-tour formlari kalici API'ye bagli degil (kismi mock)
- Host/Admin token refresh akisi customer kadar oturmus degil

### 10.2 Backend tarafi

- Forgot/reset password tam token-email akisi tamamlanmamis
- Password degisimi serviste TODO izleri
- Admin listing service'te pool/yacht/day-trip metotlari implement degil
- Blog update/delete yetki kuralinda admin author eslesmesi sorunu
- Search distance sort davranisi gercek geo mesafe yerine placeholder
- Host manual reservation overlap kontrolu zayif
- Reservation overlap kuralinda `Rejected` gibi statuler de cakisiyor sayilabiliyor

---

## 11) Standardizasyon ve Iyilestirme Onerileri

1. **API naming birligi**
   - `/auth` casing standardini tum frontendlerde sabitle
2. **Ortak hata modeli**
   - `code/message/details/validationErrors` sabit response semasi
3. **Pagination standardi**
   - `items`, `total`, `page`, `pageSize`
4. **Token refresh davranisi**
   - Customer/Host/Admin taraflarinda tek davranis seti
5. **Admin panel kapsami**
   - Backend'de hazir ama UI'da bagli olmayan modulleri asamali ac
6. **Guvenlik hardening**
   - Secret management, audit log, token invalidation stratejisi
7. **Is kurali netlestirme**
   - Reservation overlap ve status gecis matrisi

---

## 12) Gelistirici Onboarding Ozeti

1. Repo clone ve bagimliliklar: `npm install`
2. Uygulamalari baslat:
   - Customer: `npm run start:customer`
   - Host: `npm run start:host`
   - Admin: `npm run start:admin`
3. API base URL kontrolu:
   - Default `http://localhost:5088/api`
   - Host/Admin runtime override destegi var
4. Yetki testleri:
   - Customer/Host/Admin login endpointlerini role bazli test et
5. Kritik smoke senaryolari:
   - Customer rezervasyon
   - Host listing + reservation operasyonu
   - Admin destination/blog CRUD

---

## 13) Referans Dokumanlar

- `docs/SWIMAGO_FRONTEND_ANALIZI.md`
- `docs/BACKEND_DB_DOKUMANI_TR.md`
- `docs/API_DOCUMENTATION_TR.md`
- `docs/API_FRONTEND_TUKETIM_DOKUMANI_TR.md`

Bu dokuman, Swimago'nun urun akislarini teknik implementasyonla birlikte tek cati altinda anlamak ve ekipler arasi ortak dili korumak icin hazirlanmistir.
