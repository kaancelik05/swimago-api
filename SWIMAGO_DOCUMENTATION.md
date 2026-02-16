# Swimago API Dokümantasyonu ve Sistem İşleyişi

Bu belge, Swimago sisteminin nasıl çalıştığını ve API uç noktalarının (endpoints) işlevlerini açıklamaktadır.

---

## 🏗 Sistem İşleyişi (Functional Logic)

Swimago, kullanıcıların plaj, havuz ve tekne turları gibi deniz/su aktiviteleri için yer arayabildiği ve rezervasyon yapabildiği bir platformdur. Sistem üç ana aktör etrafında döner: **Misafir (Guest/User)**, **Ev Sahibi (Host)** ve **Yönetici (Admin)**.

### 1. Keşif ve Arama (Discovery & Search)
- **Coğrafi Arama:** Sistem, PostGIS kullanarak kullanıcının konumuna veya belirli koordinatlara göre "yakınındaki" yerleri bulmasını sağlar.
- **Harita Üzerinden Keşif:** `Explore` modülü ile harita sınırları (bounds) içindeki tüm ilanlar fiyat ve tür detaylarıyla listelenir.
- **Gelişmiş Filtreleme:** Kullanıcılar; aktivite türü (Plaj, Havuz, Yat, Günlük Tur), fiyat aralığı, şehir ve sunulan imkanlara (Amenity) göre arama yapabilir.

### 2. Rezervasyon Akışı (Booking Flow)
- **Müsaitlik Kontrolü:** Kullanıcı bir yeri seçtiğinde, `check-availability` üzerinden seçili tarihlerde yerin müsait olup olmadığını kontrol eder.
- **Rezervasyon Oluşturma:** Müsait olan yerler için rezervasyon kaydı oluşturulur. Rezervasyonlar ilk aşamada "Pending" (Beklemede) durumundadır.
- **Durum Yönetimi:** Ev sahibi rezervasyonu onayladığında (Confirmed), süreç başlar. Aktivite bittikten sonra "Completed" (Tamamlandı) durumuna geçer.

### 3. Rol Bazlı Yetkiler
- **Misafir (User):** İlanları arar, favorilerine ekler, rezervasyon yapar ve aktivite sonrası yorum/puan bırakır.
- **Ev Sahibi (Host):** Kendi mekanlarını sisteme ekler. Takvim üzerinden günlük fiyatlandırma yapabilir, belirli günleri kapatabilir (Availability Block) ve gelen rezervasyonları yönetir.
- **Yönetici (Admin):** Platformun genel güvenliğini ve kalitesini sağlar. Yeni ilanları onaylar/reddeder, kullanıcıların rollerini yönetir, platformdaki şehir ve imkan (category/amenity) listelerini günceller.

### 4. Çoklu Dil Desteği
- İlan başlıkları, açıklamalar ve diğer dinamik metinler veritabanında `JSONB` formatında tutularak çoklu dil desteği (Türkçe, İngilizce vb.) sağlanır.

---

## 🚀 API Endpoint Listesi

### 1. Kimlik Doğrulama (Auth) - `/api/Auth`
| Metot | Endpoint | İşlev |
| :--- | :--- | :--- |
| POST | `/register` | Yeni kullanıcı kaydı oluşturur. |
| POST | `/login` | E-posta ve şifre ile giriş yapar, JWT token döner. |
| POST | `/logout` | Mevcut oturumu kapatır. |
| POST | `/refresh` | Refresh token kullanarak erişim token'ını yeniler. |
| POST | `/forgot-password` | Şifre sıfırlama talebi oluşturur. |
| POST | `/reset-password` | Token ile şifreyi sıfırlar. |

### 2. İlan Yönetimi (Listings) - `/api/Listings`
| Metot | Endpoint | İşlev |
| :--- | :--- | :--- |
| GET | `/` | Tüm aktif ilanları sayfalı olarak listeler. |
| GET | `/{id}` | Belirli bir ilanın tüm detaylarını getirir. |
| GET | `/type/{type}` | İlan türüne göre (Plaj, Havuz, Yat vb.) filtreler. |
| GET | `/nearby` | Belirli koordinat ve yarıçap içinde ilan araması yapar. |
| POST | `/` | Yeni ilan oluşturur (Sadece Host/Admin). |
| POST | `/photos/upload` | İlan fotoğraflarını yükler. |

### 3. Rezervasyonlar (Reservations) - `/api/Reservations`
| Metot | Endpoint | İşlev |
| :--- | :--- | :--- |
| GET | `/` | Giriş yapmış kullanıcının kendi rezervasyonlarını listeler. |
| POST | `/` | Yeni bir rezervasyon oluşturur. |
| GET | `/{id}` | Rezervasyon detaylarını getirir. |
| POST | `/{id}/cancel` | Rezervasyonu iptal eder. |
| POST | `/{id}/check-in` | Etkinliğe giriş (check-in) işlemini yapar. |
| GET | `/check-availability` | Belirli tarihler için müsaitlik durumunu sorgular (Genel erişim). |

### 4. Arama ve Keşif (Search & Explore)
| Metot | Endpoint | İşlev |
| :--- | :--- | :--- |
| POST | `/api/Search/listings` | Gelişmiş kriterlerle ilan araması yapar. |
| GET | `/api/Search/suggestions` | Arama barı için otomatik tamamlama önerileri getirir. |
| GET | `/api/Explore` | Harita sınırlarına göre marker ve bölge verilerini getirir. |

### 5. Mekan ve Tekne Detayları (Spots & Boat Tours)
| Metot | Endpoint | İşlev |
| :--- | :--- | :--- |
| GET | `/api/Spots/{slug}` | Belirli bir plaj veya havuzun detaylarını (SEO dostu slug ile) getirir. |
| GET | `/api/boat-tours` | Tüm tekne ve yat turlarını filtreleme seçenekleriyle listeler. |
| GET | `/api/boat-tours/yacht/{slug}` | Belirli bir yat turunun detaylarını getirir. |
| GET | `/api/boat-tours/day-trip/{slug}` | Belirli bir günlük tur detaylarını getirir. |

### 6. Blog ve İçerik (Blog) - `/api/Blog`
| Metot | Endpoint | İşlev |
| :--- | :--- | :--- |
| GET | `/` | Yayınlanmış blog yazılarını sayfalı olarak listeler. |
| GET | `/{slug}` | Blog yazısının detaylarını getirir. |
| POST | `/` | Yeni blog yazısı oluşturur (Sadece Admin). |
| PUT | `/{id}` | Blog yazısını günceller (Sadece Admin). |
| DELETE | `/{id}` | Blog yazısını siler (Sadece Admin). |

### 7. Kullanıcı Profili (Users) - `/api/Users`
| Metot | Endpoint | İşlev |
| :--- | :--- | :--- |
| GET | `/me` | Mevcut kullanıcının profil bilgilerini getirir. |
| PUT | `/me` | Profil bilgilerini günceller. |
| PUT | `/me/avatar` | Kullanıcı fotoğrafını günceller. |
| PUT | `/me/settings` | Kullanıcı ayarlarını (bildirimler vb.) günceller. |
| DELETE | `/me` | Kullanıcı hesabını siler. |

### 6. Favoriler (Favorites) - `/api/Favorites`
| Metot | Endpoint | İşlev |
| :--- | :--- | :--- |
| GET | `/` | Kullanıcının favori listesini getirir. |
| POST | `/` | Bir mekanı favorilere ekler. |
| DELETE | `/{venueId}` | Mekanı favorilerden çıkarır. |

### 7. Değerlendirmeler (Reviews) - `/api/Reviews`
| Metot | Endpoint | İşlev |
| :--- | :--- | :--- |
| POST | `/` | Tamamlanmış bir rezervasyon için yorum ve puan bırakır. |
| GET | `/listing/{listingId}` | Bir ilana yapılmış tüm yorumları listeler. |
| POST | `/{id}/host-response` | Ev sahibinin yoruma cevap vermesini sağlar. |

### 8. Ev Sahibi Paneli (Host) - `/api/host`
| Metot | Endpoint | İşlev |
| :--- | :--- | :--- |
| GET | `/dashboard` | İstatistikler, bekleyen rezervasyonlar vb. özet verileri getirir. |
| GET | `/listings` | Ev sahibinin kendi ilanlarını listeler. |
| PUT | `/listings/{id}/pricing` | İlanın fiyatlandırma ayarlarını günceller. |
| GET | `/calendar` | İlanın takvim görünümünü (fiyat/müsaitlik) getirir. |
| GET | `/analytics` | Kazanç ve ziyaretçi analitiği sunar. |

### 9. Yönetici Paneli (Admin) - `/api/admin`
| Metot | Endpoint | İşlev |
| :--- | :--- | :--- |
| GET | `/users` | Platformdaki tüm kullanıcıları yönetir. |
| GET | `/host-applications` | Ev sahibi olma başvurularını listeler. |
| POST | `/listings/{id}/approve` | Yayınlanması bekleyen ilanları onaylar. |
| POST | `/cities` | Platformdaki şehir listesini yönetir. |
| POST | `/amenities` | İlanlarda sunulan özellik/imkan listesini yönetir. |

### 9. Ödeme Yöntemleri (Payment Methods) - `/api/payment-methods`
| Metot | Endpoint | İşlev |
| :--- | :--- | :--- |
| GET | `/` | Kullanıcının kayıtlı ödeme yöntemlerini listeler. |
| POST | `/` | Yeni bir ödeme yöntemi (kart vb.) ekler. |
| DELETE | `/{id}` | Ödeme yöntemini siler. |
| PUT | `/{id}/default` | Belirli bir ödeme yöntemini varsayılan yapar. |

### 10. Diğer Servisler
| Metot | Endpoint | İşlev |
| :--- | :--- | :--- |
| GET | `/api/Health` | Sistemin sağlık durumunu kontrol eder. |
| GET | `/api/Health/db` | Veritabanı bağlantı sağlık durumunu kontrol eder. |
| POST | `/api/Newsletter/subscribe` | E-bültene kayıt yapar. |
| POST | `/api/Newsletter/unsubscribe` | E-bülten kaydından çıkar. |
| GET | `/api/Destinations` | Şehirleri ve buralardaki mekan özetlerini listeler. |
| GET | `/api/Destinations/{slug}` | Belirli bir destinasyonun detaylarını ve içindeki mekanları getirir. |

---

> [!NOTE]
> Tüm `/api` ile başlayan ancak `/Auth` dışında kalan çoğu endpoint, Header'da geçerli bir **Bearer JWT Token** beklemektedir. Rol gerektiren işlemler (Admin/Host) için ilgili rolün token içinde tanımlı olması şarttır.
