using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using Swimago.Domain.Entities;
using Swimago.Domain.Enums;

namespace Swimago.Infrastructure.Data;

public static class MockDataSeeder
{
    private const string SeedMarkerSlug = "mock-bodrum-blue-flag-beach";

    private const string AdminEmail = "admin.mock@swimago.local";
    private const string HostOneEmail = "host.ayse.mock@swimago.local";
    private const string HostTwoEmail = "host.mehmet.mock@swimago.local";
    private const string GuestOneEmail = "guest.selim.mock@swimago.local";
    private const string GuestTwoEmail = "guest.elif.mock@swimago.local";

    public static async Task SeedAsync(ApplicationDbContext context, ILogger logger, CancellationToken cancellationToken = default)
    {
        if (await context.Listings.AnyAsync(x => x.Slug == SeedMarkerSlug, cancellationToken))
        {
            logger.LogInformation("Mock data already exists. Seed skipped.");
            return;
        }

        var now = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(now);
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("Test1234!");

        var ids = new MockIds();

        var users = new List<User>
        {
            new()
            {
                Id = ids.AdminUserId,
                Email = AdminEmail,
                PasswordHash = passwordHash,
                Role = Role.Admin,
                Status = UserStatus.Active,
                IsEmailVerified = true,
                RefreshTokenExpiry = now.AddDays(-1),
                CreatedAt = now.AddMonths(-12),
                MembershipLevel = "Platinum",
                NotificationSettings = new NotificationSettings { EmailNotifications = true, PushNotifications = true },
                LanguageSettings = new LanguageSettings { Language = "tr", Currency = "TRY" },
                PrivacySettings = new PrivacySettings { ProfileVisibility = true, DataSharing = false }
            },
            new()
            {
                Id = ids.HostOneUserId,
                Email = HostOneEmail,
                PasswordHash = passwordHash,
                Role = Role.Host,
                Status = UserStatus.Active,
                IsEmailVerified = true,
                RefreshTokenExpiry = now.AddDays(-1),
                CreatedAt = now.AddMonths(-8),
                MembershipLevel = "Gold",
                NotificationSettings = new NotificationSettings { EmailNotifications = true, PushNotifications = true },
                LanguageSettings = new LanguageSettings { Language = "tr", Currency = "TRY" },
                PrivacySettings = new PrivacySettings { ProfileVisibility = true, DataSharing = false }
            },
            new()
            {
                Id = ids.HostTwoUserId,
                Email = HostTwoEmail,
                PasswordHash = passwordHash,
                Role = Role.Host,
                Status = UserStatus.Active,
                IsEmailVerified = true,
                RefreshTokenExpiry = now.AddDays(-1),
                CreatedAt = now.AddMonths(-10),
                MembershipLevel = "Gold",
                NotificationSettings = new NotificationSettings { EmailNotifications = true, PushNotifications = true },
                LanguageSettings = new LanguageSettings { Language = "tr", Currency = "EUR" },
                PrivacySettings = new PrivacySettings { ProfileVisibility = true, DataSharing = false }
            },
            new()
            {
                Id = ids.GuestOneUserId,
                Email = GuestOneEmail,
                PasswordHash = passwordHash,
                Role = Role.Customer,
                Status = UserStatus.Active,
                IsEmailVerified = true,
                RefreshTokenExpiry = now.AddDays(-1),
                CreatedAt = now.AddMonths(-5),
                MembershipLevel = "Standard",
                NotificationSettings = new NotificationSettings { EmailNotifications = true, PushNotifications = false },
                LanguageSettings = new LanguageSettings { Language = "tr", Currency = "TRY" },
                PrivacySettings = new PrivacySettings { ProfileVisibility = true, DataSharing = false }
            },
            new()
            {
                Id = ids.GuestTwoUserId,
                Email = GuestTwoEmail,
                PasswordHash = passwordHash,
                Role = Role.Customer,
                Status = UserStatus.Active,
                IsEmailVerified = true,
                RefreshTokenExpiry = now.AddDays(-1),
                CreatedAt = now.AddMonths(-3),
                MembershipLevel = "Standard",
                NotificationSettings = new NotificationSettings { EmailNotifications = true, PushNotifications = true },
                LanguageSettings = new LanguageSettings { Language = "en", Currency = "EUR" },
                PrivacySettings = new PrivacySettings { ProfileVisibility = true, DataSharing = false }
            }
        };

        var profiles = new List<UserProfile>
        {
            new()
            {
                Id = ids.AdminProfileId,
                UserId = ids.AdminUserId,
                FirstName = new Dictionary<string, string> { ["tr"] = "System", ["en"] = "System" },
                LastName = new Dictionary<string, string> { ["tr"] = "Admin", ["en"] = "Admin" },
                PhoneNumber = "+90 555 000 0001",
                Avatar = "https://images.unsplash.com/photo-1568602471122-7832951cc4c5?auto=format&fit=crop&w=600&q=80",
                Bio = new Dictionary<string, string>
                {
                    ["tr"] = "Swimago platform yoneticisi",
                    ["en"] = "Swimago platform administrator"
                }
            },
            new()
            {
                Id = ids.HostOneProfileId,
                UserId = ids.HostOneUserId,
                FirstName = new Dictionary<string, string> { ["tr"] = "Ayse", ["en"] = "Ayse" },
                LastName = new Dictionary<string, string> { ["tr"] = "Demir", ["en"] = "Demir" },
                PhoneNumber = "+90 555 000 0002",
                Avatar = "https://images.unsplash.com/photo-1487412720507-e7ab37603c6f?auto=format&fit=crop&w=600&q=80",
                Bio = new Dictionary<string, string>
                {
                    ["tr"] = "8 yildir premium beach ve pool isletmeciligi",
                    ["en"] = "Premium beach and pool host for 8 years"
                }
            },
            new()
            {
                Id = ids.HostTwoProfileId,
                UserId = ids.HostTwoUserId,
                FirstName = new Dictionary<string, string> { ["tr"] = "Mehmet", ["en"] = "Mehmet" },
                LastName = new Dictionary<string, string> { ["tr"] = "Kaya", ["en"] = "Kaya" },
                PhoneNumber = "+90 555 000 0003",
                Avatar = "https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?auto=format&fit=crop&w=600&q=80",
                Bio = new Dictionary<string, string>
                {
                    ["tr"] = "Luks yat ve gunluk tur organizatoru",
                    ["en"] = "Luxury yacht and day-trip organizer"
                }
            },
            new()
            {
                Id = ids.GuestOneProfileId,
                UserId = ids.GuestOneUserId,
                FirstName = new Dictionary<string, string> { ["tr"] = "Selim", ["en"] = "Selim" },
                LastName = new Dictionary<string, string> { ["tr"] = "Arslan", ["en"] = "Arslan" },
                PhoneNumber = "+90 555 000 0004",
                Avatar = "https://images.unsplash.com/photo-1500648767791-00dcc994a43e?auto=format&fit=crop&w=600&q=80",
                Bio = new Dictionary<string, string>
                {
                    ["tr"] = "Deniz tutkunuyum, hafta sonlari rezervasyon yapiyorum",
                    ["en"] = "Sea lover, booking new spots every weekend"
                }
            },
            new()
            {
                Id = ids.GuestTwoProfileId,
                UserId = ids.GuestTwoUserId,
                FirstName = new Dictionary<string, string> { ["tr"] = "Elif", ["en"] = "Elif" },
                LastName = new Dictionary<string, string> { ["tr"] = "Yildiz", ["en"] = "Yildiz" },
                PhoneNumber = "+90 555 000 0005",
                Avatar = "https://images.unsplash.com/photo-1494790108377-be9c29b29330?auto=format&fit=crop&w=600&q=80",
                Bio = new Dictionary<string, string>
                {
                    ["tr"] = "Ailemle guvenli ve kaliteli mekanlari tercih ederim",
                    ["en"] = "I prefer safe and quality venues for family trips"
                }
            }
        };

        var cities = new List<City>
        {
            new()
            {
                Id = ids.CityIstanbulId,
                Name = new Dictionary<string, string> { ["tr"] = "Istanbul", ["en"] = "Istanbul" },
                Country = "Turkey",
                Latitude = 41.0082m,
                Longitude = 28.9784m,
                IsActive = true,
                CreatedAt = now.AddMonths(-24)
            },
            new()
            {
                Id = ids.CityBodrumId,
                Name = new Dictionary<string, string> { ["tr"] = "Bodrum", ["en"] = "Bodrum" },
                Country = "Turkey",
                Latitude = 37.0344m,
                Longitude = 27.4305m,
                IsActive = true,
                CreatedAt = now.AddMonths(-24)
            },
            new()
            {
                Id = ids.CityAntalyaId,
                Name = new Dictionary<string, string> { ["tr"] = "Antalya", ["en"] = "Antalya" },
                Country = "Turkey",
                Latitude = 36.8969m,
                Longitude = 30.7133m,
                IsActive = true,
                CreatedAt = now.AddMonths(-24)
            }
        };

        var amenities = new List<Amenity>
        {
            new()
            {
                Id = ids.AmenityWifiId,
                Icon = "wifi",
                Label = new Dictionary<string, string> { ["tr"] = "WiFi", ["en"] = "Wi-Fi" },
                Category = "comfort",
                ApplicableTo = new List<ListingType> { ListingType.Beach, ListingType.Pool, ListingType.Yacht, ListingType.DayTrip },
                IsActive = true,
                CreatedAt = now.AddMonths(-12)
            },
            new()
            {
                Id = ids.AmenityParkingId,
                Icon = "parking",
                Label = new Dictionary<string, string> { ["tr"] = "Otopark", ["en"] = "Parking" },
                Category = "transport",
                ApplicableTo = new List<ListingType> { ListingType.Beach, ListingType.Pool },
                IsActive = true,
                CreatedAt = now.AddMonths(-12)
            },
            new()
            {
                Id = ids.AmenityShowerId,
                Icon = "shower",
                Label = new Dictionary<string, string> { ["tr"] = "Dus", ["en"] = "Shower" },
                Category = "comfort",
                ApplicableTo = new List<ListingType> { ListingType.Beach, ListingType.Pool },
                IsActive = true,
                CreatedAt = now.AddMonths(-12)
            },
            new()
            {
                Id = ids.AmenityFoodId,
                Icon = "restaurant",
                Label = new Dictionary<string, string> { ["tr"] = "Yiyecek-Icecek", ["en"] = "Food & Drinks" },
                Category = "service",
                ApplicableTo = new List<ListingType> { ListingType.Beach, ListingType.Pool, ListingType.Yacht, ListingType.DayTrip },
                IsActive = true,
                CreatedAt = now.AddMonths(-12)
            },
            new()
            {
                Id = ids.AmenityMusicId,
                Icon = "music",
                Label = new Dictionary<string, string> { ["tr"] = "Canli Muzik", ["en"] = "Live Music" },
                Category = "entertainment",
                ApplicableTo = new List<ListingType> { ListingType.Beach, ListingType.Pool, ListingType.DayTrip },
                IsActive = true,
                CreatedAt = now.AddMonths(-12)
            },
            new()
            {
                Id = ids.AmenityCaptainId,
                Icon = "captain",
                Label = new Dictionary<string, string> { ["tr"] = "Profesyonel Kaptan", ["en"] = "Professional Captain" },
                Category = "crew",
                ApplicableTo = new List<ListingType> { ListingType.Yacht, ListingType.DayTrip },
                IsActive = true,
                CreatedAt = now.AddMonths(-12)
            },
            new()
            {
                Id = ids.AmenityLifeJacketId,
                Icon = "lifejacket",
                Label = new Dictionary<string, string> { ["tr"] = "Can Yelegi", ["en"] = "Life Jacket" },
                Category = "safety",
                ApplicableTo = new List<ListingType> { ListingType.Yacht, ListingType.DayTrip },
                IsActive = true,
                CreatedAt = now.AddMonths(-12)
            }
        };

        var listings = new List<Listing>
        {
            new()
            {
                Id = ids.ListingBeachActiveId,
                HostId = ids.HostOneUserId,
                Type = ListingType.Beach,
                Status = ListingStatus.Active,
                IsActive = true,
                IsFeatured = true,
                Slug = SeedMarkerSlug,
                Title = new Dictionary<string, string>
                {
                    ["tr"] = "Bodrum Blue Flag Beach",
                    ["en"] = "Bodrum Blue Flag Beach"
                },
                Description = new Dictionary<string, string>
                {
                    ["tr"] = "Temiz denizi, ozel localari ve gun batimi manzarasi ile premium beach deneyimi.",
                    ["en"] = "Premium beach experience with clean sea, private cabanas and sunset view."
                },
                Address = new Dictionary<string, string>
                {
                    ["tr"] = "Yalikavak Mah. Sahil Cad. No:12",
                    ["en"] = "Yalikavak District, Beach Street No:12"
                },
                City = "Bodrum",
                Country = "Turkey",
                Latitude = 37.1234m,
                Longitude = 27.4567m,
                Location = CreatePoint(37.1234m, 27.4567m),
                MaxGuestCount = 6,
                BasePricePerHour = 45m,
                BasePricePerDay = 230m,
                PriceRangeMin = 180m,
                PriceRangeMax = 420m,
                PriceCurrency = "EUR",
                Conditions = new List<ListingCondition>
                {
                    new() { Icon = "sun", IconColor = "#f59e0b", BgColor = "#fffbeb", Label = "Hava", Value = "Acik" },
                    new() { Icon = "wave", IconColor = "#0ea5e9", BgColor = "#eff6ff", Label = "Dalga", Value = "Dusuk" }
                },
                CreatedAt = now.AddMonths(-7),
                UpdatedAt = now.AddDays(-4),
                Rating = 4.8m,
                ReviewCount = 32,
                SpotCount = 48,
                IsSuperhost = true
            },
            new()
            {
                Id = ids.ListingPoolActiveId,
                HostId = ids.HostOneUserId,
                Type = ListingType.Pool,
                Status = ListingStatus.Active,
                IsActive = true,
                IsFeatured = false,
                Slug = "mock-istanbul-skyline-pool",
                Title = new Dictionary<string, string>
                {
                    ["tr"] = "Istanbul Skyline Pool Club",
                    ["en"] = "Istanbul Skyline Pool Club"
                },
                Description = new Dictionary<string, string>
                {
                    ["tr"] = "Sehir manzarali havuz, DJ performansi ve aileye uygun alanlar.",
                    ["en"] = "City-view pool with DJ sets and family-friendly zones."
                },
                Address = new Dictionary<string, string>
                {
                    ["tr"] = "Besiktas Nispetiye Cad. No:44",
                    ["en"] = "Besiktas Nispetiye Street No:44"
                },
                City = "Istanbul",
                Country = "Turkey",
                Latitude = 41.0811m,
                Longitude = 29.0095m,
                Location = CreatePoint(41.0811m, 29.0095m),
                MaxGuestCount = 5,
                BasePricePerHour = 35m,
                BasePricePerDay = 160m,
                PriceRangeMin = 130m,
                PriceRangeMax = 300m,
                PriceCurrency = "TRY",
                Conditions = new List<ListingCondition>
                {
                    new() { Icon = "temperature", IconColor = "#f97316", BgColor = "#fff7ed", Label = "Su Sicakligi", Value = "27C" },
                    new() { Icon = "music", IconColor = "#a855f7", BgColor = "#faf5ff", Label = "Etkinlik", Value = "DJ Night" }
                },
                CreatedAt = now.AddMonths(-6),
                UpdatedAt = now.AddDays(-2),
                Rating = 4.4m,
                ReviewCount = 19,
                SpotCount = 36,
                IsSuperhost = false
            },
            new()
            {
                Id = ids.ListingYachtActiveId,
                HostId = ids.HostTwoUserId,
                Type = ListingType.Yacht,
                Status = ListingStatus.Active,
                IsActive = true,
                IsFeatured = true,
                Slug = "mock-bodrum-luxury-yacht",
                Title = new Dictionary<string, string>
                {
                    ["tr"] = "Bodrum Luxury Yacht Charter",
                    ["en"] = "Bodrum Luxury Yacht Charter"
                },
                Description = new Dictionary<string, string>
                {
                    ["tr"] = "Mavi yolculuk icin kaptanli ve full servis luks yat turu.",
                    ["en"] = "Luxury blue-cruise yacht tour with captain and full service."
                },
                Address = new Dictionary<string, string>
                {
                    ["tr"] = "Bodrum Marina, Iskele 5",
                    ["en"] = "Bodrum Marina, Pier 5"
                },
                City = "Bodrum",
                Country = "Turkey",
                Latitude = 37.0332m,
                Longitude = 27.4301m,
                Location = CreatePoint(37.0332m, 27.4301m),
                MaxGuestCount = 12,
                BasePricePerHour = 120m,
                BasePricePerDay = 980m,
                PriceRangeMin = 850m,
                PriceRangeMax = 1800m,
                PriceCurrency = "EUR",
                Conditions = new List<ListingCondition>
                {
                    new() { Icon = "wind", IconColor = "#2563eb", BgColor = "#eff6ff", Label = "Ruzgar", Value = "Orta" },
                    new() { Icon = "visibility", IconColor = "#0f766e", BgColor = "#f0fdfa", Label = "Gorus", Value = "Cok iyi" }
                },
                Details = "{\"length\":\"18m\",\"cabins\":3,\"crew\":2,\"model\":\"Azimut 55\"}",
                CreatedAt = now.AddMonths(-9),
                UpdatedAt = now.AddDays(-3),
                Rating = 4.9m,
                ReviewCount = 41,
                SpotCount = 8,
                Duration = "Full day",
                IsSuperhost = true
            },
            new()
            {
                Id = ids.ListingDayTripActiveId,
                HostId = ids.HostTwoUserId,
                Type = ListingType.DayTrip,
                Status = ListingStatus.Active,
                IsActive = true,
                IsFeatured = false,
                Slug = "mock-antalya-sunset-daytrip",
                Title = new Dictionary<string, string>
                {
                    ["tr"] = "Antalya Sunset Day Trip",
                    ["en"] = "Antalya Sunset Day Trip"
                },
                Description = new Dictionary<string, string>
                {
                    ["tr"] = "Koylar arasinda yuzme molali, yemek dahil gunluk tekne turu.",
                    ["en"] = "Daily boat tour with swimming breaks and meal included."
                },
                Address = new Dictionary<string, string>
                {
                    ["tr"] = "Kaleici Liman, Iskele 2",
                    ["en"] = "Kaleici Harbor, Pier 2"
                },
                City = "Antalya",
                Country = "Turkey",
                Latitude = 36.8852m,
                Longitude = 30.7044m,
                Location = CreatePoint(36.8852m, 30.7044m),
                MaxGuestCount = 20,
                BasePricePerHour = 30m,
                BasePricePerDay = 95m,
                PriceRangeMin = 70m,
                PriceRangeMax = 180m,
                PriceCurrency = "EUR",
                Conditions = new List<ListingCondition>
                {
                    new() { Icon = "sun", IconColor = "#f59e0b", BgColor = "#fffbeb", Label = "Hava", Value = "Gunesli" },
                    new() { Icon = "sea", IconColor = "#0891b2", BgColor = "#ecfeff", Label = "Deniz", Value = "Sakin" }
                },
                Details = "{\"route\":[\"Mermerli\",\"Sican Adasi\",\"Falez\"],\"included\":[\"Lunch\",\"Soft drinks\"]}",
                CreatedAt = now.AddMonths(-4),
                UpdatedAt = now.AddDays(-1),
                Rating = 4.6m,
                ReviewCount = 24,
                SpotCount = 22,
                Duration = "8 hours",
                IsSuperhost = false
            },
            new()
            {
                Id = ids.ListingBeachPendingId,
                HostId = ids.HostOneUserId,
                Type = ListingType.Beach,
                Status = ListingStatus.Pending,
                IsActive = false,
                IsFeatured = false,
                Slug = "mock-bodrum-hidden-cove-beach",
                Title = new Dictionary<string, string>
                {
                    ["tr"] = "Bodrum Hidden Cove Beach",
                    ["en"] = "Bodrum Hidden Cove Beach"
                },
                Description = new Dictionary<string, string>
                {
                    ["tr"] = "Yeni acilacak, sakin koyda butik beach alanı.",
                    ["en"] = "Soon-to-open boutique beach area in a calm bay."
                },
                Address = new Dictionary<string, string>
                {
                    ["tr"] = "Gundogan Koyu",
                    ["en"] = "Gundogan Bay"
                },
                City = "Bodrum",
                Country = "Turkey",
                Latitude = 37.1700m,
                Longitude = 27.3000m,
                Location = CreatePoint(37.1700m, 27.3000m),
                MaxGuestCount = 4,
                BasePricePerHour = 20m,
                BasePricePerDay = 110m,
                PriceRangeMin = 90m,
                PriceRangeMax = 190m,
                PriceCurrency = "EUR",
                CreatedAt = now.AddDays(-20),
                UpdatedAt = now.AddDays(-7),
                Rating = 0,
                ReviewCount = 0,
                SpotCount = 12,
                IsSuperhost = false
            },
            new()
            {
                Id = ids.ListingPoolInactiveId,
                HostId = ids.HostTwoUserId,
                Type = ListingType.Pool,
                Status = ListingStatus.Inactive,
                IsActive = false,
                IsFeatured = false,
                Slug = "mock-cesme-infinity-pool",
                Title = new Dictionary<string, string>
                {
                    ["tr"] = "Cesme Infinity Pool",
                    ["en"] = "Cesme Infinity Pool"
                },
                Description = new Dictionary<string, string>
                {
                    ["tr"] = "Sezon disi bakimda olan infinity pool.",
                    ["en"] = "Infinity pool currently under seasonal maintenance."
                },
                Address = new Dictionary<string, string>
                {
                    ["tr"] = "Cesme Marina",
                    ["en"] = "Cesme Marina"
                },
                City = "Istanbul",
                Country = "Turkey",
                Latitude = 41.0300m,
                Longitude = 28.9900m,
                Location = CreatePoint(41.0300m, 28.9900m),
                MaxGuestCount = 4,
                BasePricePerHour = 25m,
                BasePricePerDay = 140m,
                PriceRangeMin = 110m,
                PriceRangeMax = 240m,
                PriceCurrency = "TRY",
                CreatedAt = now.AddMonths(-2),
                UpdatedAt = now.AddDays(-10),
                Rating = 4.1m,
                ReviewCount = 7,
                SpotCount = 16,
                IsSuperhost = false
            }
        };

        var listingImages = new List<ListingImage>
        {
            new() { Id = ids.ListingImage1Id, ListingId = ids.ListingBeachActiveId, Url = "https://images.unsplash.com/photo-1507525428034-b723cf961d3e?auto=format&fit=crop&w=1200&q=80", Alt = "Beach main", DisplayOrder = 0, IsCover = true },
            new() { Id = ids.ListingImage2Id, ListingId = ids.ListingBeachActiveId, Url = "https://images.unsplash.com/photo-1473116763249-2faaef81ccda?auto=format&fit=crop&w=1200&q=80", Alt = "Beach cabanas", DisplayOrder = 1, IsCover = false },
            new() { Id = ids.ListingImage3Id, ListingId = ids.ListingPoolActiveId, Url = "https://images.unsplash.com/photo-1602738328654-51ab2ae6c4ff?auto=format&fit=crop&w=1200&q=80", Alt = "Pool main", DisplayOrder = 0, IsCover = true },
            new() { Id = ids.ListingImage4Id, ListingId = ids.ListingPoolActiveId, Url = "https://images.unsplash.com/photo-1575429198097-0414ec08e8cd?auto=format&fit=crop&w=1200&q=80", Alt = "Pool lounge", DisplayOrder = 1, IsCover = false },
            new() { Id = ids.ListingImage5Id, ListingId = ids.ListingYachtActiveId, Url = "https://images.unsplash.com/photo-1567899378494-47b22a2ae96a?auto=format&fit=crop&w=1200&q=80", Alt = "Yacht exterior", DisplayOrder = 0, IsCover = true },
            new() { Id = ids.ListingImage6Id, ListingId = ids.ListingYachtActiveId, Url = "https://images.unsplash.com/photo-1544551763-46a013bb70d5?auto=format&fit=crop&w=1200&q=80", Alt = "Yacht deck", DisplayOrder = 1, IsCover = false },
            new() { Id = ids.ListingImage7Id, ListingId = ids.ListingDayTripActiveId, Url = "https://images.unsplash.com/photo-1528150177508-62a5f72e5678?auto=format&fit=crop&w=1200&q=80", Alt = "Day trip boat", DisplayOrder = 0, IsCover = true },
            new() { Id = ids.ListingImage8Id, ListingId = ids.ListingDayTripActiveId, Url = "https://images.unsplash.com/photo-1500375592092-40eb2168fd21?auto=format&fit=crop&w=1200&q=80", Alt = "Sunset route", DisplayOrder = 1, IsCover = false },
            new() { Id = ids.ListingImage9Id, ListingId = ids.ListingBeachPendingId, Url = "https://images.unsplash.com/photo-1493558103817-58b2924bce98?auto=format&fit=crop&w=1200&q=80", Alt = "Pending beach", DisplayOrder = 0, IsCover = true },
            new() { Id = ids.ListingImage10Id, ListingId = ids.ListingPoolInactiveId, Url = "https://images.unsplash.com/photo-1454944338482-a69bb95894af?auto=format&fit=crop&w=1200&q=80", Alt = "Inactive pool", DisplayOrder = 0, IsCover = true }
        };

        var listingAmenities = new List<ListingAmenity>
        {
            new() { ListingId = ids.ListingBeachActiveId, AmenityId = ids.AmenityWifiId, IsEnabled = true },
            new() { ListingId = ids.ListingBeachActiveId, AmenityId = ids.AmenityParkingId, IsEnabled = true },
            new() { ListingId = ids.ListingBeachActiveId, AmenityId = ids.AmenityShowerId, IsEnabled = true },
            new() { ListingId = ids.ListingBeachActiveId, AmenityId = ids.AmenityFoodId, IsEnabled = true },
            new() { ListingId = ids.ListingPoolActiveId, AmenityId = ids.AmenityWifiId, IsEnabled = true },
            new() { ListingId = ids.ListingPoolActiveId, AmenityId = ids.AmenityMusicId, IsEnabled = true },
            new() { ListingId = ids.ListingPoolActiveId, AmenityId = ids.AmenityFoodId, IsEnabled = true },
            new() { ListingId = ids.ListingYachtActiveId, AmenityId = ids.AmenityWifiId, IsEnabled = true },
            new() { ListingId = ids.ListingYachtActiveId, AmenityId = ids.AmenityCaptainId, IsEnabled = true },
            new() { ListingId = ids.ListingYachtActiveId, AmenityId = ids.AmenityLifeJacketId, IsEnabled = true },
            new() { ListingId = ids.ListingDayTripActiveId, AmenityId = ids.AmenityCaptainId, IsEnabled = true },
            new() { ListingId = ids.ListingDayTripActiveId, AmenityId = ids.AmenityLifeJacketId, IsEnabled = true },
            new() { ListingId = ids.ListingDayTripActiveId, AmenityId = ids.AmenityFoodId, IsEnabled = true },
            new() { ListingId = ids.ListingBeachPendingId, AmenityId = ids.AmenityShowerId, IsEnabled = true },
            new() { ListingId = ids.ListingPoolInactiveId, AmenityId = ids.AmenityWifiId, IsEnabled = true }
        };

        var dailyPricings = new List<DailyPricing>
        {
            new() { Id = ids.DailyPricing1Id, ListingId = ids.ListingBeachActiveId, Date = today.AddDays(1), Price = 240m, HourlyPrice = 48m, IsAvailable = true, Label = "Weekend", Notes = "High demand" },
            new() { Id = ids.DailyPricing2Id, ListingId = ids.ListingBeachActiveId, Date = today.AddDays(2), Price = 250m, HourlyPrice = 50m, IsAvailable = true, Label = "Weekend", Notes = null },
            new() { Id = ids.DailyPricing3Id, ListingId = ids.ListingBeachActiveId, Date = today.AddDays(3), Price = 220m, HourlyPrice = 44m, IsAvailable = true, Label = "Weekday", Notes = null },
            new() { Id = ids.DailyPricing4Id, ListingId = ids.ListingPoolActiveId, Date = today.AddDays(1), Price = 170m, HourlyPrice = 36m, IsAvailable = true, Label = "Prime", Notes = "DJ set" },
            new() { Id = ids.DailyPricing5Id, ListingId = ids.ListingPoolActiveId, Date = today.AddDays(2), Price = 165m, HourlyPrice = 35m, IsAvailable = true, Label = "Prime", Notes = null },
            new() { Id = ids.DailyPricing6Id, ListingId = ids.ListingYachtActiveId, Date = today.AddDays(5), Price = 1100m, HourlyPrice = 135m, IsAvailable = true, Label = "Luxury", Notes = "Fuel included" },
            new() { Id = ids.DailyPricing7Id, ListingId = ids.ListingYachtActiveId, Date = today.AddDays(6), Price = 1120m, HourlyPrice = 140m, IsAvailable = true, Label = "Luxury", Notes = null },
            new() { Id = ids.DailyPricing8Id, ListingId = ids.ListingDayTripActiveId, Date = today.AddDays(3), Price = 105m, HourlyPrice = 32m, IsAvailable = true, Label = "Popular", Notes = "Lunch included" },
            new() { Id = ids.DailyPricing9Id, ListingId = ids.ListingDayTripActiveId, Date = today.AddDays(4), Price = 95m, HourlyPrice = 30m, IsAvailable = true, Label = "Standard", Notes = null }
        };

        var availabilityBlocks = new List<AvailabilityBlock>
        {
            new()
            {
                Id = ids.AvailabilityBlock1Id,
                ListingId = ids.ListingBeachActiveId,
                StartDate = today.AddDays(10),
                EndDate = today.AddDays(11),
                IsAvailable = false,
                Reason = "Private event",
                CustomPrice = null
            },
            new()
            {
                Id = ids.AvailabilityBlock2Id,
                ListingId = ids.ListingYachtActiveId,
                StartDate = today.AddDays(15),
                EndDate = today.AddDays(16),
                IsAvailable = false,
                Reason = "Maintenance",
                CustomPrice = null
            }
        };

        var hostBusinessSettings = new List<HostBusinessSettings>
        {
            new()
            {
                Id = ids.HostBusinessSettings1Id,
                HostId = ids.HostOneUserId,
                AutoConfirmReservations = false,
                AllowSameDayBookings = true,
                MinimumNoticeHours = 3,
                CancellationWindowHours = 24,
                DynamicPricingEnabled = true,
                SmartOverbookingProtection = true,
                WhatsappNotifications = true,
                EmailNotifications = true,
                CreatedAt = now.AddMonths(-7),
                UpdatedAt = now.AddDays(-2)
            },
            new()
            {
                Id = ids.HostBusinessSettings2Id,
                HostId = ids.HostTwoUserId,
                AutoConfirmReservations = true,
                AllowSameDayBookings = false,
                MinimumNoticeHours = 12,
                CancellationWindowHours = 48,
                DynamicPricingEnabled = true,
                SmartOverbookingProtection = true,
                WhatsappNotifications = false,
                EmailNotifications = true,
                CreatedAt = now.AddMonths(-8),
                UpdatedAt = now.AddDays(-1)
            }
        };

        var hostListingMetadata = new List<HostListingMetadata>
        {
            new()
            {
                Id = ids.HostListingMetadata1Id,
                ListingId = ids.ListingBeachActiveId,
                Highlights = new List<string> { "Private cabanas", "Blue Flag sea", "Family-friendly" },
                SeatingAreas = new List<HostSeatingArea>
                {
                    new() { Id = "A", Name = "Premium Front", Capacity = 2, PriceMultiplier = 1.5m, IsVip = true, MinSpend = 120m },
                    new() { Id = "B", Name = "Garden", Capacity = 4, PriceMultiplier = 1.0m, IsVip = false, MinSpend = null }
                },
                AvailabilityNotes = "Peak season weekends fill up quickly",
                CreatedAt = now.AddMonths(-7),
                UpdatedAt = now.AddDays(-2)
            },
            new()
            {
                Id = ids.HostListingMetadata2Id,
                ListingId = ids.ListingPoolActiveId,
                Highlights = new List<string> { "Skyline view", "DJ events", "Kids area" },
                SeatingAreas = new List<HostSeatingArea>
                {
                    new() { Id = "POOL-1", Name = "Cabana", Capacity = 3, PriceMultiplier = 1.3m, IsVip = true, MinSpend = 80m },
                    new() { Id = "POOL-2", Name = "Deck", Capacity = 5, PriceMultiplier = 1.0m, IsVip = false, MinSpend = null }
                },
                AvailabilityNotes = "Events on Friday and Saturday",
                CreatedAt = now.AddMonths(-6),
                UpdatedAt = now.AddDays(-1)
            },
            new()
            {
                Id = ids.HostListingMetadata3Id,
                ListingId = ids.ListingYachtActiveId,
                Highlights = new List<string> { "Crewed charter", "Premium sound system", "Sundeck" },
                SeatingAreas = new List<HostSeatingArea>
                {
                    new() { Id = "Y-LOUNGE", Name = "Lounge Deck", Capacity = 8, PriceMultiplier = 1.2m, IsVip = false, MinSpend = null },
                    new() { Id = "Y-VIP", Name = "VIP Bow", Capacity = 4, PriceMultiplier = 1.7m, IsVip = true, MinSpend = 250m }
                },
                AvailabilityNotes = "Long routes require 24h notice",
                CreatedAt = now.AddMonths(-9),
                UpdatedAt = now.AddDays(-3)
            },
            new()
            {
                Id = ids.HostListingMetadata4Id,
                ListingId = ids.ListingDayTripActiveId,
                Highlights = new List<string> { "Lunch included", "Swim stops", "Sunset route" },
                SeatingAreas = new List<HostSeatingArea>
                {
                    new() { Id = "DT-STD", Name = "Main Deck", Capacity = 16, PriceMultiplier = 1.0m, IsVip = false, MinSpend = null },
                    new() { Id = "DT-VIP", Name = "Upper Deck", Capacity = 4, PriceMultiplier = 1.4m, IsVip = true, MinSpend = 90m }
                },
                AvailabilityNotes = "Route can change due to weather",
                CreatedAt = now.AddMonths(-4),
                UpdatedAt = now.AddDays(-1)
            }
        };

        var paymentMethods = new List<PaymentMethod>
        {
            new()
            {
                Id = ids.PaymentMethod1Id,
                UserId = ids.GuestOneUserId,
                Type = "card",
                Brand = PaymentBrand.Visa,
                Last4 = "4242",
                ExpiryMonth = 12,
                ExpiryYear = now.Year + 2,
                IsDefault = true,
                CreatedAt = now.AddMonths(-2),
                ProviderToken = "pm_mock_guest1_default"
            },
            new()
            {
                Id = ids.PaymentMethod2Id,
                UserId = ids.GuestTwoUserId,
                Type = "card",
                Brand = PaymentBrand.Mastercard,
                Last4 = "5454",
                ExpiryMonth = 6,
                ExpiryYear = now.Year + 3,
                IsDefault = true,
                CreatedAt = now.AddMonths(-1),
                ProviderToken = "pm_mock_guest2_default"
            }
        };

        var reservations = new List<Reservation>
        {
            new()
            {
                Id = ids.Reservation1Id,
                ListingId = ids.ListingBeachActiveId,
                GuestId = ids.GuestOneUserId,
                VenueType = VenueType.Beach,
                BookingType = BookingType.Daily,
                StartTime = now.AddDays(-20).Date.AddHours(10),
                EndTime = now.AddDays(-19).Date.AddHours(18),
                GuestCount = 2,
                Guests = new GuestDetails { Adults = 2, Children = 0, Seniors = 0 },
                Selections = new ReservationSelections { Sunbeds = 2, Cabanas = 1 },
                UnitPrice = 220m,
                UnitCount = 1,
                TotalPrice = 220m,
                DiscountAmount = 20m,
                FinalPrice = 200m,
                Currency = "EUR",
                PriceBreakdown = new List<PriceBreakdownItem>
                {
                    new() { Label = "Base package", Amount = 220m },
                    new() { Label = "Loyalty discount", Amount = -20m }
                },
                Status = ReservationStatus.Completed,
                Source = ReservationSource.Online,
                ConfirmationNumber = "SWMOCK240001",
                CheckInCode = "BEACH2001",
                SpecialRequests = new Dictionary<string, string> { ["tr"] = "Sessiz bolge tercih edilir", ["en"] = "Prefer a quiet zone" },
                CreatedAt = now.AddDays(-24),
                ConfirmedAt = now.AddDays(-23),
                CheckedInAt = now.AddDays(-20).Date.AddHours(10)
            },
            new()
            {
                Id = ids.Reservation2Id,
                ListingId = ids.ListingYachtActiveId,
                GuestId = ids.GuestOneUserId,
                VenueType = VenueType.Yacht,
                BookingType = BookingType.Daily,
                StartTime = now.AddDays(10).Date.AddHours(9),
                EndTime = now.AddDays(11).Date.AddHours(18),
                GuestCount = 6,
                Guests = new GuestDetails { Adults = 4, Children = 2, Seniors = 0 },
                Selections = new ReservationSelections { Sunbeds = 4, Cabanas = 0 },
                UnitPrice = 980m,
                UnitCount = 1,
                TotalPrice = 980m,
                DiscountAmount = 0,
                FinalPrice = 980m,
                Currency = "EUR",
                PriceBreakdown = new List<PriceBreakdownItem>
                {
                    new() { Label = "Charter fee", Amount = 980m }
                },
                Status = ReservationStatus.Confirmed,
                Source = ReservationSource.Online,
                ConfirmationNumber = "SWMOCK240002",
                CheckInCode = "YACHT9802",
                SpecialRequests = new Dictionary<string, string> { ["tr"] = "Vegetaryen menu", ["en"] = "Vegetarian menu" },
                CreatedAt = now.AddDays(-5),
                ConfirmedAt = now.AddDays(-4)
            },
            new()
            {
                Id = ids.Reservation3Id,
                ListingId = ids.ListingPoolActiveId,
                GuestId = ids.GuestTwoUserId,
                VenueType = VenueType.Pool,
                BookingType = BookingType.Daily,
                StartTime = now.AddDays(5).Date.AddHours(11),
                EndTime = now.AddDays(6).Date.AddHours(17),
                GuestCount = 3,
                Guests = new GuestDetails { Adults = 2, Children = 1, Seniors = 0 },
                Selections = new ReservationSelections { Sunbeds = 3, Cabanas = 0 },
                UnitPrice = 160m,
                UnitCount = 1,
                TotalPrice = 160m,
                DiscountAmount = 0,
                FinalPrice = 160m,
                Currency = "TRY",
                PriceBreakdown = new List<PriceBreakdownItem>
                {
                    new() { Label = "Pool entrance", Amount = 160m }
                },
                Status = ReservationStatus.Pending,
                Source = ReservationSource.Online,
                ConfirmationNumber = "SWMOCK240003",
                CheckInCode = null,
                SpecialRequests = new Dictionary<string, string> { ["tr"] = "Cocuk icin golgelik", ["en"] = "Shade area for child" },
                CreatedAt = now.AddDays(-1)
            },
            new()
            {
                Id = ids.Reservation4Id,
                ListingId = ids.ListingDayTripActiveId,
                GuestId = ids.GuestTwoUserId,
                VenueType = VenueType.DayTrip,
                BookingType = BookingType.Daily,
                StartTime = now.AddDays(-7).Date.AddHours(8),
                EndTime = now.AddDays(-7).Date.AddHours(18),
                GuestCount = 2,
                Guests = new GuestDetails { Adults = 2, Children = 0, Seniors = 0 },
                Selections = new ReservationSelections { Sunbeds = 2, Cabanas = 0 },
                UnitPrice = 95m,
                UnitCount = 2,
                TotalPrice = 190m,
                DiscountAmount = 0,
                FinalPrice = 190m,
                Currency = "EUR",
                PriceBreakdown = new List<PriceBreakdownItem>
                {
                    new() { Label = "Day-trip tickets", Amount = 190m }
                },
                Status = ReservationStatus.Cancelled,
                Source = ReservationSource.Phone,
                ConfirmationNumber = "SWMOCK240004",
                CheckInCode = null,
                SpecialRequests = new Dictionary<string, string> { ["tr"] = "On sirada oturma", ["en"] = "Front-row seats" },
                CreatedAt = now.AddDays(-12),
                ConfirmedAt = now.AddDays(-11),
                CancelledAt = now.AddDays(-8),
                CancellationReason = "Weather warning"
            },
            new()
            {
                Id = ids.Reservation5Id,
                ListingId = ids.ListingYachtActiveId,
                GuestId = ids.GuestTwoUserId,
                VenueType = VenueType.Yacht,
                BookingType = BookingType.Daily,
                StartTime = now.AddDays(-3).Date.AddHours(10),
                EndTime = now.AddDays(-2).Date.AddHours(18),
                GuestCount = 5,
                Guests = new GuestDetails { Adults = 5, Children = 0, Seniors = 0 },
                Selections = new ReservationSelections { Sunbeds = 5, Cabanas = 1 },
                UnitPrice = 900m,
                UnitCount = 1,
                TotalPrice = 900m,
                DiscountAmount = 50m,
                FinalPrice = 850m,
                Currency = "EUR",
                PriceBreakdown = new List<PriceBreakdownItem>
                {
                    new() { Label = "Charter fee", Amount = 900m },
                    new() { Label = "Promo discount", Amount = -50m }
                },
                Status = ReservationStatus.Completed,
                Source = ReservationSource.WalkIn,
                ConfirmationNumber = "SWMOCK240005",
                CheckInCode = "YACHT8505",
                SpecialRequests = new Dictionary<string, string> { ["tr"] = "Dogum gunu surprizi", ["en"] = "Birthday surprise setup" },
                CreatedAt = now.AddDays(-9),
                ConfirmedAt = now.AddDays(-8),
                CheckedInAt = now.AddDays(-3).Date.AddHours(10)
            }
        };

        var reservationPayments = new List<ReservationPayment>
        {
            new()
            {
                Id = ids.ReservationPayment1Id,
                ReservationId = ids.Reservation1Id,
                PaymentMethodId = ids.PaymentMethod1Id,
                Amount = 200m,
                Currency = "EUR",
                Status = PaymentStatus.Completed,
                ProviderTransactionId = "txn_mock_2001",
                PaymentIntentId = "pi_mock_2001",
                CreatedAt = now.AddDays(-24),
                PaidAt = now.AddDays(-23)
            },
            new()
            {
                Id = ids.ReservationPayment2Id,
                ReservationId = ids.Reservation2Id,
                PaymentMethodId = ids.PaymentMethod1Id,
                Amount = 980m,
                Currency = "EUR",
                Status = PaymentStatus.Completed,
                ProviderTransactionId = "txn_mock_2002",
                PaymentIntentId = "pi_mock_2002",
                CreatedAt = now.AddDays(-5),
                PaidAt = now.AddDays(-4)
            },
            new()
            {
                Id = ids.ReservationPayment3Id,
                ReservationId = ids.Reservation3Id,
                PaymentMethodId = ids.PaymentMethod2Id,
                Amount = 160m,
                Currency = "TRY",
                Status = PaymentStatus.Pending,
                ProviderTransactionId = null,
                PaymentIntentId = "pi_mock_2003",
                CreatedAt = now.AddDays(-1)
            },
            new()
            {
                Id = ids.ReservationPayment4Id,
                ReservationId = ids.Reservation4Id,
                PaymentMethodId = ids.PaymentMethod2Id,
                Amount = 190m,
                Currency = "EUR",
                Status = PaymentStatus.Refunded,
                ProviderTransactionId = "txn_mock_2004",
                PaymentIntentId = "pi_mock_2004",
                CreatedAt = now.AddDays(-12),
                PaidAt = now.AddDays(-11),
                RefundedAt = now.AddDays(-8),
                RefundAmount = 190m
            },
            new()
            {
                Id = ids.ReservationPayment5Id,
                ReservationId = ids.Reservation5Id,
                PaymentMethodId = ids.PaymentMethod2Id,
                Amount = 850m,
                Currency = "EUR",
                Status = PaymentStatus.Completed,
                ProviderTransactionId = "txn_mock_2005",
                PaymentIntentId = "pi_mock_2005",
                CreatedAt = now.AddDays(-9),
                PaidAt = now.AddDays(-8)
            }
        };

        var reviews = new List<Review>
        {
            new()
            {
                Id = ids.Review1Id,
                ReservationId = ids.Reservation1Id,
                ListingId = ids.ListingBeachActiveId,
                GuestId = ids.GuestOneUserId,
                Rating = 5,
                Text = "Cok temiz, ekip cok ilgiliydi. Tekrar gelecegiz.",
                Categories = new ReviewCategories { Cleanliness = 5, Facilities = 5, Service = 5 },
                HostResponseText = "Nazik geri bildiriminiz icin tesekkur ederiz.",
                HostResponseDate = now.AddDays(-18),
                CreatedAt = now.AddDays(-19),
                IsVerified = true
            },
            new()
            {
                Id = ids.Review2Id,
                ReservationId = ids.Reservation5Id,
                ListingId = ids.ListingYachtActiveId,
                GuestId = ids.GuestTwoUserId,
                Rating = 4,
                Text = "Rota ve servis cok iyiydi, organizasyon basariliydi.",
                Categories = new ReviewCategories { Cleanliness = 4, Facilities = 4, Service = 5 },
                HostResponseText = null,
                HostResponseDate = null,
                CreatedAt = now.AddDays(-2),
                IsVerified = true
            }
        };

        var favorites = new List<Favorite>
        {
            new() { Id = ids.Favorite1Id, UserId = ids.GuestOneUserId, VenueId = ids.ListingBeachActiveId, VenueType = VenueType.Beach, CreatedAt = now.AddDays(-10) },
            new() { Id = ids.Favorite2Id, UserId = ids.GuestOneUserId, VenueId = ids.ListingYachtActiveId, VenueType = VenueType.Yacht, CreatedAt = now.AddDays(-8) },
            new() { Id = ids.Favorite3Id, UserId = ids.GuestTwoUserId, VenueId = ids.ListingPoolActiveId, VenueType = VenueType.Pool, CreatedAt = now.AddDays(-6) }
        };

        var blogPosts = new List<BlogPost>
        {
            new()
            {
                Id = ids.BlogPost1Id,
                AuthorId = ids.AdminUserId,
                Slug = "mock-bodrum-en-iyi-plaj-rehberi",
                Title = new Dictionary<string, string>
                {
                    ["tr"] = "Bodrum'da En Iyi Plajlari Secme Rehberi",
                    ["en"] = "Guide to Choosing the Best Beaches in Bodrum"
                },
                Description = new Dictionary<string, string>
                {
                    ["tr"] = "Bodrum tatiliniz icin butceye ve beklentiye gore plaj secim tüyolari.",
                    ["en"] = "Beach selection tips for your Bodrum holiday by budget and expectations."
                },
                Content = new Dictionary<string, string>
                {
                    ["tr"] = "<p>Bodrum sahillerinde gunluk konfor, hizmet kalitesi ve lokasyon on plandadir...</p>",
                    ["en"] = "<p>When choosing beaches in Bodrum, daily comfort, service quality and location matter...</p>"
                },
                ImageUrl = "https://images.unsplash.com/photo-1507525428034-b723cf961d3e?auto=format&fit=crop&w=1200&q=80",
                Category = "travel",
                Tags = new List<string> { "bodrum", "beach", "guide" },
                ReadTime = 6,
                IsPublished = true,
                IsFeatured = true,
                PublishedAt = now.AddDays(-15),
                CreatedAt = now.AddDays(-16),
                UpdatedAt = now.AddDays(-15),
                ViewCount = 312
            },
            new()
            {
                Id = ids.BlogPost2Id,
                AuthorId = ids.AdminUserId,
                Slug = "mock-yat-turu-paket-karsilastirma",
                Title = new Dictionary<string, string>
                {
                    ["tr"] = "Yat Turu Paketleri Nasil Karsilastirilir?",
                    ["en"] = "How to Compare Yacht Tour Packages"
                },
                Description = new Dictionary<string, string>
                {
                    ["tr"] = "Rota, ekip, hizmet ve fiyat dengesine gore dogru paket secimi.",
                    ["en"] = "Picking the right package by balancing route, crew, services and price."
                },
                Content = new Dictionary<string, string>
                {
                    ["tr"] = "<p>Yat kiralama yaparken toplam maliyetin icinde hangi hizmetlerin olduguna dikkat edin...</p>",
                    ["en"] = "<p>When chartering a yacht, always inspect which services are included in total cost...</p>"
                },
                ImageUrl = "https://images.unsplash.com/photo-1567899378494-47b22a2ae96a?auto=format&fit=crop&w=1200&q=80",
                Category = "tips",
                Tags = new List<string> { "yacht", "charter", "pricing" },
                ReadTime = 5,
                IsPublished = true,
                IsFeatured = false,
                PublishedAt = now.AddDays(-7),
                CreatedAt = now.AddDays(-8),
                UpdatedAt = now.AddDays(-7),
                ViewCount = 178
            },
            new()
            {
                Id = ids.BlogPost3Id,
                AuthorId = ids.AdminUserId,
                Slug = "mock-antalya-gizli-koylar",
                Title = new Dictionary<string, string>
                {
                    ["tr"] = "Antalya Cevresindeki Gizli Koylar",
                    ["en"] = "Hidden Bays Around Antalya"
                },
                Description = new Dictionary<string, string>
                {
                    ["tr"] = "Sezon acilmadan once kesfedilecek sakin rotalar.",
                    ["en"] = "Quiet routes to explore before high season starts."
                },
                Content = new Dictionary<string, string>
                {
                    ["tr"] = "<p>Bu yazida kalabalik disinda kalmis koylari listeliyoruz...</p>",
                    ["en"] = "<p>In this article we list secluded bays away from crowds...</p>"
                },
                ImageUrl = "https://images.unsplash.com/photo-1528150177508-62a5f72e5678?auto=format&fit=crop&w=1200&q=80",
                Category = "destinations",
                Tags = new List<string> { "antalya", "daytrip" },
                ReadTime = 4,
                IsPublished = false,
                IsFeatured = false,
                PublishedAt = null,
                CreatedAt = now.AddDays(-2),
                UpdatedAt = now.AddDays(-1),
                ViewCount = 0
            }
        };

        var destinations = new List<Destination>
        {
            new()
            {
                Id = ids.DestinationBodrumId,
                Name = "Bodrum",
                Slug = "bodrum",
                Country = "Turkey",
                Description = "Turquoise bays, luxury beach clubs and yacht routes.",
                Subtitle = "Aegean premium destination",
                ImageUrl = "https://images.unsplash.com/photo-1507525428034-b723cf961d3e?auto=format&fit=crop&w=1200&q=80",
                MapImageUrl = "https://images.unsplash.com/photo-1524661135-423995f22d0b?auto=format&fit=crop&w=1200&q=80",
                Location = CreatePoint(37.0344m, 27.4305m),
                Latitude = 37.0344,
                Longitude = 27.4305,
                AvgWaterTemp = "24C",
                SunnyDaysPerYear = 290,
                AverageRating = 4.7,
                SpotCount = 54,
                Tags = new List<string> { "beach", "yacht", "nightlife" },
                Features = new List<DestinationFeature>
                {
                    new() { Icon = "sun", Title = "Long season", Description = "Warm weather from May to October" },
                    new() { Icon = "anchor", Title = "Marinas", Description = "Strong yacht infrastructure" }
                },
                IsFeatured = true,
                IsActive = true,
                CreatedAt = now.AddMonths(-10),
                UpdatedAt = now.AddDays(-3)
            },
            new()
            {
                Id = ids.DestinationIstanbulId,
                Name = "Istanbul",
                Slug = "istanbul",
                Country = "Turkey",
                Description = "Urban pools and Bosphorus yacht experiences.",
                Subtitle = "City meets water",
                ImageUrl = "https://images.unsplash.com/photo-1524231757912-21f4fe3a7200?auto=format&fit=crop&w=1200&q=80",
                MapImageUrl = "https://images.unsplash.com/photo-1505765050516-f72dcac9c60f?auto=format&fit=crop&w=1200&q=80",
                Location = CreatePoint(41.0082m, 28.9784m),
                Latitude = 41.0082,
                Longitude = 28.9784,
                AvgWaterTemp = "21C",
                SunnyDaysPerYear = 215,
                AverageRating = 4.4,
                SpotCount = 31,
                Tags = new List<string> { "pool", "city", "bosphorus" },
                Features = new List<DestinationFeature>
                {
                    new() { Icon = "city", Title = "Easy access", Description = "Fast transportation options" },
                    new() { Icon = "music", Title = "Events", Description = "Weekend parties and premium venues" }
                },
                IsFeatured = true,
                IsActive = true,
                CreatedAt = now.AddMonths(-9),
                UpdatedAt = now.AddDays(-5)
            },
            new()
            {
                Id = ids.DestinationAntalyaId,
                Name = "Antalya",
                Slug = "antalya",
                Country = "Turkey",
                Description = "Family-friendly day trips and warm waters.",
                Subtitle = "Mediterranean escape",
                ImageUrl = "https://images.unsplash.com/photo-1519046904884-53103b34b206?auto=format&fit=crop&w=1200&q=80",
                MapImageUrl = "https://images.unsplash.com/photo-1527838832700-5059252407fa?auto=format&fit=crop&w=1200&q=80",
                Location = CreatePoint(36.8969m, 30.7133m),
                Latitude = 36.8969,
                Longitude = 30.7133,
                AvgWaterTemp = "25C",
                SunnyDaysPerYear = 300,
                AverageRating = 4.6,
                SpotCount = 46,
                Tags = new List<string> { "day-trip", "family", "mediterranean" },
                Features = new List<DestinationFeature>
                {
                    new() { Icon = "family", Title = "Family routes", Description = "Safe and comfortable options" },
                    new() { Icon = "sea", Title = "Clear sea", Description = "Popular swimming stops" }
                },
                IsFeatured = false,
                IsActive = true,
                CreatedAt = now.AddMonths(-8),
                UpdatedAt = now.AddDays(-6)
            }
        };

        var newsletterSubscribers = new List<NewsletterSubscriber>
        {
            new() { Id = ids.Newsletter1Id, Email = "newsletter.user1@swimago.local", IsActive = true, SubscribedAt = now.AddDays(-20), UnsubscribedAt = null },
            new() { Id = ids.Newsletter2Id, Email = "newsletter.user2@swimago.local", IsActive = true, SubscribedAt = now.AddDays(-9), UnsubscribedAt = null }
        };

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            await context.Users.AddRangeAsync(users, cancellationToken);
            await context.UserProfiles.AddRangeAsync(profiles, cancellationToken);
            await context.Cities.AddRangeAsync(cities, cancellationToken);
            await context.Amenities.AddRangeAsync(amenities, cancellationToken);
            await context.Listings.AddRangeAsync(listings, cancellationToken);
            await context.ListingImages.AddRangeAsync(listingImages, cancellationToken);
            await context.ListingAmenities.AddRangeAsync(listingAmenities, cancellationToken);
            await context.DailyPricings.AddRangeAsync(dailyPricings, cancellationToken);
            await context.AvailabilityBlocks.AddRangeAsync(availabilityBlocks, cancellationToken);
            await context.HostBusinessSettings.AddRangeAsync(hostBusinessSettings, cancellationToken);
            await context.HostListingMetadata.AddRangeAsync(hostListingMetadata, cancellationToken);
            await context.PaymentMethods.AddRangeAsync(paymentMethods, cancellationToken);
            await context.Reservations.AddRangeAsync(reservations, cancellationToken);
            await context.ReservationPayments.AddRangeAsync(reservationPayments, cancellationToken);
            await context.Reviews.AddRangeAsync(reviews, cancellationToken);
            await context.Favorites.AddRangeAsync(favorites, cancellationToken);
            await context.BlogPosts.AddRangeAsync(blogPosts, cancellationToken);
            await context.Destinations.AddRangeAsync(destinations, cancellationToken);
            await context.NewsletterSubscribers.AddRangeAsync(newsletterSubscribers, cancellationToken);

            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            logger.LogInformation(
                "Mock seed completed successfully. Users: {UserCount}, Listings: {ListingCount}, Reservations: {ReservationCount}, Blogs: {BlogCount}",
                users.Count,
                listings.Count,
                reservations.Count,
                blogPosts.Count);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private static Point CreatePoint(decimal latitude, decimal longitude)
    {
        return new Point((double)longitude, (double)latitude) { SRID = 4326 };
    }

    private sealed class MockIds
    {
        public Guid AdminUserId { get; } = Guid.Parse("10000000-0000-0000-0000-000000000001");
        public Guid HostOneUserId { get; } = Guid.Parse("10000000-0000-0000-0000-000000000002");
        public Guid HostTwoUserId { get; } = Guid.Parse("10000000-0000-0000-0000-000000000003");
        public Guid GuestOneUserId { get; } = Guid.Parse("10000000-0000-0000-0000-000000000004");
        public Guid GuestTwoUserId { get; } = Guid.Parse("10000000-0000-0000-0000-000000000005");

        public Guid AdminProfileId { get; } = Guid.Parse("11000000-0000-0000-0000-000000000001");
        public Guid HostOneProfileId { get; } = Guid.Parse("11000000-0000-0000-0000-000000000002");
        public Guid HostTwoProfileId { get; } = Guid.Parse("11000000-0000-0000-0000-000000000003");
        public Guid GuestOneProfileId { get; } = Guid.Parse("11000000-0000-0000-0000-000000000004");
        public Guid GuestTwoProfileId { get; } = Guid.Parse("11000000-0000-0000-0000-000000000005");

        public Guid CityIstanbulId { get; } = Guid.Parse("20000000-0000-0000-0000-000000000001");
        public Guid CityBodrumId { get; } = Guid.Parse("20000000-0000-0000-0000-000000000002");
        public Guid CityAntalyaId { get; } = Guid.Parse("20000000-0000-0000-0000-000000000003");

        public Guid AmenityWifiId { get; } = Guid.Parse("30000000-0000-0000-0000-000000000001");
        public Guid AmenityParkingId { get; } = Guid.Parse("30000000-0000-0000-0000-000000000002");
        public Guid AmenityShowerId { get; } = Guid.Parse("30000000-0000-0000-0000-000000000003");
        public Guid AmenityFoodId { get; } = Guid.Parse("30000000-0000-0000-0000-000000000004");
        public Guid AmenityMusicId { get; } = Guid.Parse("30000000-0000-0000-0000-000000000005");
        public Guid AmenityCaptainId { get; } = Guid.Parse("30000000-0000-0000-0000-000000000006");
        public Guid AmenityLifeJacketId { get; } = Guid.Parse("30000000-0000-0000-0000-000000000007");

        public Guid ListingBeachActiveId { get; } = Guid.Parse("40000000-0000-0000-0000-000000000001");
        public Guid ListingPoolActiveId { get; } = Guid.Parse("40000000-0000-0000-0000-000000000002");
        public Guid ListingYachtActiveId { get; } = Guid.Parse("40000000-0000-0000-0000-000000000003");
        public Guid ListingDayTripActiveId { get; } = Guid.Parse("40000000-0000-0000-0000-000000000004");
        public Guid ListingBeachPendingId { get; } = Guid.Parse("40000000-0000-0000-0000-000000000005");
        public Guid ListingPoolInactiveId { get; } = Guid.Parse("40000000-0000-0000-0000-000000000006");

        public Guid ListingImage1Id { get; } = Guid.Parse("50000000-0000-0000-0000-000000000001");
        public Guid ListingImage2Id { get; } = Guid.Parse("50000000-0000-0000-0000-000000000002");
        public Guid ListingImage3Id { get; } = Guid.Parse("50000000-0000-0000-0000-000000000003");
        public Guid ListingImage4Id { get; } = Guid.Parse("50000000-0000-0000-0000-000000000004");
        public Guid ListingImage5Id { get; } = Guid.Parse("50000000-0000-0000-0000-000000000005");
        public Guid ListingImage6Id { get; } = Guid.Parse("50000000-0000-0000-0000-000000000006");
        public Guid ListingImage7Id { get; } = Guid.Parse("50000000-0000-0000-0000-000000000007");
        public Guid ListingImage8Id { get; } = Guid.Parse("50000000-0000-0000-0000-000000000008");
        public Guid ListingImage9Id { get; } = Guid.Parse("50000000-0000-0000-0000-000000000009");
        public Guid ListingImage10Id { get; } = Guid.Parse("50000000-0000-0000-0000-000000000010");

        public Guid DailyPricing1Id { get; } = Guid.Parse("60000000-0000-0000-0000-000000000001");
        public Guid DailyPricing2Id { get; } = Guid.Parse("60000000-0000-0000-0000-000000000002");
        public Guid DailyPricing3Id { get; } = Guid.Parse("60000000-0000-0000-0000-000000000003");
        public Guid DailyPricing4Id { get; } = Guid.Parse("60000000-0000-0000-0000-000000000004");
        public Guid DailyPricing5Id { get; } = Guid.Parse("60000000-0000-0000-0000-000000000005");
        public Guid DailyPricing6Id { get; } = Guid.Parse("60000000-0000-0000-0000-000000000006");
        public Guid DailyPricing7Id { get; } = Guid.Parse("60000000-0000-0000-0000-000000000007");
        public Guid DailyPricing8Id { get; } = Guid.Parse("60000000-0000-0000-0000-000000000008");
        public Guid DailyPricing9Id { get; } = Guid.Parse("60000000-0000-0000-0000-000000000009");

        public Guid AvailabilityBlock1Id { get; } = Guid.Parse("61000000-0000-0000-0000-000000000001");
        public Guid AvailabilityBlock2Id { get; } = Guid.Parse("61000000-0000-0000-0000-000000000002");

        public Guid HostBusinessSettings1Id { get; } = Guid.Parse("70000000-0000-0000-0000-000000000001");
        public Guid HostBusinessSettings2Id { get; } = Guid.Parse("70000000-0000-0000-0000-000000000002");

        public Guid HostListingMetadata1Id { get; } = Guid.Parse("71000000-0000-0000-0000-000000000001");
        public Guid HostListingMetadata2Id { get; } = Guid.Parse("71000000-0000-0000-0000-000000000002");
        public Guid HostListingMetadata3Id { get; } = Guid.Parse("71000000-0000-0000-0000-000000000003");
        public Guid HostListingMetadata4Id { get; } = Guid.Parse("71000000-0000-0000-0000-000000000004");

        public Guid PaymentMethod1Id { get; } = Guid.Parse("80000000-0000-0000-0000-000000000001");
        public Guid PaymentMethod2Id { get; } = Guid.Parse("80000000-0000-0000-0000-000000000002");

        public Guid Reservation1Id { get; } = Guid.Parse("90000000-0000-0000-0000-000000000001");
        public Guid Reservation2Id { get; } = Guid.Parse("90000000-0000-0000-0000-000000000002");
        public Guid Reservation3Id { get; } = Guid.Parse("90000000-0000-0000-0000-000000000003");
        public Guid Reservation4Id { get; } = Guid.Parse("90000000-0000-0000-0000-000000000004");
        public Guid Reservation5Id { get; } = Guid.Parse("90000000-0000-0000-0000-000000000005");

        public Guid ReservationPayment1Id { get; } = Guid.Parse("91000000-0000-0000-0000-000000000001");
        public Guid ReservationPayment2Id { get; } = Guid.Parse("91000000-0000-0000-0000-000000000002");
        public Guid ReservationPayment3Id { get; } = Guid.Parse("91000000-0000-0000-0000-000000000003");
        public Guid ReservationPayment4Id { get; } = Guid.Parse("91000000-0000-0000-0000-000000000004");
        public Guid ReservationPayment5Id { get; } = Guid.Parse("91000000-0000-0000-0000-000000000005");

        public Guid Review1Id { get; } = Guid.Parse("92000000-0000-0000-0000-000000000001");
        public Guid Review2Id { get; } = Guid.Parse("92000000-0000-0000-0000-000000000002");

        public Guid Favorite1Id { get; } = Guid.Parse("93000000-0000-0000-0000-000000000001");
        public Guid Favorite2Id { get; } = Guid.Parse("93000000-0000-0000-0000-000000000002");
        public Guid Favorite3Id { get; } = Guid.Parse("93000000-0000-0000-0000-000000000003");

        public Guid BlogPost1Id { get; } = Guid.Parse("94000000-0000-0000-0000-000000000001");
        public Guid BlogPost2Id { get; } = Guid.Parse("94000000-0000-0000-0000-000000000002");
        public Guid BlogPost3Id { get; } = Guid.Parse("94000000-0000-0000-0000-000000000003");

        public Guid DestinationBodrumId { get; } = Guid.Parse("95000000-0000-0000-0000-000000000001");
        public Guid DestinationIstanbulId { get; } = Guid.Parse("95000000-0000-0000-0000-000000000002");
        public Guid DestinationAntalyaId { get; } = Guid.Parse("95000000-0000-0000-0000-000000000003");

        public Guid Newsletter1Id { get; } = Guid.Parse("96000000-0000-0000-0000-000000000001");
        public Guid Newsletter2Id { get; } = Guid.Parse("96000000-0000-0000-0000-000000000002");
    }
}
