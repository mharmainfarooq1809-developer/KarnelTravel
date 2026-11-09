using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KarnelTravel.Migrations
{
    /// <inheritdoc />
    public partial class RemoveEmailVerification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CustomerAccounts_EmailVerificationCodeHash",
                table: "CustomerAccounts");

            migrationBuilder.DropColumn(
                name: "EmailConfirmed",
                table: "CustomerAccounts");

            migrationBuilder.DropColumn(
                name: "EmailVerificationCodeExpiresAt",
                table: "CustomerAccounts");

            migrationBuilder.DropColumn(
                name: "EmailVerificationCodeHash",
                table: "CustomerAccounts");

            migrationBuilder.DropColumn(
                name: "EmailVerificationTokenExpiresAt",
                table: "CustomerAccounts");

            migrationBuilder.DropColumn(
                name: "EmailVerificationTokenHash",
                table: "CustomerAccounts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EmailConfirmed",
                table: "CustomerAccounts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "EmailVerificationCodeExpiresAt",
                table: "CustomerAccounts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmailVerificationCodeHash",
                table: "CustomerAccounts",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EmailVerificationTokenExpiresAt",
                table: "CustomerAccounts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmailVerificationTokenHash",
                table: "CustomerAccounts",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerAccounts_EmailVerificationCodeHash",
                table: "CustomerAccounts",
                column: "EmailVerificationCodeHash");
        }
    }
}
