using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swimago.Application.Interfaces;
using Swimago.Domain.Interfaces;
using Swimago.Infrastructure.Data;
using Swimago.Infrastructure.Repositories;
using Swimago.Infrastructure.Services;
using Swimago.Infrastructure.Services.Admin;

namespace Swimago.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Add DbContext
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsqlOptions =>
                {
                    npgsqlOptions.UseNetTopologySuite();
                    npgsqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                });
        });

        // Register Repositories
        services.AddMemoryCache();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IListingRepository, ListingRepository>();
        services.AddScoped<IReservationRepository, ReservationRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<IBlogPostRepository, BlogPostRepository>();
        services.AddScoped<IBlogCommentRepository, BlogCommentRepository>();
        services.AddScoped<IFavoriteRepository, FavoriteRepository>();
        services.AddScoped<IPaymentMethodRepository, PaymentMethodRepository>();
        services.AddScoped<INewsletterRepository, NewsletterRepository>();
        services.AddScoped<ICityRepository, CityRepository>();
        services.AddScoped<IAmenityRepository, AmenityRepository>();
        services.AddScoped<IDailyPricingRepository, DailyPricingRepository>();
        services.AddScoped<IHostBusinessSettingsRepository, HostBusinessSettingsRepository>();
        services.AddScoped<IHostListingMetadataRepository, HostListingMetadataRepository>();

        // Register Infrastructure Services
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<ITokenBlacklistService, MemoryCacheTokenBlacklistService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IFileStorageService, CloudflareR2Service>();
        services.AddScoped<ITranslationService, TranslationService>();

        // Admin Services
        services.AddScoped<IAdminDestinationService, AdminDestinationService>();
        services.AddScoped<IAdminListingService, AdminListingService>();
        services.AddScoped<IAdminBlogService, AdminBlogService>();
        services.AddScoped<IAdminMediaService, AdminMediaService>();

        return services;
    }
}
