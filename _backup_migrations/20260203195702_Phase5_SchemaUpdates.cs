using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Swimago.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class Phase5_SchemaUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Favorites_Listings_ListingId",
                table: "Favorites");

            migrationBuilder.DropIndex(
                name: "IX_Listings_IsActive",
                table: "Listings");

            migrationBuilder.DropIndex(
                name: "IX_Favorites_ListingId",
                table: "Favorites");

            migrationBuilder.DropIndex(
                name: "IX_Favorites_UserId_ListingId",
                table: "Favorites");

            migrationBuilder.DropColumn(
                name: "Comment",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "Provider",
                table: "ReservationPayments");

            migrationBuilder.DropColumn(
                name: "AltText",
                table: "ListingImages");

            migrationBuilder.DropColumn(
                name: "EndDateTime",
                table: "AvailabilityBlocks");

            migrationBuilder.DropColumn(
                name: "StartDateTime",
                table: "AvailabilityBlocks");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Amenities");

            migrationBuilder.RenameColumn(
                name: "ProfileImageUrl",
                table: "UserProfiles",
                newName: "Avatar");

            migrationBuilder.RenameColumn(
                name: "HostResponse",
                table: "Reviews",
                newName: "Categories");

            migrationBuilder.RenameColumn(
                name: "RefundedAmount",
                table: "ReservationPayments",
                newName: "RefundAmount");

            migrationBuilder.RenameColumn(
                name: "IsPrimary",
                table: "ListingImages",
                newName: "IsCover");

            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "ListingImages",
                newName: "Url");

            migrationBuilder.RenameColumn(
                name: "ListingId",
                table: "Favorites",
                newName: "VenueType");

            migrationBuilder.RenameColumn(
                name: "FeaturedImageUrl",
                table: "BlogPosts",
                newName: "ImageUrl");

            migrationBuilder.RenameColumn(
                name: "IsBlocked",
                table: "AvailabilityBlocks",
                newName: "IsAvailable");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Amenities",
                newName: "Label");

            migrationBuilder.RenameColumn(
                name: "IconName",
                table: "Amenities",
                newName: "Icon");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Amenities",
                newName: "ApplicableTo");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Users",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "LanguageSettings",
                table: "Users",
                type: "jsonb",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MembershipLevel",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NotificationSettings",
                table: "Users",
                type: "jsonb",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PrivacySettings",
                table: "Users",
                type: "jsonb",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "UserProfiles",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "UserProfiles",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<Guid>(
                name: "ReservationId",
                table: "Reviews",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<Guid>(
                name: "ListingId",
                table: "Reviews",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<Guid>(
                name: "GuestId",
                table: "Reviews",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Reviews",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "HostResponseText",
                table: "Reviews",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Text",
                table: "Reviews",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<Guid>(
                name: "ListingId",
                table: "Reservations",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<Guid>(
                name: "GuestId",
                table: "Reservations",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Reservations",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "CheckInCode",
                table: "Reservations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CheckedInAt",
                table: "Reservations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConfirmationNumber",
                table: "Reservations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "Reservations",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Guests",
                table: "Reservations",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PriceBreakdown",
                table: "Reservations",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Selections",
                table: "Reservations",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VenueType",
                table: "Reservations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<Guid>(
                name: "ReservationId",
                table: "ReservationPayments",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "ReservationPayments",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "PaymentIntentId",
                table: "ReservationPayments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PaymentMethodId",
                table: "ReservationPayments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "Notifications",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Notifications",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReadAt",
                table: "Notifications",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "HostId",
                table: "Listings",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Listings",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Listings",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Conditions",
                table: "Listings",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "Listings",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Duration",
                table: "Listings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFeatured",
                table: "Listings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSuperhost",
                table: "Listings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PriceCurrency",
                table: "Listings",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "PriceRangeMax",
                table: "Listings",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PriceRangeMin",
                table: "Listings",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "Listings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Listings",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "SpotCount",
                table: "Listings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Listings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<Guid>(
                name: "ListingId",
                table: "ListingImages",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "ListingImages",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "Alt",
                table: "ListingImages",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "AmenityId",
                table: "ListingAmenities",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<Guid>(
                name: "ListingId",
                table: "ListingAmenities",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<bool>(
                name: "IsEnabled",
                table: "ListingAmenities",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "Favorites",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Favorites",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<Guid>(
                name: "VenueId",
                table: "Favorites",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<Guid>(
                name: "ListingId",
                table: "DailyPricings",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "DailyPricings",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "Label",
                table: "DailyPricings",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Slug",
                table: "BlogPosts",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "jsonb");

            migrationBuilder.AlterColumn<Guid>(
                name: "AuthorId",
                table: "BlogPosts",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "BlogPosts",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "BlogPosts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "BlogPosts",
                type: "jsonb",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsFeatured",
                table: "BlogPosts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ReadTime",
                table: "BlogPosts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Tags",
                table: "BlogPosts",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "ListingId",
                table: "AvailabilityBlocks",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "AvailabilityBlocks",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<decimal>(
                name: "CustomPrice",
                table: "AvailabilityBlocks",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "EndDate",
                table: "AvailabilityBlocks",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<DateOnly>(
                name: "StartDate",
                table: "AvailabilityBlocks",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Amenities",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Amenities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Amenities",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "Cities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "jsonb", nullable: false),
                    Country = table.Column<string>(type: "text", nullable: false),
                    Latitude = table.Column<decimal>(type: "numeric", nullable: false),
                    Longitude = table.Column<decimal>(type: "numeric", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NewsletterSubscribers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    SubscribedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UnsubscribedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewsletterSubscribers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PaymentMethods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Brand = table.Column<int>(type: "integer", nullable: false),
                    Last4 = table.Column<string>(type: "text", nullable: false),
                    ExpiryMonth = table.Column<int>(type: "integer", nullable: false),
                    ExpiryYear = table.Column<int>(type: "integer", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProviderToken = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentMethods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentMethods_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_ConfirmationNumber",
                table: "Reservations",
                column: "ConfirmationNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReservationPayments_PaymentMethodId",
                table: "ReservationPayments",
                column: "PaymentMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_Listings_City",
                table: "Listings",
                column: "City");

            migrationBuilder.CreateIndex(
                name: "IX_Listings_IsFeatured",
                table: "Listings",
                column: "IsFeatured");

            migrationBuilder.CreateIndex(
                name: "IX_Listings_Slug",
                table: "Listings",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Listings_Status",
                table: "Listings",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Favorites_UserId_VenueId_VenueType",
                table: "Favorites",
                columns: new[] { "UserId", "VenueId", "VenueType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Favorites_VenueId",
                table: "Favorites",
                column: "VenueId");

            migrationBuilder.CreateIndex(
                name: "IX_BlogPosts_IsFeatured",
                table: "BlogPosts",
                column: "IsFeatured");

            migrationBuilder.CreateIndex(
                name: "IX_BlogPosts_IsPublished",
                table: "BlogPosts",
                column: "IsPublished");

            migrationBuilder.CreateIndex(
                name: "IX_BlogPosts_Slug",
                table: "BlogPosts",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cities_Country",
                table: "Cities",
                column: "Country");

            migrationBuilder.CreateIndex(
                name: "IX_NewsletterSubscribers_Email",
                table: "NewsletterSubscribers",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMethods_UserId",
                table: "PaymentMethods",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Favorites_Listings_VenueId",
                table: "Favorites",
                column: "VenueId",
                principalTable: "Listings",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ReservationPayments_PaymentMethods_PaymentMethodId",
                table: "ReservationPayments",
                column: "PaymentMethodId",
                principalTable: "PaymentMethods",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Favorites_Listings_VenueId",
                table: "Favorites");

            migrationBuilder.DropForeignKey(
                name: "FK_ReservationPayments_PaymentMethods_PaymentMethodId",
                table: "ReservationPayments");

            migrationBuilder.DropTable(
                name: "Cities");

            migrationBuilder.DropTable(
                name: "NewsletterSubscribers");

            migrationBuilder.DropTable(
                name: "PaymentMethods");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_ConfirmationNumber",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_ReservationPayments_PaymentMethodId",
                table: "ReservationPayments");

            migrationBuilder.DropIndex(
                name: "IX_Listings_City",
                table: "Listings");

            migrationBuilder.DropIndex(
                name: "IX_Listings_IsFeatured",
                table: "Listings");

            migrationBuilder.DropIndex(
                name: "IX_Listings_Slug",
                table: "Listings");

            migrationBuilder.DropIndex(
                name: "IX_Listings_Status",
                table: "Listings");

            migrationBuilder.DropIndex(
                name: "IX_Favorites_UserId_VenueId_VenueType",
                table: "Favorites");

            migrationBuilder.DropIndex(
                name: "IX_Favorites_VenueId",
                table: "Favorites");

            migrationBuilder.DropIndex(
                name: "IX_BlogPosts_IsFeatured",
                table: "BlogPosts");

            migrationBuilder.DropIndex(
                name: "IX_BlogPosts_IsPublished",
                table: "BlogPosts");

            migrationBuilder.DropIndex(
                name: "IX_BlogPosts_Slug",
                table: "BlogPosts");

            migrationBuilder.DropColumn(
                name: "LanguageSettings",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "MembershipLevel",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "NotificationSettings",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PrivacySettings",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "HostResponseText",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "Text",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "CheckInCode",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "CheckedInAt",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "ConfirmationNumber",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "Guests",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "PriceBreakdown",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "Selections",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "VenueType",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "PaymentIntentId",
                table: "ReservationPayments");

            migrationBuilder.DropColumn(
                name: "PaymentMethodId",
                table: "ReservationPayments");

            migrationBuilder.DropColumn(
                name: "ReadAt",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "Conditions",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "Duration",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "IsFeatured",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "IsSuperhost",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "PriceCurrency",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "PriceRangeMax",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "PriceRangeMin",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "SpotCount",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "Alt",
                table: "ListingImages");

            migrationBuilder.DropColumn(
                name: "IsEnabled",
                table: "ListingAmenities");

            migrationBuilder.DropColumn(
                name: "VenueId",
                table: "Favorites");

            migrationBuilder.DropColumn(
                name: "Label",
                table: "DailyPricings");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "BlogPosts");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "BlogPosts");

            migrationBuilder.DropColumn(
                name: "IsFeatured",
                table: "BlogPosts");

            migrationBuilder.DropColumn(
                name: "ReadTime",
                table: "BlogPosts");

            migrationBuilder.DropColumn(
                name: "Tags",
                table: "BlogPosts");

            migrationBuilder.DropColumn(
                name: "CustomPrice",
                table: "AvailabilityBlocks");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "AvailabilityBlocks");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "AvailabilityBlocks");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Amenities");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Amenities");

            migrationBuilder.RenameColumn(
                name: "Avatar",
                table: "UserProfiles",
                newName: "ProfileImageUrl");

            migrationBuilder.RenameColumn(
                name: "Categories",
                table: "Reviews",
                newName: "HostResponse");

            migrationBuilder.RenameColumn(
                name: "RefundAmount",
                table: "ReservationPayments",
                newName: "RefundedAmount");

            migrationBuilder.RenameColumn(
                name: "Url",
                table: "ListingImages",
                newName: "ImageUrl");

            migrationBuilder.RenameColumn(
                name: "IsCover",
                table: "ListingImages",
                newName: "IsPrimary");

            migrationBuilder.RenameColumn(
                name: "VenueType",
                table: "Favorites",
                newName: "ListingId");

            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "BlogPosts",
                newName: "FeaturedImageUrl");

            migrationBuilder.RenameColumn(
                name: "IsAvailable",
                table: "AvailabilityBlocks",
                newName: "IsBlocked");

            migrationBuilder.RenameColumn(
                name: "Label",
                table: "Amenities",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "Icon",
                table: "Amenities",
                newName: "IconName");

            migrationBuilder.RenameColumn(
                name: "ApplicableTo",
                table: "Amenities",
                newName: "Description");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Users",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "UserProfiles",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "UserProfiles",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "ReservationId",
                table: "Reviews",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<int>(
                name: "ListingId",
                table: "Reviews",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<int>(
                name: "GuestId",
                table: "Reviews",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Reviews",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "Comment",
                table: "Reviews",
                type: "jsonb",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<int>(
                name: "ListingId",
                table: "Reservations",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<int>(
                name: "GuestId",
                table: "Reservations",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Reservations",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "ReservationId",
                table: "ReservationPayments",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "ReservationPayments",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "Provider",
                table: "ReservationPayments",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "Notifications",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Notifications",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "HostId",
                table: "Listings",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Listings",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "ListingId",
                table: "ListingImages",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "ListingImages",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "AltText",
                table: "ListingImages",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "AmenityId",
                table: "ListingAmenities",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<int>(
                name: "ListingId",
                table: "ListingAmenities",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "Favorites",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Favorites",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "ListingId",
                table: "DailyPricings",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "DailyPricings",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "Slug",
                table: "BlogPosts",
                type: "jsonb",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "AuthorId",
                table: "BlogPosts",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "BlogPosts",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "ListingId",
                table: "AvailabilityBlocks",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "AvailabilityBlocks",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDateTime",
                table: "AvailabilityBlocks",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDateTime",
                table: "AvailabilityBlocks",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Amenities",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Amenities",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Listings_IsActive",
                table: "Listings",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Favorites_ListingId",
                table: "Favorites",
                column: "ListingId");

            migrationBuilder.CreateIndex(
                name: "IX_Favorites_UserId_ListingId",
                table: "Favorites",
                columns: new[] { "UserId", "ListingId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Favorites_Listings_ListingId",
                table: "Favorites",
                column: "ListingId",
                principalTable: "Listings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
