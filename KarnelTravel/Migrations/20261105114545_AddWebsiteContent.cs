using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KarnelTravel.Migrations
{
    /// <inheritdoc />
    public partial class AddWebsiteContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WebsiteContents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SiteName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Tagline = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    LogoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    HomeHeroHeading = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    HomeHeroSubtitle = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    HomeHeroImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    HomeHeroButtonText = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AboutHeading = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AboutDescription = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: false),
                    Mission = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Vision = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ContactAddress = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    ContactPhone = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ContactEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    FooterDescription = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    FacebookUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    InstagramUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    YouTubeUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DefaultMetaTitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DefaultMetaDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebsiteContents", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WebsiteContents");
        }
    }
}
