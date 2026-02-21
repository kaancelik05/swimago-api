# Swimago Projesi - Mimari ve Kod Kalitesi İnceleme Raporu

Bu rapor, projedeki .NET 9, Clean Architecture ve diğer backend standartları göz önüne alınarak `@dotnet-architect`, `@senior-architect` ve `.NET Backend Patterns` yetkinlikleri üzerinden hazırlanmıştır.

## 1. Mimari ve Temel Yapı İyileştirmeleri

### 1.1 `ApplicationDbContext` ve Konfigürasyonların Ayrıştırılması
Mevcut `ApplicationDbContext.cs` dosyasında `OnModelCreating` metodu yaklaşık 400 satır uzunluğunda ve tüm *Entity* ilişkilerini, PostGIS ayarlarını ve JSONB dönüşümlerini tek bir yerde barındırıyor. Her ne kadar `modelBuilder.ApplyConfigurationsFromAssembly` çağrılsa da kodun çoğu bu metodun içinde kalmış.
**Öneri:** Her bir Entity için `IEntityTypeConfiguration<T>` arayüzünden türeyen konfigürasyon sınıfları oluşturulmalıdır (örn. `ListingConfiguration`, `UserConfiguration`). Bu, DbContext'i temiz tutar ve Single Responsibility (Tek Sorumluluk) prensibine uyar.

### 1.2 Program.cs'in Sadeleştirilmesi
`Program.cs` içerisinde tüm repository ve servis kayıtları tek tek yapılmış. `builder.Services.AddApplication()` yöntemi kullanılmasına rağmen altyapı servisleri dışarıda kalmış.
**Öneri:** `DependencyInjection.cs` genişletme (extension) metotları her katman için oluşturulmalıdır (örn. `AddInfrastructure(this IServiceCollection services, IConfiguration config)`). Böylece `Program.cs` çok daha modüler ve okunabilir olur.

### 1.3 CQRS ve MediatR Kullanımı
Şu an Application katmanında geleneksel *Service/Repository* deseni kullanılıyor (örn. `ReservationService`). Bu yapı küçük/orta projelerde yeterli olsa da karmaşık rezervasyon kuralları ve çoklu dil yapısı düşünüldüğünde servis sınıfları hızla şişebilir (God Class antipattern).
**Öneri:** MediatR (veya benzeri bir kütüphane) ile **CQRS (Command Query Responsibility Segregation)** mimarisine geçilmesi; Read (okuma) ve Write (yazma) işlemlerinin birbirinden ayrılmasını, böylece her bir Use-Case'in (örn. `CreateReservationCommand`) kendi *Handler*'ı içinde izole edilmesini sağlar.

## 2. Hata Yönetimi ve Kontrol Akışı (Control Flow)

### 2.1 İstisnai Durumlar (Exceptions) Yerine Result Pattern
`ReservationService.cs` içindeki kontrollerde (örneğin müsaitlik olmaması durumunda) `throw new InvalidOperationException(...)` fırlatılıyor. İş kurallarının (Business Rules) ihlali teknik bir hata değildir, bir iş kuralı sonucudur. Kontrol akışını exception ile sağlamak performans maliyeti yaratır ve REST prensiplerini ihlal eder.
**Öneri:** FluentResults, ErrorOr veya özel yazılmış bir `Result<T>` sınıfı üzerinden işlemlerin başarılı/başarısız dönmesi sağlanmalıdır. `Result.Fail("Dates are already booked")` şeklinde Controller'a dönülüp orada standart bir hata formatına (`400 Bad Request`) çevrilmelidir. Hata formatı için de `ProblemDetails` standardı kullanılabilir.

### 2.2 Global Validasyon (FluentValidation) Mimarisinin Güçlendirilmesi
Görebildiğim kadarıyla Application/Validators klasörü mevcut. Ancak validator'ların Middleware veya MediatR Pipeline üzerinden otomatik çalışıp çalışmadığı belirsiz.
**Öneri:** Request'ler Controller'a düşmeden veya Handler'a girmeden önce FluentValidation Middleware / Pipeline Behavior tarafında yakalandığından ve API ucuna otomatik olarak 400 Validation Error Response döndüğünden emin olunmalıdır.

## 3. Veri, Performans ve Ölçeklenebilirlik

### 3.1 Caching (Önbellekleme) Mekanizması
Performans ve veritabanı yorgunluğunu azaltmak için `AppDbContext` üzerinden sık yapılan okumalar yavaşlatıcı olabilir. Özellikle listeleme ve "Destinations/Spots" sayfalarının sık sorgulanacağı düşünülürse:
**Öneri:**
- .NET 8/9 ile gelen **OutputCaching** aktif edilmelidir.
- Sık değişmeyen veriler için (Şehirler, Özellikler, Aktif Blog yazıları vb.) Redis bazlı veya `IMemoryCache` tabanlı bir caching servisi Application katmanına entegre edilmelidir.

### 3.2 Pagination (Sayfalama) Standardının Eksikliği
Genel dokümanda "Pagination standardı: items, total, page, pageSize" denmiş ancak servislerin dönüş türleri çoğunlukla `IEnumerable<T>`.
**Öneri:** `PagedResult<T>` isimli sarmalayıcı (wrapper) bir class yazılmalı. `IQueryable` üzerinden çalışan `ToPagedListAsync(page, pageSize)` gibi bir extension metot eklenerek hem veritabanı seviyesinde `Skip/Take` yapılması hem de sayfalama bilgilerinin API ucuna standart şekilde dönülmesi garanti altına alınmalıdır.

### 3.3 Soft Delete Uygulaması
`Listing`, `Reservation` gibi kritik entity'ler silindiğinde hard delete (tamamen veritabanından uçurma) yerine Soft Delete yapıları uygulanmalıdır. Veri bütünlüğünü ve olası uyuşmazlıkları (örn: ödemesi olan bir rezervasyonun silinmesi) korumak için, base bir `ISoftDeletable` interface ve EF Core *Global Query Filter* eklenmelidir.

## 4. Güvenlik ve Gözlemlenebilirlik (Observability)

### 4.1 Token Yönetimi ve Çıkış İşlemleri (Invalidation)
`AuthController.cs` içerisinde `Logout` fonksiyonunda "// TODO: Implement token blacklisting" (Token karaliste uygulamasını yap) ifadesi yer alıyor.
**Öneri:** Kullanıcı çıkış yaptığında token'ların geçerliliğini yitirmesi için JWT JTI (JWT ID) tabanlı bir Redis Blacklist yapısı kurulmalıdır veya Database üzerindeki Refresh Token "Revoked" (İptal edildi) olarak işaretlenmelidir.

### 4.2 Health Checks (Sistem Sağlık Taraması)
Modern mimarilerde projenin canlı (Liveness) ve hazır (Readiness) durumunu bildiren endpoint'ler şarttır.
**Öneri:** `Program.cs` içine `builder.Services.AddHealthChecks().AddNpgSql(..).AddRedis(..)` eklenmeli ve `/health` endpoint'i üzerinden Cloudflare/DevOps süreçlerinde sistemin (DB, R2, Redis vb.) sağlık durumu izlenmelidir.

### 4.3 Yapısal Loglama (Structured Logging)
Loglamalar yapılıyor ancak `ILogger` kullanımlarının çoğunlukla sadece terminal (Console) seviyesinde olduğu anlaşılıyor. Beklenmeyen durumlarda request detaylı izlenemeyebilir.
**Öneri:** Serilog kurularak, `{Email}` gibi yapısal log parametrelerinin ElasticSearch veya SEQ benzeri bir log yönetim sistemine (veya json dosyalarına) basılması sağlanmalıdır. Ayrıca global exception yakalandığında `TraceId` veya `CorrelationId` de dönülmelidir ki, kullanıcı hatayı müşteri temsilcisine ilettiğinde loglardan kolayca bulunabilsin.
