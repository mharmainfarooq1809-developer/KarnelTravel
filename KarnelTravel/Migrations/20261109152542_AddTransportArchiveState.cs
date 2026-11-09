using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KarnelTravel.Migrations
{
    /// <inheritdoc />
    public partial class AddTransportArchiveState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ArchivedAt",
                table: "TransportServices",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "TransportServices",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ArchivedAt",
                table: "TransportRoutePricings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "TransportRoutePricings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_TransportServices_IsArchived",
                table: "TransportServices",
                column: "IsArchived");

            migrationBuilder.CreateIndex(
                name: "IX_TransportRoutePricings_TransportServiceId_IsArchived",
                table: "TransportRoutePricings",
                columns: new[] { "TransportServiceId", "IsArchived" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TransportServices_IsArchived",
                table: "TransportServices");

            migrationBuilder.DropIndex(
                name: "IX_TransportRoutePricings_TransportServiceId_IsArchived",
                table: "TransportRoutePricings");

            migrationBuilder.DropColumn(
                name: "ArchivedAt",
                table: "TransportServices");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "TransportServices");

            migrationBuilder.DropColumn(
                name: "ArchivedAt",
                table: "TransportRoutePricings");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "TransportRoutePricings");
        }
    }
}
