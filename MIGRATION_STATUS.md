# Swimago API - Migration Durum Raporu

## 📊 Mevcut Migration Durumu

### Aktif Migration
**Migration Adı:** `20260203204953_InitialCreate_UUID`  
**Tarih:** 3 Şubat 2026  
**Durum:** ✅ Uygulanmış (Supabase'de aktif)

### Migration Özellikleri

Bu migration, tam bir UUID tabanlı schema oluşturur:

#### Core Entities (UUID Primary Keys)
- ✅ **Users** - Kullanıcı yönetimi (Customer, Host, Admin rolleri)
- ✅ **UserProfiles** - Kullanıcı profil bilgileri
- ✅ **Listings** - İlanlar (Plaj, Havuz, Yat, Günlük Tur)
- ✅ **Reservations** - Rezervasyonlar
- ✅ **Reviews** - Değerlendirmeler
- ✅ **Favorites** - Favori listesi
- ✅ **PaymentMethods** - Ödeme yöntemleri

#### Supporting Entities
- ✅ **Cities** - Şehir bilgileri (multi-language JSONB)
- ✅ **Amenities** - Özellikler ve imkanlar
- ✅ **BlogPosts** - Blog yazıları
- ✅ **NewsletterSubscribers** - Bülten aboneleri
- ✅ **Notifications** - Bildirimler

#### Relationship Tables
- ✅ **ListingAmenities** - İlan-Özellik ilişkisi
- ✅ **ListingImages** - İlan görselleri
- ✅ **AvailabilityBlocks** - Müsaitlik blokları
- ✅ **DailyPricings** - Günlük fiyatlandırma
- ✅ **ReservationPayments** - Ödeme kayıtları

### Önemli Özellikler

#### 1. PostGIS Integration (Coğrafi Sorgular)
```sql
Location geography (point)  -- Listings tablosunda
```
- ✅ GIST index ile optimize edilmiş spatial queries
- ✅ Yakınlık tabanlı arama desteği
- ✅ Harita üzerinden filtreleme

#### 2. Multi-Language Support (JSONB)
```json
{
  "tr": "Plaj",
  "en": "Beach",
  "de": "Strand",
  "ru": "Пляж"
}
```

**JSONB Kullanan Alanlar:**
- Listing: Title, Description, Address, Conditions
- City: Name
- Amenity: Label, ApplicableTo
- BlogPost: Title, Description, Content
- Review: Categories
- User: NotificationSettings, LanguageSettings, PrivacySettings

#### 3. Performance Indexes

**GIN Indexes (JSONB):**
- `IX_Listings_Title` - Başlık araması
- `IX_Listings_Description` - Açıklama araması
- `IX_BlogPosts_Title` - Blog başlık araması

**GIST Indexes (PostGIS):**
- `IX_Listings_Location` - Coğrafi sorgular

**B-Tree Indexes:**
- Status, Type, City, IsFeatured fields
- Foreign keys
- Unique constraints (Slug, Email, ConfirmationNumber)

## 🔧 Migration Komutları

### Migration Listesini Görüntüleme
```bash
dotnet ef migrations list --project src/Swimago.Infrastructure --startup-project src/Swimago.API
```

**Beklenen Çıktı:**
```
20260203204953_InitialCreate_UUID (Applied)
```

### Migration Durumunu Kontrol Etme
```bash
dotnet ef database update --project src/Swimago.Infrastructure --startup-project src/Swimago.API
```

**Başarılı Durum:**
```
Build succeeded.
Done.
```

### Yeni Migration Oluşturma
```bash
dotnet ef migrations add MigrationName --project src/Swimago.Infrastructure --startup-project src/Swimago.API
```

### Migration Geri Alma (Dikkatli!)
```bash
# Son migration'ı geri al
dotnet ef database update PreviousMigrationName --project src/Swimago.Infrastructure --startup-project src/Swimago.API

# Tüm migration'ları geri al (veritabanını sıfırla)
dotnet ef database update 0 --project src/Swimago.Infrastructure --startup-project src/Swimago.API
```

### Migration Script Oluşturma (SQL)
```bash
dotnet ef migrations script --project src/Swimago.Infrastructure --startup-project src/Swimago.API -o migration.sql
```

## 📦 Yedek SQL Dosyaları

Proje kök klasöründe şu SQL yedekleri bulunur:

| Dosya | Açıklama | Kullanım |
|-------|----------|----------|
| `full_migration.sql` | Tam schema (tüm migration'lar birleştirilmiş) | Manuel restore için |
| `golden_uuid.sql` | UUID versiyonu | İlk UUID migration backup |
| `golden_v2.sql` | Güncellenmiş schema | Versiyon 2 backup |
| `golden_v3.sql` | En son schema | En güncel backup |

### Manuel SQL Import (Gerekirse)
```bash
# Supabase SQL Editor'de çalıştırın:
# 1. full_migration.sql içeriğini kopyalayın
# 2. SQL Editor'e yapıştırın
# 3. Run tuşuna basın
```

## 🔍 Supabase'de Doğrulama

### 1. Table Editor ile Kontrol
Supabase Dashboard → **Table Editor** → Şu tabloları görmeli:
- Users
- Listings  
- Reservations
- Reviews
- Favorites
- PaymentMethods
- Cities
- Amenities
- BlogPosts
- NewsletterSubscribers

### 2. Extensions Kontrolü
Supabase Dashboard → **Database** → **Extensions**:
- ✅ `postgis` - Enabled

### 3. SQL Query ile Kontrol
```sql
-- Tabloları listele
SELECT table_name 
FROM information_schema.tables 
WHERE table_schema = 'public' 
ORDER BY table_name;

-- PostGIS extension kontrolü
SELECT * FROM pg_extension WHERE extname = 'postgis';

-- Migration history
SELECT * FROM "__EFMigrationsHistory";
```

## ⚠️ Migration Best Practices

### ✅ Yapılması Gerekenler
1. **Her migration öncesi yedek alın** (Supabase'de otomatik backup var)
2. **Test ortamında deneyin** (Önce development branch'te)
3. **Migration'ları atomic tutun** (Tek bir mantıksal değişiklik)
4. **Anlamlı isimler kullanın** (`AddUserPreferences`, `UpdateListingSchema`)

### ❌ Yapılmaması Gerekenler
1. **Uygulanan migration'ları değiştirmeyin** (Yeni migration oluşturun)
2. **Production'da doğrudan test etmeyin**
3. **Migration dosyalarını manuel düzenlemeyin** (EF Core'a bırakın)
4. **Breaking changes yapmadan data migration yapmayın**

## 🔄 Schema Değişikliği Workflow'u

1. **Entity'yi Değiştir** (`src/Swimago.Domain/Entities`)
2. **Migration Oluştur**
   ```bash
   dotnet ef migrations add DescriptiveName --project src/Swimago.Infrastructure --startup-project src/Swimago.API
   ```
3. **Migration'ı İncele** (`src/Swimago.Infrastructure/Migrations`)
4. **Test Et** (Local veya Supabase development branch)
5. **Uygula**
   ```bash
   dotnet ef database update --project src/Swimago.Infrastructure --startup-project src/Swimago.API
   ```
6. **Doğrula** (Supabase Table Editor'de kontrol et)

## 📝 Migration History

| Migration | Tarih | Açıklama |
|-----------|-------|----------|
| `20260203204953_InitialCreate_UUID` | 3 Şubat 2026 | İlk schema oluşturma (UUID tabanlı, PostGIS, JSONB) |

---

**Son Güncelleme:** 8 Şubat 2026  
**Database Provider:** Supabase PostgreSQL 15  
**ORM:** Entity Framework Core 8.0.11
