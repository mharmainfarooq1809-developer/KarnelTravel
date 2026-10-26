using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KarnelTravel.Migrations
{
    /// <inheritdoc />
    public partial class AddDestinationRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DestinationId",
                table: "TransportServices",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DestinationId",
                table: "TourPackages",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DestinationId",
                table: "Restaurants",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DestinationId",
                table: "Resorts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DestinationId",
                table: "Hotels",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransportServices_DestinationId",
                table: "TransportServices",
                column: "DestinationId");

            migrationBuilder.CreateIndex(
                name: "IX_TourPackages_DestinationId",
                table: "TourPackages",
                column: "DestinationId");

            migrationBuilder.CreateIndex(
                name: "IX_Restaurants_DestinationId",
                table: "Restaurants",
                column: "DestinationId");

            migrationBuilder.CreateIndex(
                name: "IX_Resorts_DestinationId",
                table: "Resorts",
                column: "DestinationId");

            migrationBuilder.CreateIndex(
                name: "IX_Hotels_DestinationId",
                table: "Hotels",
                column: "DestinationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Hotels_Destinations_DestinationId",
                table: "Hotels",
                column: "DestinationId",
                principalTable: "Destinations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Resorts_Destinations_DestinationId",
                table: "Resorts",
                column: "DestinationId",
                principalTable: "Destinations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Restaurants_Destinations_DestinationId",
                table: "Restaurants",
                column: "DestinationId",
                principalTable: "Destinations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TourPackages_Destinations_DestinationId",
                table: "TourPackages",
                column: "DestinationId",
                principalTable: "Destinations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TransportServices_Destinations_DestinationId",
                table: "TransportServices",
                column: "DestinationId",
                principalTable: "Destinations",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Hotels_Destinations_DestinationId",
                table: "Hotels");

            migrationBuilder.DropForeignKey(
                name: "FK_Resorts_Destinations_DestinationId",
                table: "Resorts");

            migrationBuilder.DropForeignKey(
                name: "FK_Restaurants_Destinations_DestinationId",
                table: "Restaurants");

            migrationBuilder.DropForeignKey(
                name: "FK_TourPackages_Destinations_DestinationId",
                table: "TourPackages");

            migrationBuilder.DropForeignKey(
                name: "FK_TransportServices_Destinations_DestinationId",
                table: "TransportServices");

            migrationBuilder.DropIndex(
                name: "IX_TransportServices_DestinationId",
                table: "TransportServices");

            migrationBuilder.DropIndex(
                name: "IX_TourPackages_DestinationId",
                table: "TourPackages");

            migrationBuilder.DropIndex(
                name: "IX_Restaurants_DestinationId",
                table: "Restaurants");

            migrationBuilder.DropIndex(
                name: "IX_Resorts_DestinationId",
                table: "Resorts");

            migrationBuilder.DropIndex(
                name: "IX_Hotels_DestinationId",
                table: "Hotels");

            migrationBuilder.DropColumn(
                name: "DestinationId",
                table: "TransportServices");

            migrationBuilder.DropColumn(
                name: "DestinationId",
                table: "TourPackages");

            migrationBuilder.DropColumn(
                name: "DestinationId",
                table: "Restaurants");

            migrationBuilder.DropColumn(
                name: "DestinationId",
                table: "Resorts");

            migrationBuilder.DropColumn(
                name: "DestinationId",
                table: "Hotels");
        }
    }
}
