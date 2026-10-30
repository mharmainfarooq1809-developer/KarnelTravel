using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KarnelTravel.Migrations
{
    /// <inheritdoc />
    public partial class AddTransportReturnFare : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ReturnFarePKR",
                table: "TransportRoutePricings",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReturnFarePKR",
                table: "TransportRoutePricings");
        }
    }
}
