# Swimago API - Supabase PostgreSQL Kurulum Rehberi

Bu rehber, Swimago API projesini Supabase PostgreSQL ile nasıl kuracağınızı ve çalıştıracağınızı adım adım açıklar.

## 📋 Gereksinimler

- ✅ **.NET 8 SDK** - [İndir](https://dotnet.microsoft.com/download/dotnet/8.0)
- ✅ **Supabase Hesabı** - [Ücretsiz Kayıt](https://supabase.com)

## 🚀 Hızlı Başlangıç

### Adım 1: Supabase Projesi Oluşturma

1. [Supabase Dashboard](https://app.supabase.com)'a gidin
2. **New Project** butonuna tıklayın
3. Proje bilgilerini doldurun:
   - **Name**: `swimago-api`
   - **Database Password**: Güçlü bir şifre oluşturun (kaydedin!)
   - **Region**: `Frankfurt (eu-central-1)` (Türkiye'ye en yakın)
   - **Pricing Plan**: Free
4. **Create new project** butonuna tıklayın

### Adım 2: PostGIS Extension'ı Etkinleştirme

1. Supabase Dashboard → **SQL Editor**'e gidin
2. Aşağıdaki komutu çalıştırın:

```sql
CREATE EXTENSION IF NOT EXISTS postgis;
```

3. ✅ "Success. No rows returned" mesajını görmelisiniz

### Adım 3: Connection String'i Alma

1. Supabase Dashboard → **Settings** → **Database**'e gidin
2. **Connection Pooling** sekmesine tıklayın
3. **Connection string** modunu **URI** olarak değiştirin
4. Gösterilen connection string'i kopyalayın (şuna benzer):

```
postgresql://postgres.yupmknxjeezwiwayciws:[YOUR-PASSWORD]@aws-1-eu-central-1.pooler.supabase.com:5432/postgres
```

### Adım 4: Connection String'i Projeye Ekleme

1. Proje klasöründe `src/Swimago.API/appsettings.json` dosyasını açın
2. `ConnectionStrings` bölümünü güncelleyin:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=aws-1-eu-central-1.pooler.supabase.com;Database=postgres;Username=postgres.yupmknxjeezwiwayciws;Password=BURAYA_SİFRENİZİ_YAZIN;Port=5432;SslMode=Require;Trust Server Certificate=true"
  }
}
```

> ⚠️ **Önemli:** `Password=` kısmına Adım 1'de oluşturduğunuz şifreyi yazın!

### Adım 5: Migration'ları Uygulama

Terminal'de proje klasörüne gidin ve şu komutu çalıştırın:

```bash
dotnet ef database update --project src/Swimago.Infrastructure --startup-project src/Swimago.API
```

✅ **Başarılı olursa:** "Done" mesajını göreceksiniz.

❌ **Hata alırsanız:** Bağlantı bilgilerini kontrol edin (özellikle şifre).

### Adım 6: API'yi Çalıştırma

```bash
dotnet run --project src/Swimago.API
```

✅ **Başarılı log'lar:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5088
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

### Adım 7: Swagger UI'da Test Etme

Tarayıcınızda şu adrese gidin:

👉 **http://localhost:5088/swagger**

Tüm API endpoint'lerini görebilir ve test edebilirsiniz!

---

## 📊 Veritabanı Yapısı Kontrolü

Migration'ların doğru uygulandığını kontrol etmek için:

1. Supabase Dashboard → **Table Editor**'e gidin
2. Şu tabloları görmelisiniz:
   - ✅ Users
   - ✅ Listings
   - ✅ Reservations
   - ✅ Reviews
   - ✅ Favorites
   - ✅ PaymentMethods
   - ✅ Cities
   - ✅ Amenities
   - ✅ BlogPosts
   - ✅ NewsletterSubscribers

---

## 🔧 Sık Kullanılan Komutlar

### API'yi Çalıştırma (Normal)
```bash
dotnet run --project src/Swimago.API
```

### API'yi Çalıştırma (Hot Reload)
```bash
dotnet watch run --project src/Swimago.API
```

### Migration Durumunu Kontrol Etme
```bash
dotnet ef migrations list --project src/Swimago.Infrastructure --startup-project src/Swimago.API
```

### Yeni Migration Oluşturma
```bash
dotnet ef migrations add MigrationName --project src/Swimago.Infrastructure --startup-project src/Swimago.API
```

### Production Build
```bash
dotnet publish src/Swimago.API -c Release -o publish
```

---

## 🐛 Sorun Giderme

### Hata: "Login failed for user"

**Çözüm:** 
- Connection string'deki şifrenin doğru olduğundan emin olun
- Özel karakterler varsa URL encoding yapın (`@` → `%40`, `#` → `%23`)

### Hata: "Could not connect to the server"

**Çözüm:**
- İnternet bağlantınızı kontrol edin
- Supabase projesinin **paused** durumda olmadığını kontrol edin (Dashboard'dan)
- Firewall ayarlarınızı kontrol edin

### Hata: "PostGIS extension not found"

**Çözüm:**
- Adım 2'deki SQL komutunu yeniden çalıştırın
- Supabase SQL Editor'de şu sorguyu çalıştırın:
  ```sql
  SELECT * FROM pg_extension WHERE extname = 'postgis';
  ```

### Migration Hataları

**Çözüm:**
1. Mevcut migration durumunu kontrol edin:
   ```bash
   dotnet ef migrations list --project src/Swimago.Infrastructure --startup-project src/Swimago.API
   ```

2. Veritabanını sıfırdan oluşturmak için (DİKKAT: Tüm verileri siler):
   ```bash
   dotnet ef database drop --project src/Swimago.Infrastructure --startup-project src/Swimago.API
   dotnet ef database update --project src/Swimago.Infrastructure --startup-project src/Swimago.API
   ```

---

## 🔒 Güvenlik Notları

> ⚠️ **ÖNEMLİ:** `appsettings.json` dosyası Git'e commit edilmemelidir!

**Önerilen Yaklaşım (Production için):**

1. `.gitignore` dosyasına ekleyin:
   ```
   appsettings.*.json
   ```

2. Environment variable kullanın:
   ```bash
   export ConnectionStrings__DefaultConnection="Host=...;Database=...;"
   ```

3. Veya `appsettings.Production.json` oluşturun ve bunu Git'e eklemeyin

---

## 📚 Ek Kaynaklar

- 📖 [Supabase Database Documentation](https://supabase.com/docs/guides/database)
- 📖 [PostGIS Documentation](https://postgis.net/documentation/)
- 📖 [Entity Framework Core Migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- 📖 [Swimago API Endpoints](./SWIMAGO_DOCUMENTATION.md)

---

## ✅ Tamamlandı!

Artık Swimago API'niz Supabase PostgreSQL ile çalışıyor! 🎉

**Sırada ne var?**
- Test kullanıcısı oluşturun: `POST /api/auth/register`
- İlan ekleyin: `POST /api/listings`
- Rezervasyon yapın: `POST /api/reservations`

Sorularınız için: Swagger UI'daki endpoint'leri inceleyin! 📝
