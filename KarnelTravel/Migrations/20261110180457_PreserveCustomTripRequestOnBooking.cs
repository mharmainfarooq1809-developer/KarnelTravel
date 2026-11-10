using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KarnelTravel.Migrations
{
    /// <inheritdoc />
    public partial class PreserveCustomTripRequestOnBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ServiceNeeds",
                table: "Bookings",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StartingCity",
                table: "Bookings",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TripInquiryId",
                table: "Bookings",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_TripInquiryId",
                table: "Bookings",
                column: "TripInquiryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_TripInquiries_TripInquiryId",
                table: "Bookings",
                column: "TripInquiryId",
                principalTable: "TripInquiries",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_TripInquiries_TripInquiryId",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_TripInquiryId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "ServiceNeeds",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "StartingCity",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "TripInquiryId",
                table: "Bookings");
        }
    }
}
