using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KarnelTravel.Migrations
{
    /// <inheritdoc />
    public partial class AddApplicationSettingsAndAuditLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdminAuditLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Administrator = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EntityId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Succeeded = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OrganizationName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DefaultLanguage = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DefaultCountry = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TimeZone = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    DateFormat = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    TimeFormat = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CountryCallingCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ContactEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ContactPhone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    MaintenanceMode = table.Column<bool>(type: "bit", nullable: false),
                    PublicWebsiteVisible = table.Column<bool>(type: "bit", nullable: false),
                    AllowBookingInquiries = table.Column<bool>(type: "bit", nullable: false),
                    AllowContactSubmissions = table.Column<bool>(type: "bit", nullable: false),
                    AllowFeedbackSubmissions = table.Column<bool>(type: "bit", nullable: false),
                    ShowDemoContentNotice = table.Column<bool>(type: "bit", nullable: false),
                    DefaultSeoTitle = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    DefaultSeoDescription = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false),
                    BookingEnabled = table.Column<bool>(type: "bit", nullable: false),
                    DefaultBookingStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    AllowGuestBookings = table.Column<bool>(type: "bit", nullable: false),
                    RequirePhone = table.Column<bool>(type: "bit", nullable: false),
                    RequireEmail = table.Column<bool>(type: "bit", nullable: false),
                    MinimumAdvanceBookingDays = table.Column<int>(type: "int", nullable: false),
                    MaximumAdvanceBookingDays = table.Column<int>(type: "int", nullable: false),
                    DefaultAdults = table.Column<int>(type: "int", nullable: false),
                    MaximumAdults = table.Column<int>(type: "int", nullable: false),
                    MaximumChildren = table.Column<int>(type: "int", nullable: false),
                    BookingCurrency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    BookingReferencePrefix = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    BookingConfirmationBehavior = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    AdminNotificationIndicators = table.Column<bool>(type: "bit", nullable: false),
                    NotifyNewBooking = table.Column<bool>(type: "bit", nullable: false),
                    NotifyNewContact = table.Column<bool>(type: "bit", nullable: false),
                    NotifyNewFeedback = table.Column<bool>(type: "bit", nullable: false),
                    NotifyBookingStatusChange = table.Column<bool>(type: "bit", nullable: false),
                    NotificationEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    NotificationSubjectPrefix = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AdminTheme = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SidebarBehavior = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    InterfaceDensity = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DashboardDateRange = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TablePageSize = table.Column<int>(type: "int", nullable: false),
                    DashboardLayout = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationSettings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdminAuditLogs_Action",
                table: "AdminAuditLogs",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX_AdminAuditLogs_CreatedAt",
                table: "AdminAuditLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationSettings_Id",
                table: "ApplicationSettings",
                column: "Id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdminAuditLogs");

            migrationBuilder.DropTable(
                name: "ApplicationSettings");
        }
    }
}
