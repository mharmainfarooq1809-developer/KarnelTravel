using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KarnelTravel.Migrations
{
    /// <inheritdoc />
    public partial class CompleteDemoPaymentGateway : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DemoPaymentTransactions_BookingId",
                table: "DemoPaymentTransactions");

            migrationBuilder.AddColumn<string>(
                name: "CardBrand",
                table: "DemoPaymentTransactions",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSimulation",
                table: "DemoPaymentTransactions",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "MaskedCardNumber",
                table: "DemoPaymentTransactions",
                type: "nvarchar(25)",
                maxLength: 25,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DemoPaymentTransactions_BookingId",
                table: "DemoPaymentTransactions",
                column: "BookingId",
                unique: true,
                filter: "[PaymentStatus] = 'Successful'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DemoPaymentTransactions_BookingId",
                table: "DemoPaymentTransactions");

            migrationBuilder.DropColumn(
                name: "CardBrand",
                table: "DemoPaymentTransactions");

            migrationBuilder.DropColumn(
                name: "IsSimulation",
                table: "DemoPaymentTransactions");

            migrationBuilder.DropColumn(
                name: "MaskedCardNumber",
                table: "DemoPaymentTransactions");

            migrationBuilder.CreateIndex(
                name: "IX_DemoPaymentTransactions_BookingId",
                table: "DemoPaymentTransactions",
                column: "BookingId",
                unique: true,
                filter: "[PaymentStatus] = 'Paid (Demo)'");
        }
    }
}
