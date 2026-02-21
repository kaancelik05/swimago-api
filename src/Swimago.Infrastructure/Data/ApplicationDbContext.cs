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
    public DbSet<BlogComment> BlogComments { get; set; }
    public DbSet<Favorite> Favorites { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<PaymentMethod> PaymentMethods { get; set; }
    public DbSet<NewsletterSubscriber> NewsletterSubscribers { get; set; }
    public DbSet<City> Cities { get; set; }
    public DbSet<Destination> Destinations { get; set; }
    public DbSet<HostBusinessSettings> HostBusinessSettings { get; set; }
    public DbSet<HostListingMetadata> HostListingMetadata { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Enable PostGIS extension
        modelBuilder.HasPostgresExtension("postgis");

        // Apply entity configurations from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
