using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using Swimago.API.Authorization;
using Swimago.Infrastructure.Data;
using Swimago.Application;
using Swimago.Domain.Enums;
using Swimago.Infrastructure;
using System.Text;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, loggerConfig) => loggerConfig
    .ReadFrom.Configuration(context.Configuration)
    .WriteTo.Console()
    .Enrich.FromLogContext());
var seedMockData = builder.Configuration.GetValue<bool>("MockSeed:Enabled");

// Register Application and Infrastructure Services
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

// Exception Handling
builder.Services.AddExceptionHandler<Swimago.API.Infrastructure.GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Caching and HealthChecks
builder.Services.AddOutputCache();
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!);


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

    options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            var blacklistService = context.HttpContext.RequestServices.GetRequiredService<Swimago.Application.Interfaces.ITokenBlacklistService>();
            var token = context.Request.Headers.Authorization.FirstOrDefault()?.Replace("Bearer ", "");
            
            if (!string.IsNullOrEmpty(token) && await blacklistService.IsTokenBlacklistedAsync(token))
            {
                context.Fail("Token has been revoked.");
            }
        }
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
app.UseExceptionHandler(); // Uses GlobalExceptionHandler automatically
app.UseSerilogRequestLogging();
app.UseMiddleware<Swimago.API.Middleware.SecurityHeadersMiddleware>();
app.UseMiddleware<Swimago.API.Middleware.RateLimitMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseOutputCache();

app.MapHealthChecks("/health");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
