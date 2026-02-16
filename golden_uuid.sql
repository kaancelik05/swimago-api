CREATE EXTENSION IF NOT EXISTS postgis;
CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);



CREATE EXTENSION IF NOT EXISTS postgis;

CREATE TABLE "Amenities" (
    "Id" uuid NOT NULL,
    "Icon" text NOT NULL,
    "Label" jsonb NOT NULL,
    "Category" text,
    "ApplicableTo" jsonb,
    "IsActive" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Amenities" PRIMARY KEY ("Id")
);

CREATE TABLE "Cities" (
    "Id" uuid NOT NULL,
    "Name" jsonb NOT NULL,
    "Country" text NOT NULL,
    "Latitude" numeric NOT NULL,
    "Longitude" numeric NOT NULL,
    "IsActive" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Cities" PRIMARY KEY ("Id")
);

CREATE TABLE "NewsletterSubscribers" (
    "Id" uuid NOT NULL,
    "Email" text NOT NULL,
    "IsActive" boolean NOT NULL,
    "SubscribedAt" timestamp with time zone NOT NULL,
    "UnsubscribedAt" timestamp with time zone,
    CONSTRAINT "PK_NewsletterSubscribers" PRIMARY KEY ("Id")
);

CREATE TABLE "Users" (
    "Id" uuid NOT NULL,
    "Email" text NOT NULL,
    "PasswordHash" text NOT NULL,
    "Role" integer NOT NULL,
    "Status" integer NOT NULL,
    "IsEmailVerified" boolean NOT NULL,
    "RefreshToken" text,
    "RefreshTokenExpiry" timestamp with time zone NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "LastLoginAt" timestamp with time zone,
    "MembershipLevel" text NOT NULL,
    "NotificationSettings" jsonb NOT NULL,
    "LanguageSettings" jsonb NOT NULL,
    "PrivacySettings" jsonb NOT NULL,
    CONSTRAINT "PK_Users" PRIMARY KEY ("Id")
);

CREATE TABLE "BlogPosts" (
    "Id" uuid NOT NULL,
    "AuthorId" uuid NOT NULL,
    "Slug" text NOT NULL,
    "Title" jsonb NOT NULL,
    "Description" jsonb NOT NULL,
    "Content" jsonb NOT NULL,
    "ImageUrl" text,
    "Category" text,
    "Tags" jsonb,
    "ReadTime" integer NOT NULL,
    "IsPublished" boolean NOT NULL,
    "IsFeatured" boolean NOT NULL,
    "PublishedAt" timestamp with time zone,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "ViewCount" integer NOT NULL,
    CONSTRAINT "PK_BlogPosts" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_BlogPosts_Users_AuthorId" FOREIGN KEY ("AuthorId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Listings" (
    "Id" uuid NOT NULL,
    "HostId" uuid NOT NULL,
    "Type" integer NOT NULL,
    "Status" integer NOT NULL,
    "IsActive" boolean NOT NULL,
    "IsFeatured" boolean NOT NULL,
    "Slug" text NOT NULL,
    "Title" jsonb NOT NULL,
    "Description" jsonb NOT NULL,
    "Address" jsonb NOT NULL,
    "City" text NOT NULL,
    "Country" text NOT NULL,
    "Location" geography (point),
    "Latitude" numeric NOT NULL,
    "Longitude" numeric NOT NULL,
    "MaxGuestCount" integer NOT NULL,
    "BasePricePerHour" numeric NOT NULL,
    "BasePricePerDay" numeric NOT NULL,
    "PriceRangeMin" numeric,
    "PriceRangeMax" numeric,
    "PriceCurrency" text NOT NULL,
    "Conditions" jsonb,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "Rating" numeric NOT NULL,
    "ReviewCount" integer NOT NULL,
    "SpotCount" integer NOT NULL,
    "Duration" text,
    "IsSuperhost" boolean NOT NULL,
    "RejectionReason" text,
    CONSTRAINT "PK_Listings" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Listings_Users_HostId" FOREIGN KEY ("HostId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Notifications" (
    "Id" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "Type" integer NOT NULL,
    "Title" text NOT NULL,
    "Message" text NOT NULL,
    "ActionUrl" text,
    "IsRead" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "ReadAt" timestamp with time zone,
    CONSTRAINT "PK_Notifications" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Notifications_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE TABLE "PaymentMethods" (
    "Id" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "Type" text NOT NULL,
    "Brand" integer NOT NULL,
    "Last4" text NOT NULL,
    "ExpiryMonth" integer NOT NULL,
    "ExpiryYear" integer NOT NULL,
    "IsDefault" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "ProviderToken" text,
    CONSTRAINT "PK_PaymentMethods" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_PaymentMethods_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE TABLE "UserProfiles" (
    "Id" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "FirstName" jsonb NOT NULL,
    "LastName" jsonb NOT NULL,
    "PhoneNumber" text,
    "Avatar" text,
    "Bio" jsonb,
    CONSTRAINT "PK_UserProfiles" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_UserProfiles_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE TABLE "AvailabilityBlocks" (
    "Id" uuid NOT NULL,
    "ListingId" uuid NOT NULL,
    "StartDate" date NOT NULL,
    "EndDate" date NOT NULL,
    "IsAvailable" boolean NOT NULL,
    "Reason" text,
    "CustomPrice" numeric,
    CONSTRAINT "PK_AvailabilityBlocks" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_AvailabilityBlocks_Listings_ListingId" FOREIGN KEY ("ListingId") REFERENCES "Listings" ("Id") ON DELETE CASCADE
);

CREATE TABLE "DailyPricings" (
    "Id" uuid NOT NULL,
    "ListingId" uuid NOT NULL,
    "Date" date NOT NULL,
    "Price" numeric NOT NULL,
    "HourlyPrice" numeric,
    "IsAvailable" boolean NOT NULL,
    "Label" text,
    "Notes" text,
    CONSTRAINT "PK_DailyPricings" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_DailyPricings_Listings_ListingId" FOREIGN KEY ("ListingId") REFERENCES "Listings" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Favorites" (
    "Id" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "VenueId" uuid NOT NULL,
    "VenueType" integer NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Favorites" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Favorites_Listings_VenueId" FOREIGN KEY ("VenueId") REFERENCES "Listings" ("Id"),
    CONSTRAINT "FK_Favorites_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE TABLE "ListingAmenities" (
    "ListingId" uuid NOT NULL,
    "AmenityId" uuid NOT NULL,
    "IsEnabled" boolean NOT NULL,
    CONSTRAINT "PK_ListingAmenities" PRIMARY KEY ("ListingId", "AmenityId"),
    CONSTRAINT "FK_ListingAmenities_Amenities_AmenityId" FOREIGN KEY ("AmenityId") REFERENCES "Amenities" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_ListingAmenities_Listings_ListingId" FOREIGN KEY ("ListingId") REFERENCES "Listings" ("Id") ON DELETE CASCADE
);

CREATE TABLE "ListingImages" (
    "Id" uuid NOT NULL,
    "ListingId" uuid NOT NULL,
    "Url" text NOT NULL,
    "Alt" text,
    "DisplayOrder" integer NOT NULL,
    "IsCover" boolean NOT NULL,
    CONSTRAINT "PK_ListingImages" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_ListingImages_Listings_ListingId" FOREIGN KEY ("ListingId") REFERENCES "Listings" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Reservations" (
    "Id" uuid NOT NULL,
    "ListingId" uuid NOT NULL,
    "GuestId" uuid NOT NULL,
    "VenueType" integer NOT NULL,
    "BookingType" integer NOT NULL,
    "StartTime" timestamp with time zone NOT NULL,
    "EndTime" timestamp with time zone NOT NULL,
    "GuestCount" integer NOT NULL,
    "Guests" jsonb,
    "Selections" jsonb,
    "UnitPrice" numeric NOT NULL,
    "UnitCount" integer NOT NULL,
    "TotalPrice" numeric NOT NULL,
    "DiscountAmount" numeric,
    "FinalPrice" numeric NOT NULL,
    "Currency" text NOT NULL,
    "PriceBreakdown" jsonb,
    "Status" integer NOT NULL,
    "ConfirmationNumber" text,
    "CheckInCode" text,
    "SpecialRequests" jsonb,
    "CreatedAt" timestamp with time zone NOT NULL,
    "ConfirmedAt" timestamp with time zone,
    "CheckedInAt" timestamp with time zone,
    "CancelledAt" timestamp with time zone,
    "CancellationReason" text,
    CONSTRAINT "PK_Reservations" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Reservations_Listings_ListingId" FOREIGN KEY ("ListingId") REFERENCES "Listings" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Reservations_Users_GuestId" FOREIGN KEY ("GuestId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE TABLE "ReservationPayments" (
    "Id" uuid NOT NULL,
    "ReservationId" uuid NOT NULL,
    "PaymentMethodId" uuid,
    "Amount" numeric NOT NULL,
    "Currency" text NOT NULL,
    "Status" integer NOT NULL,
    "ProviderTransactionId" text,
    "PaymentIntentId" text,
    "CreatedAt" timestamp with time zone NOT NULL,
    "PaidAt" timestamp with time zone,
    "RefundedAt" timestamp with time zone,
    "RefundAmount" numeric,
    CONSTRAINT "PK_ReservationPayments" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_ReservationPayments_PaymentMethods_PaymentMethodId" FOREIGN KEY ("PaymentMethodId") REFERENCES "PaymentMethods" ("Id"),
    CONSTRAINT "FK_ReservationPayments_Reservations_ReservationId" FOREIGN KEY ("ReservationId") REFERENCES "Reservations" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Reviews" (
    "Id" uuid NOT NULL,
    "ReservationId" uuid NOT NULL,
    "ListingId" uuid NOT NULL,
    "GuestId" uuid NOT NULL,
    "Rating" integer NOT NULL,
    "Text" text NOT NULL,
    "Categories" jsonb,
    "HostResponseText" text,
    "HostResponseDate" timestamp with time zone,
    "CreatedAt" timestamp with time zone NOT NULL,
    "IsVerified" boolean NOT NULL,
    CONSTRAINT "PK_Reviews" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Reviews_Listings_ListingId" FOREIGN KEY ("ListingId") REFERENCES "Listings" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Reviews_Reservations_ReservationId" FOREIGN KEY ("ReservationId") REFERENCES "Reservations" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Reviews_Users_GuestId" FOREIGN KEY ("GuestId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_AvailabilityBlocks_ListingId" ON "AvailabilityBlocks" ("ListingId");

CREATE INDEX "IX_BlogPosts_AuthorId" ON "BlogPosts" ("AuthorId");

CREATE INDEX "IX_BlogPosts_IsFeatured" ON "BlogPosts" ("IsFeatured");

CREATE INDEX "IX_BlogPosts_IsPublished" ON "BlogPosts" ("IsPublished");

CREATE UNIQUE INDEX "IX_BlogPosts_Slug" ON "BlogPosts" ("Slug");

CREATE INDEX "IX_BlogPosts_Title" ON "BlogPosts" USING gin ("Title");

CREATE INDEX "IX_Cities_Country" ON "Cities" ("Country");

CREATE INDEX "IX_DailyPricings_ListingId_Date" ON "DailyPricings" ("ListingId", "Date");

CREATE UNIQUE INDEX "IX_Favorites_UserId_VenueId_VenueType" ON "Favorites" ("UserId", "VenueId", "VenueType");

CREATE INDEX "IX_Favorites_VenueId" ON "Favorites" ("VenueId");

CREATE INDEX "IX_ListingAmenities_AmenityId" ON "ListingAmenities" ("AmenityId");

CREATE INDEX "IX_ListingImages_ListingId" ON "ListingImages" ("ListingId");

CREATE INDEX "IX_Listings_City" ON "Listings" ("City");

CREATE INDEX "IX_Listings_Description" ON "Listings" USING gin ("Description");

CREATE INDEX "IX_Listings_HostId" ON "Listings" ("HostId");

CREATE INDEX "IX_Listings_IsFeatured" ON "Listings" ("IsFeatured");

CREATE INDEX "IX_Listings_Location" ON "Listings" USING gist ("Location");

CREATE UNIQUE INDEX "IX_Listings_Slug" ON "Listings" ("Slug");

CREATE INDEX "IX_Listings_Status" ON "Listings" ("Status");

CREATE INDEX "IX_Listings_Title" ON "Listings" USING gin ("Title");

CREATE INDEX "IX_Listings_Type" ON "Listings" ("Type");

CREATE UNIQUE INDEX "IX_NewsletterSubscribers_Email" ON "NewsletterSubscribers" ("Email");

CREATE INDEX "IX_Notifications_UserId" ON "Notifications" ("UserId");

CREATE INDEX "IX_PaymentMethods_UserId" ON "PaymentMethods" ("UserId");

CREATE INDEX "IX_ReservationPayments_PaymentMethodId" ON "ReservationPayments" ("PaymentMethodId");

CREATE UNIQUE INDEX "IX_ReservationPayments_ReservationId" ON "ReservationPayments" ("ReservationId");

CREATE UNIQUE INDEX "IX_Reservations_ConfirmationNumber" ON "Reservations" ("ConfirmationNumber");

CREATE INDEX "IX_Reservations_GuestId" ON "Reservations" ("GuestId");

CREATE INDEX "IX_Reservations_ListingId" ON "Reservations" ("ListingId");

CREATE INDEX "IX_Reservations_StartTime" ON "Reservations" ("StartTime");

CREATE INDEX "IX_Reservations_Status" ON "Reservations" ("Status");

CREATE INDEX "IX_Reviews_GuestId" ON "Reviews" ("GuestId");

CREATE INDEX "IX_Reviews_ListingId" ON "Reviews" ("ListingId");

CREATE UNIQUE INDEX "IX_Reviews_ReservationId" ON "Reviews" ("ReservationId");

CREATE UNIQUE INDEX "IX_UserProfiles_UserId" ON "UserProfiles" ("UserId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260203204953_InitialCreate_UUID', '8.0.11');



