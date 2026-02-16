using Microsoft.EntityFrameworkCore;
using Swimago.Domain.Entities;
using Swimago.Domain.Enums;
using System.Reflection;
using System.Text.Json;

namespace Swimago.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // DbSets
    public DbSet<User> Users { get; set; }
    public DbSet<UserProfile> UserProfiles { get; set; }
    public DbSet<Listing> Listings { get; set; }
    public DbSet<ListingImage> ListingImages { get; set; }
    public DbSet<DailyPricing> DailyPricings { get; set; }
    public DbSet<AvailabilityBlock> AvailabilityBlocks { get; set; }
    public DbSet<Amenity> Amenities { get; set; }
    public DbSet<ListingAmenity> ListingAmenities { get; set; }
    public DbSet<Reservation> Reservations { get; set; }
    public DbSet<ReservationPayment> ReservationPayments { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<BlogPost> BlogPosts { get; set; }
    public DbSet<Favorite> Favorites { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<PaymentMethod> PaymentMethods { get; set; }
    public DbSet<NewsletterSubscriber> NewsletterSubscribers { get; set; }
    public DbSet<City> Cities { get; set; }
    public DbSet<Destination> Destinations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Enable PostGIS extension
        modelBuilder.HasPostgresExtension("postgis");

        // Configure entity relationships and keys
        ConfigureEntityRelationships(modelBuilder);

        // Configure JSONB for multi-language fields and complex objects
        ConfigureJsonbProperties(modelBuilder);

        // Configure PostGIS geometry
        ConfigurePostGIS(modelBuilder);

        // Apply entity configurations from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    private void ConfigureEntityRelationships(ModelBuilder modelBuilder)
    {
        // ListingAmenity - Composite Primary Key (Many-to-Many)
        modelBuilder.Entity<ListingAmenity>()
            .HasKey(la => new { la.ListingId, la.AmenityId });

        modelBuilder.Entity<ListingAmenity>()
            .HasOne(la => la.Listing)
            .WithMany(l => l.Amenities)
            .HasForeignKey(la => la.ListingId);

        modelBuilder.Entity<ListingAmenity>()
            .HasOne(la => la.Amenity)
            .WithMany(a => a.ListingAmenities)
            .HasForeignKey(la => la.AmenityId);

        // User - UserProfile (One-to-One)
        modelBuilder.Entity<UserProfile>()
            .HasOne(p => p.User)
            .WithOne(u => u.Profile)
            .HasForeignKey<UserProfile>(p => p.UserId);

        // User - PaymentMethods (One-to-Many)
        modelBuilder.Entity<PaymentMethod>()
            .HasOne(pm => pm.User)
            .WithMany(u => u.PaymentMethods)
            .HasForeignKey(pm => pm.UserId);

        // Listing - Indexes
        modelBuilder.Entity<Listing>()
            .HasIndex(l => l.HostId);

        modelBuilder.Entity<Listing>()
            .HasIndex(l => l.Type);

        modelBuilder.Entity<Listing>()
            .HasIndex(l => l.Status);

        modelBuilder.Entity<Listing>()
            .HasIndex(l => l.Slug)
            .IsUnique();

        modelBuilder.Entity<Listing>()
            .HasIndex(l => l.City);

        modelBuilder.Entity<Listing>()
            .HasIndex(l => l.IsFeatured);

        // Reservation - Indexes
        modelBuilder.Entity<Reservation>()
            .HasIndex(r => r.GuestId);

        modelBuilder.Entity<Reservation>()
            .HasIndex(r => r.ListingId);

        modelBuilder.Entity<Reservation>()
            .HasIndex(r => r.Status);

        modelBuilder.Entity<Reservation>()
            .HasIndex(r => r.StartTime);

        modelBuilder.Entity<Reservation>()
            .HasIndex(r => r.ConfirmationNumber)
            .IsUnique();

        // DailyPricing - Composite Index
        modelBuilder.Entity<DailyPricing>()
            .HasIndex(dp => new { dp.ListingId, dp.Date });

        // Review - One-to-One with Reservation
        modelBuilder.Entity<Review>()
            .HasOne(r => r.Reservation)
            .WithOne(res => res.Review)
            .HasForeignKey<Review>(r => r.ReservationId);

        // ReservationPayment - One-to-One with Reservation
        modelBuilder.Entity<ReservationPayment>()
            .HasOne(rp => rp.Reservation)
            .WithOne(r => r.Payment)
            .HasForeignKey<ReservationPayment>(rp => rp.ReservationId);

        // ReservationPayment - PaymentMethod relationship
        modelBuilder.Entity<ReservationPayment>()
            .HasOne(rp => rp.PaymentMethod)
            .WithMany()
            .HasForeignKey(rp => rp.PaymentMethodId)
            .IsRequired(false);

        // Favorite - Composite Index
        modelBuilder.Entity<Favorite>()
            .HasIndex(f => new { f.UserId, f.VenueId, f.VenueType })
            .IsUnique();

        // Favorite - Listing relationship (optional, only when VenueType matches)
        modelBuilder.Entity<Favorite>()
            .HasOne(f => f.Listing)
            .WithMany(l => l.Favorites)
            .HasForeignKey(f => f.VenueId)
            .IsRequired(false);

        // BlogPost - Indexes
        modelBuilder.Entity<BlogPost>()
            .HasIndex(b => b.Slug)
            .IsUnique();

        modelBuilder.Entity<BlogPost>()
            .HasIndex(b => b.IsPublished);

        modelBuilder.Entity<BlogPost>()
            .HasIndex(b => b.IsFeatured);

        // NewsletterSubscriber - Email unique index
        modelBuilder.Entity<NewsletterSubscriber>()
            .HasIndex(n => n.Email)
            .IsUnique();

        // City - Index
        modelBuilder.Entity<City>()
            .HasIndex(c => c.Country);
    }

    private void ConfigureJsonbProperties(ModelBuilder modelBuilder)
    {
        var jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        // User settings
        modelBuilder.Entity<User>()
            .Property(u => u.NotificationSettings)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, jsonSerializerOptions),
                v => JsonSerializer.Deserialize<NotificationSettings>(v, jsonSerializerOptions) ?? new NotificationSettings()
            );

        modelBuilder.Entity<User>()
            .Property(u => u.LanguageSettings)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, jsonSerializerOptions),
                v => JsonSerializer.Deserialize<LanguageSettings>(v, jsonSerializerOptions) ?? new LanguageSettings()
            );

        modelBuilder.Entity<User>()
            .Property(u => u.PrivacySettings)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, jsonSerializerOptions),
                v => JsonSerializer.Deserialize<PrivacySettings>(v, jsonSerializerOptions) ?? new PrivacySettings()
            );

        // UserProfile
        modelBuilder.Entity<UserProfile>()
            .Property(p => p.FirstName)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, jsonSerializerOptions),
                v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, jsonSerializerOptions) ?? new Dictionary<string, string>()
            );

        modelBuilder.Entity<UserProfile>()
            .Property(p => p.LastName)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, jsonSerializerOptions),
                v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, jsonSerializerOptions) ?? new Dictionary<string, string>()
            );

        modelBuilder.Entity<UserProfile>()
            .Property(p => p.Bio)
            .HasColumnType("jsonb")
            .HasConversion(
                v => v == null ? null : JsonSerializer.Serialize(v, jsonSerializerOptions),
                v => v == null ? null : JsonSerializer.Deserialize<Dictionary<string, string>>(v, jsonSerializerOptions)
            );

        // Listing
        modelBuilder.Entity<Listing>()
            .Property(l => l.Title)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, jsonSerializerOptions),
                v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, jsonSerializerOptions) ?? new Dictionary<string, string>()
            );

        modelBuilder.Entity<Listing>()
            .Property(l => l.Description)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, jsonSerializerOptions),
                v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, jsonSerializerOptions) ?? new Dictionary<string, string>()
            );

        modelBuilder.Entity<Listing>()
            .Property(l => l.Address)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, jsonSerializerOptions),
                v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, jsonSerializerOptions) ?? new Dictionary<string, string>()
            );

        modelBuilder.Entity<Listing>()
            .Property(l => l.Conditions)
            .HasColumnType("jsonb")
            .HasConversion(
                v => v == null ? null : JsonSerializer.Serialize(v, jsonSerializerOptions),
                v => v == null ? null : JsonSerializer.Deserialize<List<ListingCondition>>(v, jsonSerializerOptions)
            );

        // ListingImage - removed AltText JSONB, now simple string

        // Amenity
        modelBuilder.Entity<Amenity>()
            .Property(a => a.Label)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, jsonSerializerOptions),
                v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, jsonSerializerOptions) ?? new Dictionary<string, string>()
            );

        modelBuilder.Entity<Amenity>()
            .Property(a => a.ApplicableTo)
            .HasColumnType("jsonb")
            .HasConversion(
                v => v == null ? null : JsonSerializer.Serialize(v, jsonSerializerOptions),
                v => v == null ? null : JsonSerializer.Deserialize<List<ListingType>>(v, jsonSerializerOptions)
            );

        // Reservation
        modelBuilder.Entity<Reservation>()
            .Property(r => r.SpecialRequests)
            .HasColumnType("jsonb")
            .HasConversion(
                v => v == null ? null : JsonSerializer.Serialize(v, jsonSerializerOptions),
                v => v == null ? null : JsonSerializer.Deserialize<Dictionary<string, string>>(v, jsonSerializerOptions)
            );

        modelBuilder.Entity<Reservation>()
            .Property(r => r.Guests)
            .HasColumnType("jsonb")
            .HasConversion(
                v => v == null ? null : JsonSerializer.Serialize(v, jsonSerializerOptions),
                v => v == null ? null : JsonSerializer.Deserialize<GuestDetails>(v, jsonSerializerOptions)
            );

        modelBuilder.Entity<Reservation>()
            .Property(r => r.Selections)
            .HasColumnType("jsonb")
            .HasConversion(
                v => v == null ? null : JsonSerializer.Serialize(v, jsonSerializerOptions),
                v => v == null ? null : JsonSerializer.Deserialize<ReservationSelections>(v, jsonSerializerOptions)
            );

        modelBuilder.Entity<Reservation>()
            .Property(r => r.PriceBreakdown)
            .HasColumnType("jsonb")
            .HasConversion(
                v => v == null ? null : JsonSerializer.Serialize(v, jsonSerializerOptions),
                v => v == null ? null : JsonSerializer.Deserialize<List<PriceBreakdownItem>>(v, jsonSerializerOptions)
            );

        // Review categories
        modelBuilder.Entity<Review>()
            .Property(r => r.Categories)
            .HasColumnType("jsonb")
            .HasConversion(
                v => v == null ? null : JsonSerializer.Serialize(v, jsonSerializerOptions),
                v => v == null ? null : JsonSerializer.Deserialize<ReviewCategories>(v, jsonSerializerOptions)
            );

        // BlogPost
        modelBuilder.Entity<BlogPost>()
            .Property(b => b.Title)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, jsonSerializerOptions),
                v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, jsonSerializerOptions) ?? new Dictionary<string, string>()
            );

        modelBuilder.Entity<BlogPost>()
            .Property(b => b.Description)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, jsonSerializerOptions),
                v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, jsonSerializerOptions) ?? new Dictionary<string, string>()
            );

        modelBuilder.Entity<BlogPost>()
            .Property(b => b.Content)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, jsonSerializerOptions),
                v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, jsonSerializerOptions) ?? new Dictionary<string, string>()
            );

        modelBuilder.Entity<BlogPost>()
            .Property(b => b.Tags)
            .HasColumnType("jsonb")
            .HasConversion(
                v => v == null ? null : JsonSerializer.Serialize(v, jsonSerializerOptions),
                v => v == null ? null : JsonSerializer.Deserialize<List<string>>(v, jsonSerializerOptions)
            );

        // City
        modelBuilder.Entity<City>()
            .Property(c => c.Name)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, jsonSerializerOptions),
                v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, jsonSerializerOptions) ?? new Dictionary<string, string>()
            );

        modelBuilder.Entity<Listing>()
            .Property(l => l.Details)
            .HasColumnType("jsonb"); // No conversion needed for raw JSON string

        // Destination
        modelBuilder.Entity<Destination>()
            .Property(d => d.Tags)
            .HasColumnType("jsonb")
            .HasConversion(
                v => v == null ? null : JsonSerializer.Serialize(v, jsonSerializerOptions),
                v => v == null ? null : JsonSerializer.Deserialize<List<string>>(v, jsonSerializerOptions)
            );

        modelBuilder.Entity<Destination>()
            .Property(d => d.Features)
            .HasColumnType("jsonb")
            .HasConversion(
                v => v == null ? null : JsonSerializer.Serialize(v, jsonSerializerOptions),
                v => v == null ? null : JsonSerializer.Deserialize<List<DestinationFeature>>(v, jsonSerializerOptions)
            );

        // Create GIN indexes for JSONB fields for better query performance
        modelBuilder.Entity<Listing>()
            .HasIndex(l => l.Title)
            .HasMethod("gin");

        modelBuilder.Entity<Listing>()
            .HasIndex(l => l.Description)
            .HasMethod("gin");

        modelBuilder.Entity<BlogPost>()
            .HasIndex(b => b.Title)
            .HasMethod("gin");
    }

    private void ConfigurePostGIS(ModelBuilder modelBuilder)
    {
        // Configure PostGIS for Listing.Location
        modelBuilder.Entity<Listing>()
            .Property(l => l.Location)
            .HasColumnType("geography (point)");

        // Create spatial index for better performance
        modelBuilder.Entity<Listing>()
            .HasIndex(l => l.Location)
            .HasMethod("gist");

        // Configure PostGIS for Destination.Location
        modelBuilder.Entity<Destination>()
            .Property(d => d.Location)
            .HasColumnType("geography (point)");

        modelBuilder.Entity<Destination>()
            .HasIndex(d => d.Location)
            .HasMethod("gist");
    }
}
