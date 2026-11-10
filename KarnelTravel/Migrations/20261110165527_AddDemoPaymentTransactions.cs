using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KarnelTravel.Migrations
{
    /// <inheritdoc />
    public partial class AddDemoPaymentTransactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DemoPaymentTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookingId = table.Column<int>(type: "int", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: true),
                    TransactionReference = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    PaymentStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    FailureReason = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    RefundStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DemoPaymentTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DemoPaymentTransactions_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DemoPaymentTransactions_CustomerAccounts_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "CustomerAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DemoPaymentTransactions_BookingId",
                table: "DemoPaymentTransactions",
                column: "BookingId",
                unique: true,
                filter: "[PaymentStatus] = 'Paid (Demo)'");

            migrationBuilder.CreateIndex(
                name: "IX_DemoPaymentTransactions_BookingId_PaymentStatus",
                table: "DemoPaymentTransactions",
                columns: new[] { "BookingId", "PaymentStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_DemoPaymentTransactions_CustomerId",
                table: "DemoPaymentTransactions",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_DemoPaymentTransactions_TransactionReference",
                table: "DemoPaymentTransactions",
                column: "TransactionReference",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DemoPaymentTransactions");
        }
    }
}
