# Frontend Geçiş Dokümanı: Role Bazlı Login ve RBAC

Tarih: 2026-02-17  
Proje: Swimago API

## Amaç

Backend, kullanıcı tipine göre panel erişimini kesin olarak ayıracak şekilde güncellendi:

- `Customer` kullanıcılar yalnızca customer işlemlerini yapabilir.
- `Host` kullanıcılar yalnızca host panel işlemlerini yapabilir.
- `Admin` kullanıcılar yalnızca admin panel işlemlerini yapabilir.

Bu doküman, frontend tarafında yapılması gereken değişiklikleri açıklar.

## Backend'te Yapılan Değişiklikler

1. Policy-based RBAC eklendi (`CustomerOnly`, `HostOnly`, `AdminOnly`).
2. Role bazlı login endpoint’leri eklendi:
   - `POST /api/auth/login/customer`
   - `POST /api/auth/login/host`
   - `POST /api/auth/login/admin`
3. Login sırasında role doğrulaması zorunlu hale getirildi (yanlış panelden login denemesi `401` döner).
4. Public register akışında role yükseltme kapatıldı:
   - `register` artık `Role` parametresi kabul etmez.
   - Yeni kayıt olan kullanıcı backend tarafında otomatik `Customer` olur.
5. Endpoint yetkileri role bazında ayrıldı (paneller arası erişim engellendi).

## Frontend Tarafında Zorunlu Değişiklikler

1. Login endpoint’lerini panele göre ayır

- Customer app login: `POST /api/auth/login/customer`
- Host panel login: `POST /api/auth/login/host`
- Admin panel login: `POST /api/auth/login/admin`

Not: Genel login endpoint’i (`POST /api/auth/login`) teknik olarak duruyor olabilir; panel bazlı güvenli kullanım için yeni role-specific endpoint’ler kullanılmalı.

2. Register payload’ından `role` alanını kaldır

### Eski (artık göndermeyin)
```json
{
  "email": "user@example.com",
  "password": "Secret123!",
  "firstName": "Ada",
  "lastName": "Yilmaz",
  "phoneNumber": "+905551112233",
  "role": "Host"
}
```

### Yeni
```json
{
  "email": "user@example.com",
  "password": "Secret123!",
  "firstName": "Ada",
  "lastName": "Yilmaz",
  "phoneNumber": "+905551112233"
}
```

3. 401 ve 403 handling’i net ayrıştır

- `401 Unauthorized`:
  - Geçersiz email/şifre
  - Yanlış panelden login denemesi (örn. customer hesabı ile host login endpoint’i)
- `403 Forbidden`:
  - Login başarılı ama token rolü endpoint policy’sine uymuyor
  - Örn. customer token ile `/api/host/*` veya `/api/admin/*`

UI tarafında ayrı mesajlar gösterin:
- `401`: “Giriş bilgileri veya kullanıcı tipi hatalı.”
- `403`: “Bu alana erişim yetkiniz yok.”

4. Login sonrası panel yönlendirme kuralını role ile doğrula

`AuthResponse.role` değerini kontrol ederek:
- `Customer` ise customer uygulama rotalarına
- `Host` ise host panel rotalarına
- `Admin` ise admin panel rotalarına yönlendir

Eğer beklenen panel ile role uyuşmuyorsa token’ı saklamadan çıkış/uyarı ver.

5. API katmanında endpoint map’i güncellensin

- Customer-only endpointler:
  - `/api/favorites`
  - `/api/payment-methods`
  - `/api/reservations`
  - `POST /api/reviews`
  - `DELETE /api/reviews/{id}`
- Host-only endpointler:
  - `/api/host/*`
  - `POST /api/listings`
  - `POST /api/listings/photos/upload`
  - `POST /api/reviews/{id}/host-response`
- Admin-only endpointler:
  - `/api/admin/*`
  - `POST/PUT/DELETE /api/blog`

## Önerilen Frontend İyileştirmeleri (Best Practice)

1. Her panel için ayrı auth client/facade kullan (`customerAuthApi`, `hostAuthApi`, `adminAuthApi`).
2. Token decode edip `role` claim kontrolünü route guard seviyesinde uygula.
3. Merkezi bir `http interceptor` ile:
   - `401` => login ekranına yönlendir
   - `403` => unauthorized sayfasına yönlendir
4. Frontend’de “panel tipi” state’i tut ve login endpoint seçimini buna göre otomatik yap.
5. E2E test ekle:
   - Customer ile host login endpoint’i => `401`
   - Customer token ile host endpoint => `403`
   - Host token ile customer-only endpoint => `403`

## Hızlı Kontrol Listesi

- [ ] Login formları doğru endpoint’e gidiyor.
- [ ] Register request’te `role` gönderilmiyor.
- [ ] `401` ve `403` için farklı kullanıcı mesajları var.
- [ ] Route guard role doğrulaması yapıyor.
- [ ] Panel dışı endpoint çağrıları UI’da engelleniyor.
- [ ] QA senaryoları role bazlı geçiyor.

