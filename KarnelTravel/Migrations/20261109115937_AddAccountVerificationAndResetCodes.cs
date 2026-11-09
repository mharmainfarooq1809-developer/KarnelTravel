using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KarnelTravel.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountVerificationAndResetCodes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "PasswordResetCodeExpiresAt",
                table: "CustomerAccounts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PasswordResetCodeHash",
                table: "CustomerAccounts",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerAccounts_EmailVerificationCodeHash",
                table: "CustomerAccounts",
                column: "EmailVerificationCodeHash");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerAccounts_PasswordResetCodeHash",
                table: "CustomerAccounts",
                column: "PasswordResetCodeHash");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CustomerAccounts_EmailVerificationCodeHash",
                table: "CustomerAccounts");

            migrationBuilder.DropIndex(
                name: "IX_CustomerAccounts_PasswordResetCodeHash",
                table: "CustomerAccounts");

            migrationBuilder.DropColumn(
                name: "EmailVerificationCodeExpiresAt",
                table: "CustomerAccounts");

            migrationBuilder.DropColumn(
                name: "EmailVerificationCodeHash",
                table: "CustomerAccounts");

            migrationBuilder.DropColumn(
                name: "PasswordResetCodeExpiresAt",
                table: "CustomerAccounts");

            migrationBuilder.DropColumn(
                name: "PasswordResetCodeHash",
                table: "CustomerAccounts");
        }
    }
}
