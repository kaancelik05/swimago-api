using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Swimago.Domain.Entities;
using Swimago.Domain.Enums;
using System.Text.Json;

namespace Swimago.Infrastructure.Data.Configurations;

public static class ConfigurationHelper
{
    public static readonly JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
}

public class ListingAmenityConfiguration : IEntityTypeConfiguration<ListingAmenity>
{
    public void Configure(EntityTypeBuilder<ListingAmenity> builder)
    {
        builder.HasKey(la => new { la.ListingId, la.AmenityId });
        builder.HasOne(la => la.Listing).WithMany(l => l.Amenities).HasForeignKey(la => la.ListingId);
        builder.HasOne(la => la.Amenity).WithMany(a => a.ListingAmenities).HasForeignKey(la => la.AmenityId);
    }
}

public class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.HasOne(p => p.User).WithOne(u => u.Profile).HasForeignKey<UserProfile>(p => p.UserId);

        builder.Property(p => p.FirstName)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, ConfigurationHelper.JsonSerializerOptions),
                v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, ConfigurationHelper.JsonSerializerOptions) ?? new Dictionary<string, string>()
            );

        builder.Property(p => p.LastName)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, ConfigurationHelper.JsonSerializerOptions),
                v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, ConfigurationHelper.JsonSerializerOptions) ?? new Dictionary<string, string>()
            );

        builder.Property(p => p.Bio)
            .HasColumnType("jsonb")
            .HasConversion(
                v => v == null ? null : JsonSerializer.Serialize(v, ConfigurationHelper.JsonSerializerOptions),
                v => v == null ? null : JsonSerializer.Deserialize<Dictionary<string, string>>(v, ConfigurationHelper.JsonSerializerOptions)
            );
    }
}

public class PaymentMethodConfiguration : IEntityTypeConfiguration<PaymentMethod>
{
    public void Configure(EntityTypeBuilder<PaymentMethod> builder)
    {
        builder.HasOne(pm => pm.User).WithMany(u => u.PaymentMethods).HasForeignKey(pm => pm.UserId);
    }
}

public class ListingConfiguration : IEntityTypeConfiguration<Listing>
{
    public void Configure(EntityTypeBuilder<Listing> builder)
    {
        builder.HasQueryFilter(l => !l.IsDeleted);

        builder.HasIndex(l => l.HostId);
        builder.HasIndex(l => l.Type);
        builder.HasIndex(l => l.Status);
        builder.HasIndex(l => l.Slug).IsUnique();
        builder.HasIndex(l => l.City);
        builder.HasIndex(l => l.IsFeatured);

        builder.Property(l => l.Title)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, ConfigurationHelper.JsonSerializerOptions),
                v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, ConfigurationHelper.JsonSerializerOptions) ?? new Dictionary<string, string>()
            );

        builder.Property(l => l.Description)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, ConfigurationHelper.JsonSerializerOptions),
                v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, ConfigurationHelper.JsonSerializerOptions) ?? new Dictionary<string, string>()
            );

        builder.Property(l => l.Address)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, ConfigurationHelper.JsonSerializerOptions),
                v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, ConfigurationHelper.JsonSerializerOptions) ?? new Dictionary<string, string>()
            );

        builder.Property(l => l.Conditions)
            .HasColumnType("jsonb")
            .HasConversion(
                v => v == null ? null : JsonSerializer.Serialize(v, ConfigurationHelper.JsonSerializerOptions),
                v => v == null ? null : JsonSerializer.Deserialize<List<ListingCondition>>(v, ConfigurationHelper.JsonSerializerOptions)
            );

        builder.Property(l => l.Details).HasColumnType("jsonb");

        builder.HasIndex(l => l.Title).HasMethod("gin");
        builder.HasIndex(l => l.Description).HasMethod("gin");

        builder.Property(l => l.Location).HasColumnType("geography (point)");
        builder.HasIndex(l => l.Location).HasMethod("gist");
    }
}

public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.HasQueryFilter(r => !r.IsDeleted);

        builder.HasIndex(r => r.GuestId);
        builder.HasIndex(r => r.ListingId);
        builder.HasIndex(r => r.Status);
        builder.HasIndex(r => r.StartTime);
        builder.HasIndex(r => r.ConfirmationNumber).IsUnique();
        builder.HasIndex(r => r.Source);

        builder.Property(r => r.SpecialRequests)
            .HasColumnType("jsonb")
            .HasConversion(
                v => v == null ? null : JsonSerializer.Serialize(v, ConfigurationHelper.JsonSerializerOptions),
                v => v == null ? null : JsonSerializer.Deserialize<Dictionary<string, string>>(v, ConfigurationHelper.JsonSerializerOptions)
            );

        builder.Property(r => r.Guests)
            .HasColumnType("jsonb")
            .HasConversion(
                v => v == null ? null : JsonSerializer.Serialize(v, ConfigurationHelper.JsonSerializerOptions),
                v => v == null ? null : JsonSerializer.Deserialize<GuestDetails>(v, ConfigurationHelper.JsonSerializerOptions)
            );

        builder.Property(r => r.Selections)
            .HasColumnType("jsonb")
            .HasConversion(
                v => v == null ? null : JsonSerializer.Serialize(v, ConfigurationHelper.JsonSerializerOptions),
                v => v == null ? null : JsonSerializer.Deserialize<ReservationSelections>(v, ConfigurationHelper.JsonSerializerOptions)
            );

        builder.Property(r => r.PriceBreakdown)
            .HasColumnType("jsonb")
            .HasConversion(
                v => v == null ? null : JsonSerializer.Serialize(v, ConfigurationHelper.JsonSerializerOptions),
                v => v == null ? null : JsonSerializer.Deserialize<List<PriceBreakdownItem>>(v, ConfigurationHelper.JsonSerializerOptions)
            );
    }
}

public class DailyPricingConfiguration : IEntityTypeConfiguration<DailyPricing>
{
    public void Configure(EntityTypeBuilder<DailyPricing> builder)
    {
        builder.HasIndex(dp => new { dp.ListingId, dp.Date });
    }
}

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.HasOne(r => r.Reservation).WithOne(res => res.Review).HasForeignKey<Review>(r => r.ReservationId);

        builder.Property(r => r.Categories)
            .HasColumnType("jsonb")
            .HasConversion(
                v => v == null ? null : JsonSerializer.Serialize(v, ConfigurationHelper.JsonSerializerOptions),
                v => v == null ? null : JsonSerializer.Deserialize<ReviewCategories>(v, ConfigurationHelper.JsonSerializerOptions)
            );
    }
}

public class ReservationPaymentConfiguration : IEntityTypeConfiguration<ReservationPayment>
{
    public void Configure(EntityTypeBuilder<ReservationPayment> builder)
    {
        builder.HasOne(rp => rp.Reservation).WithOne(r => r.Payment).HasForeignKey<ReservationPayment>(rp => rp.ReservationId);
        builder.HasOne(rp => rp.PaymentMethod).WithMany().HasForeignKey(rp => rp.PaymentMethodId).IsRequired(false);
    }
}

public class FavoriteConfiguration : IEntityTypeConfiguration<Favorite>
{
    public void Configure(EntityTypeBuilder<Favorite> builder)
    {
        builder.HasIndex(f => new { f.UserId, f.VenueId, f.VenueType }).IsUnique();
        builder.HasOne(f => f.Listing).WithMany(l => l.Favorites).HasForeignKey(f => f.VenueId).IsRequired(false);
    }
}

public class BlogPostConfiguration : IEntityTypeConfiguration<BlogPost>
{
    public void Configure(EntityTypeBuilder<BlogPost> builder)
    {
        builder.HasIndex(b => b.Slug).IsUnique();
        builder.HasIndex(b => b.IsPublished);
        builder.HasIndex(b => b.IsFeatured);

        builder.Property(b => b.Title)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, ConfigurationHelper.JsonSerializerOptions),
                v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, ConfigurationHelper.JsonSerializerOptions) ?? new Dictionary<string, string>()
            );

        builder.Property(b => b.Description)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, ConfigurationHelper.JsonSerializerOptions),
                v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, ConfigurationHelper.JsonSerializerOptions) ?? new Dictionary<string, string>()
            );

        builder.Property(b => b.Content)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, ConfigurationHelper.JsonSerializerOptions),
                v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, ConfigurationHelper.JsonSerializerOptions) ?? new Dictionary<string, string>()
            );

        builder.Property(b => b.Tags)
            .HasColumnType("jsonb")
            .HasConversion(
                v => v == null ? null : JsonSerializer.Serialize(v, ConfigurationHelper.JsonSerializerOptions),
                v => v == null ? null : JsonSerializer.Deserialize<List<string>>(v, ConfigurationHelper.JsonSerializerOptions)
            );

        builder.HasIndex(b => b.Title).HasMethod("gin");
    }
}

public class BlogCommentConfiguration : IEntityTypeConfiguration<BlogComment>
{
    public void Configure(EntityTypeBuilder<BlogComment> builder)
    {
        builder.HasOne(x => x.BlogPost).WithMany(x => x.Comments).HasForeignKey(x => x.BlogPostId);
        builder.HasOne(x => x.User).WithMany(x => x.BlogComments).HasForeignKey(x => x.UserId);
        builder.HasIndex(x => x.BlogPostId);
        builder.HasIndex(x => x.CreatedAt);
    }
}

public class NewsletterSubscriberConfiguration : IEntityTypeConfiguration<NewsletterSubscriber>
{
    public void Configure(EntityTypeBuilder<NewsletterSubscriber> builder)
    {
        builder.HasIndex(n => n.Email).IsUnique();
    }
}

public class CityConfiguration : IEntityTypeConfiguration<City>
{
    public void Configure(EntityTypeBuilder<City> builder)
    {
        builder.HasIndex(c => c.Country);
        builder.Property(c => c.Name)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, ConfigurationHelper.JsonSerializerOptions),
                v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, ConfigurationHelper.JsonSerializerOptions) ?? new Dictionary<string, string>()
            );
    }
}

public class HostBusinessSettingsConfiguration : IEntityTypeConfiguration<HostBusinessSettings>
{
    public void Configure(EntityTypeBuilder<HostBusinessSettings> builder)
    {
        builder.HasIndex(x => x.HostId).IsUnique();
        builder.HasOne(x => x.Host).WithOne(u => u.HostBusinessSettings).HasForeignKey<HostBusinessSettings>(x => x.HostId);
    }
}

public class HostListingMetadataConfiguration : IEntityTypeConfiguration<HostListingMetadata>
{
    public void Configure(EntityTypeBuilder<HostListingMetadata> builder)
    {
        builder.HasIndex(x => x.ListingId).IsUnique();
        builder.HasOne(x => x.Listing).WithOne(l => l.HostMetadata).HasForeignKey<HostListingMetadata>(x => x.ListingId);

        builder.Property(x => x.Highlights)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, ConfigurationHelper.JsonSerializerOptions),
                v => JsonSerializer.Deserialize<List<string>>(v, ConfigurationHelper.JsonSerializerOptions) ?? new List<string>()
            );

        builder.Property(x => x.SeatingAreas)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, ConfigurationHelper.JsonSerializerOptions),
                v => JsonSerializer.Deserialize<List<HostSeatingArea>>(v, ConfigurationHelper.JsonSerializerOptions) ?? new List<HostSeatingArea>()
            );
    }
}

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(u => u.NotificationSettings)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, ConfigurationHelper.JsonSerializerOptions),
                v => JsonSerializer.Deserialize<NotificationSettings>(v, ConfigurationHelper.JsonSerializerOptions) ?? new NotificationSettings()
            );

        builder.Property(u => u.LanguageSettings)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, ConfigurationHelper.JsonSerializerOptions),
                v => JsonSerializer.Deserialize<LanguageSettings>(v, ConfigurationHelper.JsonSerializerOptions) ?? new LanguageSettings()
            );

        builder.Property(u => u.PrivacySettings)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, ConfigurationHelper.JsonSerializerOptions),
                v => JsonSerializer.Deserialize<PrivacySettings>(v, ConfigurationHelper.JsonSerializerOptions) ?? new PrivacySettings()
            );
    }
}

public class AmenityConfiguration : IEntityTypeConfiguration<Amenity>
{
    public void Configure(EntityTypeBuilder<Amenity> builder)
    {
        builder.Property(a => a.Label)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, ConfigurationHelper.JsonSerializerOptions),
                v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, ConfigurationHelper.JsonSerializerOptions) ?? new Dictionary<string, string>()
            );

        builder.Property(a => a.ApplicableTo)
            .HasColumnType("jsonb")
            .HasConversion(
                v => v == null ? null : JsonSerializer.Serialize(v, ConfigurationHelper.JsonSerializerOptions),
                v => v == null ? null : JsonSerializer.Deserialize<List<ListingType>>(v, ConfigurationHelper.JsonSerializerOptions)
            );
    }
}

public class DestinationConfiguration : IEntityTypeConfiguration<Destination>
{
    public void Configure(EntityTypeBuilder<Destination> builder)
    {
        builder.Property(d => d.Tags)
            .HasColumnType("jsonb")
            .HasConversion(
                v => v == null ? null : JsonSerializer.Serialize(v, ConfigurationHelper.JsonSerializerOptions),
                v => v == null ? null : JsonSerializer.Deserialize<List<string>>(v, ConfigurationHelper.JsonSerializerOptions)
            );

        builder.Property(d => d.Features)
            .HasColumnType("jsonb")
            .HasConversion(
                v => v == null ? null : JsonSerializer.Serialize(v, ConfigurationHelper.JsonSerializerOptions),
                v => v == null ? null : JsonSerializer.Deserialize<List<DestinationFeature>>(v, ConfigurationHelper.JsonSerializerOptions)
            );

        builder.Property(d => d.Location).HasColumnType("geography (point)");
        builder.HasIndex(d => d.Location).HasMethod("gist");
    }
}
