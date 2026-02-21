# Swimago Frontend Analizi

Bu dokuman, `swimago-stich` reposundaki frontend uygulamalarini disaridan bakan birinin hizli ve dogru sekilde anlayabilmesi icin hazirlanmistir. Odak noktasi:

- Mevcut frontend mimarisi
- Uygulama bazli ozellik ve akislar
- Hangi API'nin ne amacla kullanildigi
- Backend analizi ile birlestirme icin gerekli baglam

Analiz tarihi: **2026-02-17**

---

## 1) Monorepo ve Teknik Cerceve

### 1.1 Uygulamalar

Bu repo Nx tabanli bir Angular monorepo'dur ve 3 uygulama barindirir:

1. `apps/customer` (proje adi: `swimago`)
2. `apps/host-panel`
3. `apps/admin-panel`

### 1.2 Teknoloji stack'i

- Angular `21`
- Nx `22`
- TypeScript `5.9`
- RxJS `7.8`
- TailwindCSS (ozellikle customer tarafinda)
- Transloco (customer + host panel)
- Leaflet (customer explore harita)
- Angular SSR + Express (sadece customer)

### 1.3 Calistirma komutlari

Root `package.json`:

- `npm run start:customer` -> `http://localhost:4200`
- `npm run start:host` -> `http://localhost:4201`
- `npm run start:admin` -> `http://localhost:4202`
- `npm run start:all` -> uc uygulamayi paralel ayaga kaldirir

### 1.4 Build/serve karakteristi

- `customer`: SSR destekli (`apps/customer/server.ts`)
- `host-panel`: SPA
- `admin-panel`: SPA

---

## 2) Ortak Mimari Kaliplari

### 2.1 Kimlik dogrulama ve rol bazli erisim

Her uygulama kendi auth guard + token service yapisina sahip:

- Customer: sadece `customer` rolunu kabul eder.
- Host panel: `host` veya `admin` rolunu kabul eder.
- Admin panel: sadece `admin` rolunu kabul eder.

### 2.2 HTTP interceptor yapisi

- Customer:
  - `auth.interceptor`: token ekler.
  - `token-refresh.interceptor`: `401` durumunda refresh akisi dener.
  - `error.interceptor`: backend hata payload'larini normalize eder.
- Host/Admin:
  - auth interceptor + error interceptor var.
  - refresh endpoint config'te olmasina ragmen otomatik refresh akisi customer kadar tam degil.

### 2.3 API base URL davranisi

- Customer: sabit `http://localhost:5088/api`
- Host/Admin: runtime override destekler:
  - `globalThis.__SWIMAGO_API_BASE_URL__`
  - default yine `http://localhost:5088/api`

### 2.4 Dil destegi

- Customer: `en`, `tr`, `de`, `ru`
- Host: `tr`, `en`
- Admin: Transloco kullanmiyor (UI metinleri agirlikla hardcoded)

---

## 3) Customer Uygulamasi (`apps/customer`)

## 3.1 Amac

Son kullanicinin (customer) destinasyon kesfetmesi, detay goruntulemesi, rezervasyon/favori/profil islemleri yapmasi.

### 3.2 Route haritasi (ana)

- `/` landing
- `/destinations` destinasyon listesi
- `/explore` map/list kesif
- `/boat-tours` tekne turu listesi
- `/boat-tours/yacht/:slug` yacht detay
- `/boat-tours/day-trip/:slug` day-trip detay
- `/blog` blog listesi
- `/blog/:slug` blog detay
- `/beaches/:slug` beach destination detail
- `/pools/:slug` pool destination detail
- `/beach/:slug` spot detail (beach)
- `/pool/:slug` spot detail (pool)
- `/login`, `/signup`, `/forgot-password`, `/reset-password`
- `/become-host`, `/become-host/create-listing`
- `/profile`, `/profile/reservations`, `/profile/favorites`, `/profile/payment`, `/profile/account-settings`

Not: `/profile/*` route'lari `customerRoleGuard` korumali.

### 3.3 Temel kullanici akislari

1. **Kesif**
   - Landing'de one cikan destinasyonlar + spotlar
   - Explore'da type bazli (`Beach`/`Pool`) arama + harita markerlari
2. **Detay**
   - Destination detail -> alt spot kartlari
   - Spot detail (`/beach/:slug` / `/pool/:slug`) -> `getSpotDetail` + `getQuote`
3. **Boat tours**
   - Liste + filtre
   - Yacht/day-trip detay sayfalari
4. **Blog**
   - Listeleme, kategori/search, pagination
   - Blog detayinda related + comments + yorum gonderme
   - Newsletter subscribe
5. **Auth**
   - register/login/logout
   - forgot/reset password
6. **Profil**
   - dashboard
   - rezervasyonlar (cancel/payment-intent/review)
   - favoriler
   - odeme yontemleri
   - hesap ayarlari (profil/settings/email/password/account)
7. **Become Host listing olusturma**
   - 3 adim wizard
   - create listing -> photo upload -> publish

### 3.4 Customer API sozlugu

Base: `http://localhost:5088/api`

#### Auth

| Method | Endpoint | Amac | Kullanildigi yer |
|---|---|---|---|
| POST | `/Auth/register` | Customer kaydi | `AuthApiService.register` |
| POST | `/Auth/login/customer` | Customer login | `AuthApiService.login` |
| POST | `/Auth/logout` | Oturumu kapatma | `AuthApiService.logout` |
| POST | `/Auth/refresh` | Access token yenileme | `token-refresh.interceptor` |
| POST | `/Auth/forgot-password` | Sifre sifirlama maili | `AuthApiService.forgotPassword` |
| POST | `/Auth/reset-password` | Yeni sifre belirleme | `AuthApiService.resetPassword` |

#### Listing/Search/Explore

| Method | Endpoint | Amac | Kullanildigi yer |
|---|---|---|---|
| GET | `/search/listings` | Spot/listing arama | `SearchService`, `ListingService`, landing/explore |
| GET | `/search/suggestions` | Arama onerileri | `SearchService.getSuggestions` |
| POST | `/listings` | Yeni listing olusturma | become-host create listing |
| POST | `/listings/photos/upload` | Listing fotograf yukleme | become-host create listing |
| POST | `/listings/{id}/publish` | Listing publish | become-host create listing |
| GET | `/explore` | Bounds bazli marker verisi | `ExploreService` (tanimli, pratikte kullanimi sinirli) |

#### Destinations/Spots/Boat Tours

| Method | Endpoint | Amac | Kullanildigi yer |
|---|---|---|---|
| GET | `/destinations` | Destination listeleme | landing + destinations page |
| GET | `/destinations/{slug}` | Destination basic detay | `DestinationService.getDestination` |
| GET | `/destinations/{slug}/detail` | Destination detay + spots | beach/pool destination detail |
| GET | `/spots/{slug}` | Spot detay | beach-detail, pool-detail |
| POST | `/spots/{slug}/quote` | Fiyat kiranimi/teklif | beach-detail, pool-detail |
| GET | `/boat-tours` | Boat tours liste | boat-tours list |
| GET | `/boat-tours/yacht/{slug}` | Yacht tour detay | yacht-tour-detail |
| GET | `/boat-tours/day-trip/{slug}` | Day-trip detay | day-trip-detail |

#### Blog/Newsletter

| Method | Endpoint | Amac | Kullanildigi yer |
|---|---|---|---|
| GET | `/blog` | Blog liste | blog page |
| GET | `/blog/{slug}` | Tekil blog | `BlogService.getBlogPost` |
| GET | `/blog/{slug}/detail` | Blog detay | blog-detail |
| GET | `/blog/{slug}/related` | Ilgili yazilar | blog-detail |
| GET | `/blog/{slug}/comments` | Yorumlar | blog-detail |
| POST | `/blog/{slug}/comments` | Yorum gonderme | blog-detail |
| POST | `/newsletter/subscribe` | Bulten uyeligi | blog page |

#### Profil/rezervasyon/favori/odeme

| Method | Endpoint | Amac | Kullanildigi yer |
|---|---|---|---|
| GET | `/reservations` | Rezervasyon liste | my-reservations |
| GET | `/reservations/{id}` | Rezervasyon detay | `ReservationService.getReservation` |
| POST | `/reservations` | Rezervasyon olustur | `ReservationService.createReservation` |
| PUT | `/reservations/{id}` | Rezervasyon guncelle | `ReservationService.updateReservation` |
| POST | `/reservations/{id}/cancel` | Rezervasyon iptal | my-reservations |
| POST | `/reservations/{id}/check-in` | Check-in | `ReservationService.checkIn` |
| POST | `/reservations/{id}/review` | Yorum/rating | my-reservations |
| POST | `/reservations/{id}/payment-intent` | Odeme intent | my-reservations |
| GET | `/favorites` | Favoriler | my-favorites |
| POST | `/favorites` | Favori ekle | `FavoriteService.addFavorite` |
| DELETE | `/favorites/{venueId}` | Favori sil | my-favorites |
| GET | `/users/me` | Profil getir | account-settings |
| PUT | `/users/me` | Profil guncelle | account-settings |
| GET | `/users/me/dashboard` | Profil dashboard verisi | user-profile |
| POST | `/users/me/avatar` | Avatar guncelle | account-settings |
| PUT | `/users/me/settings` | Kullanici ayarlari | account-settings |
| POST | `/users/me/change-email` | Email degistir | account-settings |
| POST | `/users/me/password` | Sifre degistir | `UserService.changePassword` |
| DELETE | `/users/me` | Hesap sil | account-settings |
| GET | `/payment-methods` | Odeme yontemi liste | payment-methods |
| POST | `/payment-methods` | Odeme yontemi ekle | payment-methods |
| PATCH | `/payment-methods/{id}` | Odeme yontemi update | payment-methods |
| PATCH | `/payment-methods/{id}/default` | Varsayilan sec | payment-methods |
| DELETE | `/payment-methods/{id}` | Odeme yontemi sil | payment-methods |

### 3.5 Customer ozel notlar

- SSR aktif: SEO ve ilk yukleme icin avantajli.
- Header/footer, auth sayfalarinda gizleniyor.
- Coklu dil altyapisi iyi seviyede.
- Bazi aksiyonlar halen placeholder/TODO:
  - favori toggle bazi ekranlarda sadece UI tarafinda
  - bazi butonlarda `console.log`
  - hero search su an API trigger etmiyor

---

## 4) Host Panel (`apps/host-panel`)

### 4.1 Amac

Host/isletme tarafinin operasyon paneli: listing, rezervasyon, takvim, analiz, isletme ayarlari.

### 4.2 Route haritasi

- `/login`
- `/dashboard`
- `/listings`
- `/listings/new`
- `/listings/:id/edit`
- `/reservations`
- `/quick-reservation`
- `/calendar`
- `/analytics`
- `/business-settings`

Tum child route'lar `hostAuthGuard` ile korunur.

### 4.3 Temel akislar

1. **Dashboard**
   - ozet metrikler + son rezervasyonlar + insight + compact analytics
2. **Listings**
   - listeleme, filtreleme, status toggle
   - create/edit listing formu
3. **Reservations**
   - filtre + status guncelleme
4. **Quick reservation**
   - telefon/walk-in manual rezervasyon olusturma
5. **Calendar**
   - gun bazli availability + custom price update
6. **Analytics**
   - period/listing filtreli gelir ve performans
7. **Business settings**
   - operasyonel ayarlar

### 4.4 Host API sozlugu

Auth base: `${apiRoot}`  
Host domain base: `${apiRoot}/host`

#### Auth

| Method | Endpoint | Amac |
|---|---|---|
| POST | `/Auth/login/host` | Host login |
| POST | `/Auth/logout` | Logout |
| POST | `/Auth/refresh` | Refresh endpoint (config var, aktif refresh akisi sinirli) |

#### Host domain

| Method | Endpoint | Amac |
|---|---|---|
| GET | `/host/listings` | Listing listesi |
| GET | `/host/listings/{id}` | Tek listing detayi |
| POST | `/host/listings` | Listing olustur |
| PUT | `/host/listings/{id}` | Listing guncelle |
| PATCH | `/host/listings/{id}/status` | Listing status degisimi |
| GET | `/host/dashboard/stats` | Dashboard metrikleri |
| GET | `/host/reservations/recent` | Son rezervasyonlar |
| GET | `/host/reservations` | Rezervasyon listesi |
| PATCH | `/host/reservations/{id}/status` | Rezervasyon status update |
| POST | `/host/reservations/manual` | Manual rezervasyon |
| GET | `/host/insights` | Insight card verisi |
| GET | `/host/calendar` | Takvim verisi |
| PUT | `/host/calendar` | Takvim update |
| GET | `/host/analytics` | Analytics |
| GET | `/host/business-settings` | Isletme ayarlari getir |
| PUT | `/host/business-settings` | Isletme ayarlari guncelle |

### 4.5 Host ozel notlar

- Dil secimi `tr/en`.
- `HostTokenService`, admin token key'lerini fallback olarak okuyabiliyor; panel gecisinde pratik ama dikkat edilmesi gereken bir coupling.
- Runtime API base override destekli (`__SWIMAGO_API_BASE_URL__`).

---

## 5) Admin Panel (`apps/admin-panel`)

### 5.1 Amac

Platform icerik ve master data yonetimi (destination, beach, pool, blog) + admin auth.

### 5.2 Route haritasi

- `/login`
- `/destinations`, `/destinations/create`, `/destinations/:id/edit`
- `/beaches`, `/beaches/create`, `/beaches/:id/edit`
- `/pools`, `/pools/create`, `/pools/:id/edit`
- `/boat-tours/yacht`, `/boat-tours/yacht/create`, `/boat-tours/yacht/:id/edit`
- `/boat-tours/day-trip`, `/boat-tours/day-trip/create`, `/boat-tours/day-trip/:id/edit`
- `/blogs`, `/blogs/create`, `/blogs/:id/edit`

Child route'lar `adminAuthGuard` ile korunur.

### 5.3 Gercekten API'ye bagli moduller

- Destinations CRUD
- Beaches CRUD
- Pools CRUD
- Blogs CRUD
- Auth login/logout

### 5.4 Kismi/mock kalan moduller

- Boat tours (yacht/day-trip) formlari:
  - `AdminService.submitForm` ile console'a yaziyor.
  - Kalici API call yok.
- Admin tarafinda servis olarak tanimli ama route/page baglantisi olmayan alanlar:
  - users
  - dashboard
  - host applications
  - listing moderation
  - reports
  - master data (cities/amenities)
  - media upload

### 5.5 Admin API sozlugu

Auth base: `${apiRoot}`  
Admin domain base: `${apiRoot}/admin`

#### Auth

| Method | Endpoint | Amac |
|---|---|---|
| POST | `/auth/login/admin` | Admin login |
| POST | `/auth/logout` | Logout |
| POST | `/auth/refresh` | Refresh (service var, otomatik akista kullanimi sinirli) |

#### Aktif kullanılan admin domain endpointleri

| Method | Endpoint | Amac |
|---|---|---|
| GET | `/admin/destinations` | Liste |
| GET | `/admin/destinations/{id}` | Detay |
| POST | `/admin/destinations` | Olustur |
| PUT | `/admin/destinations/{id}` | Guncelle |
| DELETE | `/admin/destinations/{id}` | Sil |
| GET | `/admin/beaches` | Liste |
| GET | `/admin/beaches/{id}` | Detay |
| POST | `/admin/beaches` | Olustur |
| PUT | `/admin/beaches/{id}` | Guncelle |
| DELETE | `/admin/beaches/{id}` | Sil |
| GET | `/admin/pools` | Liste |
| GET | `/admin/pools/{id}` | Detay |
| POST | `/admin/pools` | Olustur |
| PUT | `/admin/pools/{id}` | Guncelle |
| DELETE | `/admin/pools/{id}` | Sil |
| GET | `/admin/blogs` | Liste |
| GET | `/admin/blogs/{id}` | Detay |
| POST | `/admin/blogs` | Olustur |
| PUT | `/admin/blogs/{id}` | Guncelle |
| DELETE | `/admin/blogs/{id}` | Sil |

#### Servislerde tanimli ama UI baglantisi zayif endpointler

| Endpoint grubu | Durum |
|---|---|
| `/admin/users*` | Servis var, aktif sayfa yok |
| `/admin/dashboard` | Servis var, aktif sayfa yok |
| `/admin/host-applications*` | Servis var, aktif sayfa yok |
| `/admin/listings*` moderasyon | Servis var, aktif sayfa yok |
| `/admin/reports` | Servis var, aktif sayfa yok |
| `/admin/cities`, `/admin/amenities` | Servis var, aktif sayfa yok |
| `/admin/media/*` | Servis var, aktif sayfa yok |

---

## 6) Frontendler Arasi Karsilastirma (Hizli)

| Alan | Customer | Host | Admin |
|---|---|---|---|
| Hedef rol | customer | host/admin | admin |
| Port | 4200 | 4201 | 4202 |
| SSR | Evet | Hayir | Hayir |
| i18n | en/tr/de/ru | tr/en | Yok (su an) |
| API base override | Hayir (hardcoded) | Evet | Evet |
| Token refresh | Var | Endpoint var, akis kisitli | Endpoint var, akis kisitli |
| Uygulama olgunlugu | En yuksek | Yuksek | Orta (bazi moduller mock) |

---

## 7) Tespit Edilen Teknik Bosluklar / Riskler

1. **Dokuman-kod base URL farki**
   - Eski dokumanlarda `localhost:5000` geciyor.
   - Kod tarafinda aktif default `localhost:5088/api`.

2. **Customer profil menusu route tutarsizligi**
   - Bazi componentlerde `/profile/settings` linki var.
   - Route tanimi `/profile/account-settings`.

3. **Kismi placeholder aksiyonlar**
   - Customer tarafinda bazi CTA'ler `console.log`/TODO.
   - Admin boat tours formlari API yerine `AdminService.submitForm` kullanir.

4. **Tanimli fakat kullanilmayan servisler**
   - Customer `ExploreService` pratikte route/component tarafinda belirgin kullanilmiyor.
   - Admin'in bircok servis modulu henuz route/page'e bagli degil.

5. **Refresh token akislarinin tutarliligi**
   - Customer refresh interceptor daha oturmus.
   - Host/Admin tarafinda refresh endpoint tanimli olsa da otomatik mekanizma sinirli.

---

## 8) Backend Analiziyle Birlestirme Icin Hazir Esleme

Bu tablo backend analizi ile merge ederken dogrudan kullanilabilir:

| Domain | Frontend kaynaklari | Endpoint prefix | Beklenen backend module |
|---|---|---|---|
| Auth | customer/host/admin auth services | `/Auth/*` veya `/auth/*` | Auth controller + token refresh |
| Destination | customer + admin destination services | `/destinations`, `/admin/destinations` | Destination query + admin CRUD |
| Listing/Search | customer listing/search + host listing | `/search/listings`, `/listings`, `/host/listings` | Listing read/write + search index |
| Spot/Quote | customer spot service | `/spots/*` | Spot detail + pricing quote engine |
| Reservation | customer reservation + host reservation | `/reservations*`, `/host/reservations*` | Reservation lifecycle + payment intent |
| Favorites | customer favorite service | `/favorites*` | Favorite aggregate |
| User profile | customer user service | `/users/me*` | User profile/settings module |
| Payment methods | customer payment service | `/payment-methods*` | Payment vault/provider adapter |
| Blog | customer + admin blog services | `/blog*`, `/admin/blogs*` | CMS + public blog |
| Admin moderation/reporting | admin services | `/admin/users*`, `/admin/reports*`, ... | Admin ops/moderation/reporting |

### Merge sirasinda onerilen kontrol listesi

1. Endpoint casing standardizasyonu (`/Auth` vs `/auth`)
2. Ortak hata modeli standardizasyonu (`error`, `message`, validation payload)
3. Pagination response standardizasyonu (items/total/page/pageSize)
4. Role claim standardizasyonu (`role`, `roles`, claim URI)
5. Frontend dokumanlarini tek base URL gercegine getirme

---

## 9) Dogrudan Kaynak Dosya Referanslari

Ana referans dosyalar:

- `apps/customer/src/app/app.routes.ts`
- `apps/customer/src/app/core/config/api.config.ts`
- `apps/customer/src/app/core/services/*.ts`
- `apps/host-panel/src/app/app.routes.ts`
- `apps/host-panel/src/app/core/config/host-api.config.ts`
- `apps/host-panel/src/app/core/services/host-data.service.ts`
- `apps/admin-panel/src/app/app.routes.ts`
- `apps/admin-panel/src/app/core/config/admin-api.config.ts`
- `apps/admin-panel/src/app/core/services/*.ts`
- `package.json`
- `apps/customer/project.json`
- `apps/host-panel/project.json`
- `apps/admin-panel/project.json`

---

Bu dokuman, backend analizi ile birlestirildiginde Swimago'nun uc panelinin (customer-host-admin) uc uca is akislarini ve API sozlesmelerini tek yerde gormenize yardimci olacak sekilde hazirlanmistir.
