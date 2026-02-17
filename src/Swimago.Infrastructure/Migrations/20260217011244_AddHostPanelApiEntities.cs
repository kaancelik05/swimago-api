using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Swimago.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHostPanelApiEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Source",
                table: "Reservations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "HostBusinessSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HostId = table.Column<Guid>(type: "uuid", nullable: false),
                    AutoConfirmReservations = table.Column<bool>(type: "boolean", nullable: false),
                    AllowSameDayBookings = table.Column<bool>(type: "boolean", nullable: false),
                    MinimumNoticeHours = table.Column<int>(type: "integer", nullable: false),
                    CancellationWindowHours = table.Column<int>(type: "integer", nullable: false),
                    DynamicPricingEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    SmartOverbookingProtection = table.Column<bool>(type: "boolean", nullable: false),
                    WhatsappNotifications = table.Column<bool>(type: "boolean", nullable: false),
                    EmailNotifications = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HostBusinessSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HostBusinessSettings_Users_HostId",
                        column: x => x.HostId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HostListingMetadata",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ListingId = table.Column<Guid>(type: "uuid", nullable: false),
                    Highlights = table.Column<string>(type: "jsonb", nullable: false),
                    SeatingAreas = table.Column<string>(type: "jsonb", nullable: false),
                    AvailabilityNotes = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HostListingMetadata", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HostListingMetadata_Listings_ListingId",
                        column: x => x.ListingId,
                        principalTable: "Listings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_Source",
                table: "Reservations",
                column: "Source");

            migrationBuilder.CreateIndex(
                name: "IX_HostBusinessSettings_HostId",
                table: "HostBusinessSettings",
                column: "HostId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HostListingMetadata_ListingId",
                table: "HostListingMetadata",
                column: "ListingId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HostBusinessSettings");

            migrationBuilder.DropTable(
                name: "HostListingMetadata");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_Source",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "Source",
                table: "Reservations");
        }
    }
}
