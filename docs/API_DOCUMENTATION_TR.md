# Swimago API Dokümantasyonu

Bu belge, Swimago API'sindeki tüm endpointlerin işlevlerini ve kullanım detaylarını içerir.

## Kimlik Doğrulama (Authentication)

Çoğu endpoint, `Authorization` başlığında bir Bearer token gerektirir:
`Authorization: Bearer <token>`

---

## 1. Kimlik Doğrulama (Auth)
**Base URL:** `/api/auth`

| Metot | Endpoint | Açıklama |
|-------|----------|----------|
| `POST` | `/register` | Yeni kullanıcı hesabı oluşturur. |
| `POST` | `/login` | E-posta ve şifre ile giriş yapar, token döndürür. |
| `POST` | `/logout` | Mevcut kullanıcının oturumunu kapatır (Token'ı geçersiz kılar). |
| `POST` | `/refresh` | Refresh token kullanarak yeni bir erişim token'ı (access token) alır. |
| `POST` | `/forgot-password` | Şifre sıfırlama e-postası gönderir. |
| `POST` | `/reset-password` | Token ile şifreyi sıfırlar. |

## 2. Kullanıcılar (Users)
**Base URL:** `/api/users`

| Metot | Endpoint | Açıklama |
|-------|----------|----------|
| `GET` | `/me` | Giriş yapmış kullanıcının profil bilgilerini getirir. |
| `PUT` | `/me` | Kullanıcı profil bilgilerini günceller. |
| `PUT` | `/me/avatar` | Kullanıcı profil fotoğrafını günceller. |
| `PUT` | `/me/settings` | Kullanıcı ayarlarını günceller. |
| `PUT` | `/me/password` | Kullanıcı şifresini değiştirir. |
| `DELETE` | `/me` | Kullanıcı hesabını siler. |

## 3. İlanlar (Listings)
**Base URL:** `/api/listings`

| Metot | Endpoint | Açıklama |
|-------|----------|----------|
| `GET` | `/` | Tüm aktif ilanları sayfalama ile listeler. |
| `GET` | `/{id}` | Belirtilen ID'ye sahip ilanın detaylarını getirir. |
| `GET` | `/type/{type}` | Belirtilen türdeki (Plaj, Havuz, Tekne Turu) ilanları listeler. |
| `GET` | `/nearby` | Coğrafi konuma göre yakındaki ilanları arar (PostGIS). |
| `POST` | `/` | Yeni ilan oluşturur (Sadece Host ve Admin). |
| `POST` | `/photos/upload` | İlan için fotoğraf yükler (Sadece Host ve Admin). |

## 4. Rezervasyonlar (Reservations)
**Base URL:** `/api/reservations`

| Metot | Endpoint | Açıklama |
|-------|----------|----------|
| `GET` | `/` | Giriş yapmış kullanıcının rezervasyonlarını listeler. |
| `POST` | `/` | Yeni bir rezervasyon oluşturur. |
| `GET` | `/{id}` | Rezervasyon detaylarını getirir. |
| `PUT` | `/{id}` | Rezervasyonu günceller. |
| `POST` | `/{id}/cancel` | Rezervasyonu iptal eder. |
| `POST` | `/{id}/check-in` | Rezervasyon için check-in işlemi yapar. |
| `POST` | `/{id}/review` | Tamamlanmış rezervasyon için değerlendirme gönderir. |
| `GET` | `/check-availability` | Bir ilanın belirtilen tarihlerde müsaitliğini kontrol eder (Herkese açık). |

## 5. Mekan Detayları (Spots)
**Base URL:** `/api/spots`

| Metot | Endpoint | Açıklama |
|-------|----------|----------|
| `GET` | `/{slug}` | Slug (URL dostu isim) ile mekan (plaj/havuz) detaylarını getirir. |

## 6. Arama (Search)
**Base URL:** `/api/search`

| Metot | Endpoint | Açıklama |
|-------|----------|----------|
| `POST` | `/listings` | Gelişmiş kriterlerle (filtreler, konum, sıralama) ilan araması yapar. |
| `GET` | `/suggestions` | Arama terimi için otomatik tamamlama önerileri getirir. |

## 7. Keşfet (Explore)
**Base URL:** `/api/explore`

| Metot | Endpoint | Açıklama |
|-------|----------|----------|
| `GET` | `/` | Harita görünümü için belirli sınırlar içindeki mekanları getirir. |

## 8. Tekne Turları (Boat Tours)
**Base URL:** `/api/boat-tours`

| Metot | Endpoint | Açıklama |
|-------|----------|----------|
| `GET` | `/` | Tüm tekne turlarını (yatlar ve günübirlik geziler) listeler. |
| `GET` | `/yacht/{slug}` | Slug ile yat turu detaylarını getirir. |
| `GET` | `/day-trip/{slug}` | Slug ile günübirlik gezi detaylarını getirir. |

## 9. Destinasyonlar (Destinations)
**Base URL:** `/api/destinations`

| Metot | Endpoint | Açıklama |
|-------|----------|----------|
| `GET` | `/` | Tüm destinasyonları (şehirler) listeler. |
| `GET` | `/{slug}` | Slug ile destinasyon detaylarını ve içindeki mekanları getirir. |

## 10. Değerlendirmeler (Reviews)
**Base URL:** `/api/reviews`

| Metot | Endpoint | Açıklama |
|-------|----------|----------|
| `POST` | `/` | Tamamlanmış bir rezervasyon için yorum oluşturur. |
| `GET` | `/{id}` | Yorum detaylarını getirir. |
| `GET` | `/listing/{listingId}` | Bir ilana ait tüm yorumları listeler (Herkese açık). |
| `POST` | `/{id}/host-response` | Ev sahibi yoruma cevap yazar. |
| `DELETE` | `/{id}` | Yorumu siler (Sadece yorum sahibi). |

## 11. Favoriler (Favorites)
**Base URL:** `/api/favorites`

| Metot | Endpoint | Açıklama |
|-------|----------|----------|
| `GET` | `/` | Kullanıcının favori mekanlarını listeler. |
| `POST` | `/` | Bir mekanı favorilere ekler. |
| `DELETE` | `/{venueId}` | Bir mekanı favorilerden çıkarır. |

## 12. Ev Sahibi Paneli (Host)
**Base URL:** `/api/host`

| Metot | Endpoint | Açıklama |
|-------|----------|----------|
| `GET` | `/dashboard` | Ev sahibi paneli istatistiklerini getirir. |
| `GET` | `/listings` | Ev sahibinin kendi ilanlarını listeler. |
| `GET` | `/listings/{id}` | Ev sahibinin ilan detayını getirir. |
| `PUT` | `/listings/{id}` | İlan temel bilgilerini günceller. |
| `PUT` | `/listings/{id}/pricing` | İlan fiyatlandırmasını günceller. |
| `DELETE` | `/listings/{id}` | İlanı pasife alır (siler). |
| `GET` | `/reservations` | Ev sahibinin ilanlarına yapılan rezervasyonları listeler. |
| `PUT` | `/reservations/{id}/status` | Rezervasyon durumunu günceller (Onayla/Reddet). |
| `GET` | `/calendar` | İlan takvimini getirir. |
| `PUT` | `/calendar` | Takvim müsaitliğini/fiyatını günceller. |
| `GET` | `/analytics` | Ev sahibi analiz verilerini getirir. |

## 13. Yönetici Paneli (Admin)
**Base URL:** `/api/admin`

| Metot | Endpoint | Açıklama |
|-------|----------|----------|
| `GET` | `/dashboard` | Yönetici paneli istatistiklerini getirir. |
| `GET` | `/users` | Kullanıcıları listeler (filtreleme ile). |
| `GET` | `/users/{id}` | Kullanıcı detaylarını getirir. |
| `PUT` | `/users/{id}/status` | Kullanıcı durumunu günceller. |
| `PUT` | `/users/{id}/role` | Kullanıcı rolünü günceller. |
| `GET` | `/host-applications` | Ev sahibi başvurularını listeler. |
| `POST` | `/host-applications/{userId}/reject` | Ev sahibi başvurusunu reddeder. |
| `GET` | `/listings` | Tüm ilanları listeler (onay durumu ile). |
| `POST` | `/listings/{id}/approve` | İlanı onaylar. |
| `POST` | `/listings/{id}/reject` | İlanı reddeder. |
| `GET` | `/reports` | Sistem raporlarını/analitiklerini getirir. |
| `GET` | `/cities` | Şehirleri listeler. |
| `POST` | `/cities` | Yeni şehir oluşturur. |
| `GET` | `/amenities` | Özellikleri (imkanlar) listeler. |
| `POST` | `/amenities` | Yeni özellik oluşturur. |

## 14. Blog
**Base URL:** `/api/blog`

| Metot | Endpoint | Açıklama |
|-------|----------|----------|
| `GET` | `/` | Yayınlanmış blog yazılarını listeler (Herkese açık). |
| `GET` | `/{slug}` | Slug ile blog yazısı detayını getirir (Herkese açık). |
| `POST` | `/` | Yeni blog yazısı oluşturur (Sadece Admin). |
| `PUT` | `/{id}` | Blog yazısını günceller (Yazar veya Admin). |
| `DELETE` | `/{id}` | Blog yazısını siler (Yazar veya Admin). |

## 15. Bülten (Newsletter)
**Base URL:** `/api/newsletter`

| Metot | Endpoint | Açıklama |
|-------|----------|----------|
| `POST` | `/subscribe` | Bültene abone olur. |
| `POST` | `/unsubscribe` | Bülten aboneliğinden çıkar. |

## 16. Ödeme Yöntemleri (Payment Methods)
**Base URL:** `/api/payment-methods`

| Metot | Endpoint | Açıklama |
|-------|----------|----------|
| `GET` | `/` | Kullanıcının kayıtlı ödeme yöntemlerini listeler. |
| `POST` | `/` | Yeni ödeme yöntemi ekler. |
| `DELETE` | `/{id}` | Ödeme yöntemini siler. |
| `PUT` | `/{id}/default` | Varsayılan ödeme yöntemini ayarlar. |

## 17. Sağlık Kontrolü (Health)
**Base URL:** `/api/health`

| Metot | Endpoint | Açıklama |
|-------|----------|----------|
| `GET` | `/` | API sağlık durumunu kontrol eder. |
| `GET` | `/db` | Veritabanı bağlantı durumunu kontrol eder. |

---
**Not:** Tüm POST ve PUT isteklerinde gönderilecek JSON formatları ve detaylı hata kodları için lütfen Swagger UI (`/swagger`) arayüzünü inceleyiniz.
