# Swimago API

> REST API for Swimago platform - Beach, Pool, and Boat Tour reservation system built with .NET 8, PostgreSQL, and Clean Architecture.

## 🚀 Features

- **Authentication & Authorization**: JWT-based auth with role management (Admin, Host, Customer)
- **Listings Management**: Beach, pool, and boat tour listings with PostGIS geospatial search
- **Reservation System**: Booking with availability checks, pricing calculation, and payment tracking
- **Reviews & Ratings**: Verified reviews with host responses and automatic rating calculation
- **Advanced Search**: Multi-language full-text search with filters, sorting, and faceting
- **Blog System**: Content management with SEO-friendly slugs and publish workflow
- **Admin Panel**: Platform management, user roles, listing moderation
- **Host Panel**: Dashboard, earnings, reservations management
- **Multi-Language Support**: JSONB-based i18n (Turkish, English, German, Russian)

## 📋 Tech Stack

- **.NET 8.0** - Web API
- **PostgreSQL 15** - Database
- **PostGIS** - Geospatial queries
- **Entity Framework Core 8** - ORM
- **AutoMapper** - Object mapping
- **FluentValidation** - Request validation
- **BCrypt.Net** - Password hashing
- **JWT** - Authentication
- **Swagger/OpenAPI** - API documentation

## 🏗️ Architecture

```
swimago-stich-api/
├── src/
│   ├── Swimago.Domain/          # Core entities, enums, interfaces
│   ├── Swimago.Application/     # DTOs, services, validators
│   ├── Swimago.Infrastructure/  # EF Core, repositories, external services
│   └── Swimago.API/             # Controllers, middleware, configuration
├── Dockerfile
├── docker-compose.yml
└── README.md
```

**Clean Architecture Principles:**
- Domain layer has no dependencies
- Application layer depends only on Domain
- Infrastructure depends on Domain + Application
- API depends on Application + Infrastructure

## 🚀 Quick Start with Supabase

This project uses **Supabase PostgreSQL** as its database. No local PostgreSQL installation required!

### Prerequisites
- .NET 8 SDK
- Supabase account ([supabase.com](https://supabase.com))

### Setup Steps

1. **Clone the Repository**
   ```bash
   git clone <repository-url>
   cd swimago-stich-api
   ```

2. **Configure Supabase Connection**
   
   Update `src/Swimago.API/appsettings.json` with your Supabase credentials:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Host=aws-1-eu-central-1.pooler.supabase.com;Database=postgres;Username=postgres.YOUR_PROJECT_ID;Password=YOUR_PASSWORD;Port=5432;SslMode=Require;Trust Server Certificate=true"
   }
   ```
   
   > Get your connection string from: Supabase Dashboard → Settings → Database → Connection Pooling

3. **Enable PostGIS Extension** (if not already enabled)
   
   In Supabase SQL Editor, run:
   ```sql
   CREATE EXTENSION IF NOT EXISTS postgis;
   ```

4. **Apply Database Migrations**
   ```bash
   dotnet ef database update --project src/Swimago.Infrastructure --startup-project src/Swimago.API
   ```

5. **Run the API**
   ```bash
   dotnet run --project src/Swimago.API
   ```

6. **Access Swagger**
   Navigate to `http://localhost:5088/swagger`

### 🐳 Local Development with Docker (Optional)

If you prefer local PostgreSQL for testing, see `docker-compose.yml` (configured for local development only).

## 💻 Development Workflow

### Hot Reload Development
```bash
dotnet watch run --project src/Swimago.API
```

### Check Migration Status
```bash
dotnet ef migrations list --project src/Swimago.Infrastructure --startup-project src/Swimago.API
```

### Create New Migration
```bash
dotnet ef migrations add MigrationName --project src/Swimago.Infrastructure --startup-project src/Swimago.API
```

## 📚 API Endpoints

### Public Endpoints
- `POST /api/auth/register` - User registration
- `POST /api/auth/login` - User login
- `GET /api/listings` - Browse listings (paginated)
- `GET /api/listings/nearby` - Geospatial search
- `GET /api/reservations/check-availability` - Check availability
- `GET /api/reviews/listing/{id}` - Listing reviews
- `POST /api/search/listings` - Advanced search
- `GET /api/blog` - Published blog posts
- `GET /api/health` - Health check

### Authenticated Endpoints
- `POST /api/reservations` - Create booking
- `GET /api/reservations/my-reservations` - User's bookings
- `POST /api/reviews` - Leave review (after completed reservation)
- `POST /api/reviews/{id}/host-response` - Host response

### Host Panel
- `GET /api/host/my-listings` - Host's listings
- `GET /api/host/reservations` - Incoming reservations
- `GET /api/host/dashboard` - Host statistics

### Admin Panel
- `GET /api/admin/dashboard` - Platform statistics
- `GET /api/admin/users` - User management
- `PUT /api/admin/users/{id}/role` - Update user role
- `PUT /api/admin/listings/{id}/toggle-active` - Moderate listings

## 🔐 Authentication

The API uses JWT Bearer tokens. After registration/login, include the token in requests:

```
Authorization: Bearer <your-jwt-token>
```

### User Roles
- **Customer**: Make reservations, leave reviews
- **Host**: Manage listings, view bookings, respond to reviews
- **Admin**: Full platform access, user/content moderation

## 🗄️ Database

### Migrations

Create a new migration:
```bash
dotnet ef migrations add MigrationName --project src/Swimago.Infrastructure --startup-project src/Swimago.API
```

Apply migrations:
```bash
dotnet ef database update --project src/Swimago.Infrastructure --startup-project src/Swimago.API
```

### Key Features
- **JSONB**: Multi-language content (Title, Description, Address, etc.)
- **PostGIS**: `geography(point)` for location data with spatial indexes
- **GIN Indexes**: Fast JSONB querying

## 🧪 Testing

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true
```

## 📦 Build & Deployment

### Build for Production

```bash
dotnet publish src/Swimago.API -c Release -o publish
```

### Docker Build

```bash
docker build -t swimago-api .
docker run -p 5088:8080 swimago-api
```

## 📝 Configuration

Key settings in `appsettings.json`:

```json
{
  "Jwt": {
    "SecretKey": "your-secret-key-minimum-256-bits",
    "ExpiryMinutes": 60,
    "RefreshTokenExpiryDays": 30
  },
  "SupportedLanguages": {
    "Default": "tr",
    "Available": ["tr", "en", "de", "ru"]
  }
}
```

## 🛠️ Development Tools

- **Swagger UI**: Interactive API documentation at `/swagger`
- **pgAdmin**: Database management at `http://localhost:5050` (when using Docker Compose)

---

**Built with ❤️ using Clean Architecture and .NET 8**
