using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using Swimago.API.Authorization;
using Swimago.Infrastructure.Data;
using Swimago.Application;
using Swimago.Domain.Enums;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var seedMockData = builder.Configuration.GetValue<bool>("MockSeed:Enabled");

// Add DbContext with PostgreSQL and PostGIS
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions =>
        {
            npgsqlOptions.UseNetTopologySuite(); // Enable PostGIS support
            npgsqlOptions.MigrationsAssembly("Swimago.Infrastructure"); // Migrations in Infrastructure project
        });
});

// Register Repositories
builder.Services.AddScoped<Swimago.Domain.Interfaces.IUnitOfWork, Swimago.Infrastructure.Repositories.UnitOfWork>();
builder.Services.AddScoped<Swimago.Domain.Interfaces.IUserRepository, Swimago.Infrastructure.Repositories.UserRepository>();
builder.Services.AddScoped<Swimago.Domain.Interfaces.IListingRepository, Swimago.Infrastructure.Repositories.ListingRepository>();
builder.Services.AddScoped<Swimago.Domain.Interfaces.IReservationRepository, Swimago.Infrastructure.Repositories.ReservationRepository>();
builder.Services.AddScoped<Swimago.Domain.Interfaces.IReviewRepository, Swimago.Infrastructure.Repositories.ReviewRepository>();
builder.Services.AddScoped<Swimago.Domain.Interfaces.IBlogPostRepository, Swimago.Infrastructure.Repositories.BlogPostRepository>();
builder.Services.AddScoped<Swimago.Domain.Interfaces.IBlogCommentRepository, Swimago.Infrastructure.Repositories.BlogCommentRepository>();
builder.Services.AddScoped<Swimago.Domain.Interfaces.IFavoriteRepository, Swimago.Infrastructure.Repositories.FavoriteRepository>();
builder.Services.AddScoped<Swimago.Domain.Interfaces.IPaymentMethodRepository, Swimago.Infrastructure.Repositories.PaymentMethodRepository>();
builder.Services.AddScoped<Swimago.Domain.Interfaces.INewsletterRepository, Swimago.Infrastructure.Repositories.NewsletterRepository>();
builder.Services.AddScoped<Swimago.Domain.Interfaces.ICityRepository, Swimago.Infrastructure.Repositories.CityRepository>();
builder.Services.AddScoped<Swimago.Domain.Interfaces.IAmenityRepository, Swimago.Infrastructure.Repositories.AmenityRepository>();
builder.Services.AddScoped<Swimago.Domain.Interfaces.IDailyPricingRepository, Swimago.Infrastructure.Repositories.DailyPricingRepository>();
builder.Services.AddScoped<Swimago.Domain.Interfaces.IHostBusinessSettingsRepository, Swimago.Infrastructure.Repositories.HostBusinessSettingsRepository>();
builder.Services.AddScoped<Swimago.Domain.Interfaces.IHostListingMetadataRepository, Swimago.Infrastructure.Repositories.HostListingMetadataRepository>();

// Register Application Services (including Validators)
builder.Services.AddApplication();

// Register Services
builder.Services.AddScoped<Swimago.Application.Interfaces.ITokenService, Swimago.Infrastructure.Services.TokenService>();
builder.Services.AddScoped<Swimago.Application.Interfaces.IAuthService, Swimago.Infrastructure.Services.AuthService>();
builder.Services.AddScoped<Swimago.Application.Interfaces.IReservationService, Swimago.Application.Services.ReservationService>();
builder.Services.AddScoped<Swimago.Application.Interfaces.IPricingService, Swimago.Application.Services.PricingService>();
builder.Services.AddScoped<Swimago.Application.Interfaces.IReviewService, Swimago.Application.Services.ReviewService>();
builder.Services.AddScoped<Swimago.Application.Interfaces.ISearchService, Swimago.Application.Services.SearchService>();
builder.Services.AddScoped<Swimago.Application.Interfaces.IBlogService, Swimago.Application.Services.BlogService>();
builder.Services.AddScoped<Swimago.Application.Interfaces.IEmailService, Swimago.Infrastructure.Services.EmailService>();
builder.Services.AddScoped<Swimago.Application.Interfaces.IFileStorageService, Swimago.Infrastructure.Services.CloudflareR2Service>();
builder.Services.AddScoped<Swimago.Application.Interfaces.ITranslationService, Swimago.Infrastructure.Services.TranslationService>();

// Admin Services
builder.Services.AddScoped<Swimago.Application.Interfaces.IAdminDestinationService, Swimago.Infrastructure.Services.Admin.AdminDestinationService>();
builder.Services.AddScoped<Swimago.Application.Interfaces.IAdminListingService, Swimago.Infrastructure.Services.Admin.AdminListingService>();
builder.Services.AddScoped<Swimago.Application.Interfaces.IAdminBlogService, Swimago.Infrastructure.Services.Admin.AdminBlogService>();
builder.Services.AddScoped<Swimago.Application.Interfaces.IAdminMediaService, Swimago.Infrastructure.Services.Admin.AdminMediaService>();


// Configure JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!)),
        NameClaimType = System.Security.Claims.ClaimTypes.NameIdentifier,
        RoleClaimType = System.Security.Claims.ClaimTypes.Role
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AuthorizationPolicies.CustomerOnly, policy =>
        policy.RequireAuthenticatedUser().RequireRole(Role.Customer.ToString()));

    options.AddPolicy(AuthorizationPolicies.HostOnly, policy =>
        policy.RequireAuthenticatedUser().RequireRole(Role.Host.ToString()));

    options.AddPolicy(AuthorizationPolicies.HostOrAdmin, policy =>
        policy.RequireAuthenticatedUser().RequireRole(Role.Host.ToString(), Role.Admin.ToString()));

    options.AddPolicy(AuthorizationPolicies.AdminOnly, policy =>
        policy.RequireAuthenticatedUser().RequireRole(Role.Admin.ToString()));
});


// Add Controllers
builder.Services.AddControllers();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Swimago API",
        Version = "v1",
        Description = "REST API for Swimago platform - Beach, Pool, and Boat Tour reservations",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Swimago Support",
            Email = "support@swimago.com"
        }
    });

    // Include XML comments
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }

    // Add JWT Bearer Security Definition
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 12345abcdef\""
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Use full type names for schema IDs to avoid conflicts
    options.CustomSchemaIds(type => type.ToString());
});


var app = builder.Build();

if (seedMockData)
{
    using var scope = app.Services.CreateScope();
    var logger = scope.ServiceProvider
        .GetRequiredService<ILoggerFactory>()
        .CreateLogger("MockDataSeeder");
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    try
    {
        await MockDataSeeder.SeedAsync(dbContext, logger);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Mock data seeding failed.");
        throw;
    }
}

// Configure the HTTP request pipeline
app.UseMiddleware<Swimago.API.Middleware.SecurityHeadersMiddleware>();
app.UseMiddleware<Swimago.API.Middleware.RateLimitMiddleware>();
app.UseMiddleware<Swimago.API.Middleware.ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
