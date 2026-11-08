using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KarnelTravel.Migrations
{
    /// <inheritdoc />
    public partial class AddConversationOwnershipAndSubmissionIdempotency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ConvertedBookingId",
                table: "TripInquiries",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdempotencyKey",
                table: "TripInquiries",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OfferAcceptedAt",
                table: "TripInquiries",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OfferIdempotencyKey",
                table: "TripInquiries",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdempotencyKey",
                table: "Bookings",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAnonymous",
                table: "AIConversations",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "OwnerEmail",
                table: "AIConversations",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TripInquiries_IdempotencyKey",
                table: "TripInquiries",
                column: "IdempotencyKey",
                unique: true,
                filter: "[IdempotencyKey] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TripInquiries_OfferIdempotencyKey",
                table: "TripInquiries",
                column: "OfferIdempotencyKey",
                unique: true,
                filter: "[OfferIdempotencyKey] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_IdempotencyKey",
                table: "Bookings",
                column: "IdempotencyKey",
                unique: true,
                filter: "[IdempotencyKey] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AIConversations_OwnerEmail",
                table: "AIConversations",
                column: "OwnerEmail");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TripInquiries_IdempotencyKey",
                table: "TripInquiries");

            migrationBuilder.DropIndex(
                name: "IX_TripInquiries_OfferIdempotencyKey",
                table: "TripInquiries");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_IdempotencyKey",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_AIConversations_OwnerEmail",
                table: "AIConversations");

            migrationBuilder.DropColumn(
                name: "ConvertedBookingId",
                table: "TripInquiries");

            migrationBuilder.DropColumn(
                name: "IdempotencyKey",
                table: "TripInquiries");

            migrationBuilder.DropColumn(
                name: "OfferAcceptedAt",
                table: "TripInquiries");

            migrationBuilder.DropColumn(
                name: "OfferIdempotencyKey",
                table: "TripInquiries");

            migrationBuilder.DropColumn(
                name: "IdempotencyKey",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "IsAnonymous",
                table: "AIConversations");

            migrationBuilder.DropColumn(
                name: "OwnerEmail",
                table: "AIConversations");
        }
    }
}
