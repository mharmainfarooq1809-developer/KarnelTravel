
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KarnelTravel.Migrations
{
    public partial class AddReviews : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Reviews",
                columns: table => new
                {
                    Id = table.Column<int>(
                        type: "int",
                        nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),

                    ReviewerName = table.Column<string>(
                        type: "nvarchar(100)",
                        maxLength: 100,
                        nullable: false),

                    Title = table.Column<string>(
                        type: "nvarchar(150)",
                        maxLength: 150,
                        nullable: false),

                    Comment = table.Column<string>(
                        type: "nvarchar(2000)",
                        maxLength: 2000,
                        nullable: false),

                    Rating = table.Column<int>(
                        type: "int",
                        nullable: false),

                    CreatedAt = table.Column<DateTime>(
                        type: "datetime2",
                        nullable: false,
                        defaultValueSql: "GETUTCDATE()"),

                    Status = table.Column<int>(
                        type: "int",
                        nullable: false),

                    DestinationId = table.Column<int>(
                        type: "int",
                        nullable: true),

                    HotelId = table.Column<int>(
                        type: "int",
                        nullable: true),

                    RestaurantId = table.Column<int>(
                        type: "int",
                        nullable: true),

                    ResortId = table.Column<int>(
                        type: "int",
                        nullable: true),

                    TransportServiceId = table.Column<int>(
                        type: "int",
                        nullable: true),

                    TourPackageId = table.Column<int>(
                        type: "int",
                        nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reviews", x => x.Id);

                    table.CheckConstraint(
                        "CK_Reviews_Rating",
                        "[Rating] >= 1 AND [Rating] <= 5");

                    table.CheckConstraint(
                        "CK_Reviews_ExactlyOneListing",
                        "(CASE WHEN [DestinationId] IS NOT NULL THEN 1 ELSE 0 END + " +
                        "CASE WHEN [HotelId] IS NOT NULL THEN 1 ELSE 0 END + " +
                        "CASE WHEN [RestaurantId] IS NOT NULL THEN 1 ELSE 0 END + " +
                        "CASE WHEN [ResortId] IS NOT NULL THEN 1 ELSE 0 END + " +
                        "CASE WHEN [TransportServiceId] IS NOT NULL THEN 1 ELSE 0 END + " +
                        "CASE WHEN [TourPackageId] IS NOT NULL THEN 1 ELSE 0 END) = 1");

                    table.ForeignKey(
                        name: "FK_Reviews_Destinations_DestinationId",
                        column: x => x.DestinationId,
                        principalTable: "Destinations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);

                    table.ForeignKey(
                        name: "FK_Reviews_Hotels_HotelId",
                        column: x => x.HotelId,
                        principalTable: "Hotels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);

                    table.ForeignKey(
                        name: "FK_Reviews_Restaurants_RestaurantId",
                        column: x => x.RestaurantId,
                        principalTable: "Restaurants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);

                    table.ForeignKey(
                        name: "FK_Reviews_Resorts_ResortId",
                        column: x => x.ResortId,
                        principalTable: "Resorts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);

                    table.ForeignKey(
                        name: "FK_Reviews_TransportServices_TransportServiceId",
                        column: x => x.TransportServiceId,
                        principalTable: "TransportServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);

                    table.ForeignKey(
                        name: "FK_Reviews_TourPackages_TourPackageId",
                        column: x => x.TourPackageId,
                        principalTable: "TourPackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_Status_CreatedAt",
                table: "Reviews",
                columns: new[] { "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_DestinationId",
                table: "Reviews",
                column: "DestinationId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_HotelId",
                table: "Reviews",
                column: "HotelId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_RestaurantId",
                table: "Reviews",
                column: "RestaurantId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_ResortId",
                table: "Reviews",
                column: "ResortId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_TransportServiceId",
                table: "Reviews",
                column: "TransportServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_TourPackageId",
                table: "Reviews",
                column: "TourPackageId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Reviews");
        }
    }
}
