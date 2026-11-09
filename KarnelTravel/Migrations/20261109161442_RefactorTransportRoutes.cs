using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KarnelTravel.Migrations
{
    /// <inheritdoc />
    public partial class RefactorTransportRoutes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TransportRoutePricings_TransportServiceId_IsArchived",
                table: "TransportRoutePricings");

            migrationBuilder.DropIndex(
                name: "IX_TransportRoutePricings_TransportServiceId_PickupCity_DropoffCity",
                table: "TransportRoutePricings");

            migrationBuilder.AlterColumn<string>(
                name: "PickupCity",
                table: "TransportRoutePricings",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<string>(
                name: "DropoffCity",
                table: "TransportRoutePricings",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.AddColumn<int>(
                name: "TransportRouteId",
                table: "TransportRoutePricings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TransportRouteId",
                table: "Bookings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TransportRouteName",
                table: "Bookings",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TransportRoutes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    StartingCity = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    DestinationCity = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    StartingPricePKR = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ReturnPricePKR = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    EstimatedDistanceKm = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    EstimatedDurationMinutes = table.Column<int>(type: "int", nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    IsFeatured = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsArchived = table.Column<bool>(type: "bit", nullable: false),
                    ArchivedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransportRoutes", x => x.Id);
                });

            migrationBuilder.Sql(@"
                ;WITH LegacyRoutes AS
                (
                    SELECT DISTINCT
                        PickupCity,
                        DropoffCity,
                        ROW_NUMBER() OVER (ORDER BY PickupCity, DropoffCity) AS RouteNumber
                    FROM TransportRoutePricings
                    WHERE PickupCity IS NOT NULL AND DropoffCity IS NOT NULL
                )
                INSERT INTO TransportRoutes
                (
                    Name, Slug, StartingCity, DestinationCity, StartingPricePKR,
                    ReturnPricePKR, Description, DisplayOrder, IsPublished,
                    IsFeatured, IsActive, IsArchived, CreatedAt
                )
                SELECT
                    CONCAT(PickupCity, ' - ', DropoffCity),
                    CONCAT('legacy-route-', RouteNumber),
                    PickupCity,
                    DropoffCity,
                    0,
                    NULL,
                    'Imported from the existing transport route pricing data.',
                    RouteNumber,
                    0,
                    0,
                    1,
                    0,
                    SYSUTCDATETIME()
                FROM LegacyRoutes;

                UPDATE pricing
                SET TransportRouteId = route.Id
                FROM TransportRoutePricings pricing
                INNER JOIN TransportRoutes route
                    ON route.StartingCity = pricing.PickupCity
                    AND route.DestinationCity = pricing.DropoffCity;

                UPDATE booking
                SET TransportRouteId = route.Id,
                    TransportRouteName = route.Name
                FROM Bookings booking
                INNER JOIN TransportServices vehicle
                    ON vehicle.Id = booking.TransportServiceId
                INNER JOIN TransportRoutePricings pricing
                    ON pricing.TransportServiceId = vehicle.Id
                    AND pricing.PickupCity = booking.PickupLocation
                    AND pricing.DropoffCity = booking.DropoffLocation
                INNER JOIN TransportRoutes route
                    ON route.Id = pricing.TransportRouteId
                WHERE booking.ServiceType = 'Transport';
            ");

            migrationBuilder.AlterColumn<int>(
                name: "TransportRouteId",
                table: "TransportRoutePricings",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransportRoutePricings_TransportRouteId_TransportServiceId",
                table: "TransportRoutePricings",
                columns: new[] { "TransportRouteId", "TransportServiceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransportRoutePricings_TransportServiceId",
                table: "TransportRoutePricings",
                column: "TransportServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_TransportRouteId",
                table: "Bookings",
                column: "TransportRouteId");

            migrationBuilder.CreateIndex(
                name: "IX_TransportRoutes_IsPublished_IsArchived_DisplayOrder",
                table: "TransportRoutes",
                columns: new[] { "IsPublished", "IsArchived", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_TransportRoutes_Slug",
                table: "TransportRoutes",
                column: "Slug",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_TransportRoutes_TransportRouteId",
                table: "Bookings",
                column: "TransportRouteId",
                principalTable: "TransportRoutes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TransportRoutePricings_TransportRoutes_TransportRouteId",
                table: "TransportRoutePricings",
                column: "TransportRouteId",
                principalTable: "TransportRoutes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_TransportRoutes_TransportRouteId",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_TransportRoutePricings_TransportRoutes_TransportRouteId",
                table: "TransportRoutePricings");

            migrationBuilder.DropTable(
                name: "TransportRoutes");

            migrationBuilder.DropIndex(
                name: "IX_TransportRoutePricings_TransportRouteId_TransportServiceId",
                table: "TransportRoutePricings");

            migrationBuilder.DropIndex(
                name: "IX_TransportRoutePricings_TransportServiceId",
                table: "TransportRoutePricings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_TransportRouteId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "TransportRouteId",
                table: "TransportRoutePricings");

            migrationBuilder.DropColumn(
                name: "TransportRouteId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "TransportRouteName",
                table: "Bookings");

            migrationBuilder.AlterColumn<string>(
                name: "PickupCity",
                table: "TransportRoutePricings",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DropoffCity",
                table: "TransportRoutePricings",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransportRoutePricings_TransportServiceId_IsArchived",
                table: "TransportRoutePricings",
                columns: new[] { "TransportServiceId", "IsArchived" });

            migrationBuilder.CreateIndex(
                name: "IX_TransportRoutePricings_TransportServiceId_PickupCity_DropoffCity",
                table: "TransportRoutePricings",
                columns: new[] { "TransportServiceId", "PickupCity", "DropoffCity" },
                unique: true);
        }
    }
}
