
using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KarnelTravel.Migrations
{
    public partial class AddTransportRoutePricing : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Existing database already has these columns:
            // DropoffLocation, PickupLocation and TripType.
            // Do not add them again.

            migrationBuilder.AddColumn<decimal>(
                name: "TransportDiscountPKR",
                table: "Bookings",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TransportOneWayFarePKR",
                table: "Bookings",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TransportReturnFarePKR",
                table: "Bookings",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TransportRoutePricings",
                columns: table => new
                {
                    Id = table.Column<int>(
                        type: "int",
                        nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),

                    TransportServiceId = table.Column<int>(
                        type: "int",
                        nullable: false),

                    PickupCity = table.Column<string>(
                        type: "nvarchar(150)",
                        maxLength: 150,
                        nullable: false),

                    DropoffCity = table.Column<string>(
                        type: "nvarchar(150)",
                        maxLength: 150,
                        nullable: false),

                    OneWayFarePKR = table.Column<decimal>(
                        type: "decimal(18,2)",
                        precision: 18,
                        scale: 2,
                        nullable: false),

                    IsActive = table.Column<bool>(
                        type: "bit",
                        nullable: false),

                    CreatedAt = table.Column<DateTime>(
                        type: "datetime2",
                        nullable: false),

                    UpdatedAt = table.Column<DateTime>(
                        type: "datetime2",
                        nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_TransportRoutePricings",
                        x => x.Id);

                    table.ForeignKey(
                        name: "FK_TransportRoutePricings_TransportServices_TransportServiceId",
                        column: x => x.TransportServiceId,
                        principalTable: "TransportServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TransportRoutePricings_TransportServiceId_PickupCity_DropoffCity",
                table: "TransportRoutePricings",
                columns: new[]
                {
                    "TransportServiceId",
                    "PickupCity",
                    "DropoffCity"
                },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TransportRoutePricings");

            migrationBuilder.DropColumn(
                name: "TransportDiscountPKR",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "TransportOneWayFarePKR",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "TransportReturnFarePKR",
                table: "Bookings");
        }
    }
}
