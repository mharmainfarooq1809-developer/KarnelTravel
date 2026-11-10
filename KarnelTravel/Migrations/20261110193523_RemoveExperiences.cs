using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KarnelTravel.Migrations
{
    /// <inheritdoc />
    public partial class RemoveExperiences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Experiences");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Experiences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Category = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    Duration = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Excludes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Includes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsFeatured = table.Column<bool>(type: "bit", nullable: false),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ShortDescription = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    StartingPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Experiences", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Experiences_Category",
                table: "Experiences",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Experiences_IsPublished_DisplayOrder",
                table: "Experiences",
                columns: new[] { "IsPublished", "DisplayOrder" });
        }
    }
}
