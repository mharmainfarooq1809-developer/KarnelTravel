using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KarnelTravel.Migrations;

public partial class AddCustomTripRequestOwnership : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>("AdminNotes", "TripInquiries", type: "nvarchar(2000)", maxLength: 2000, nullable: true);
        migrationBuilder.AddColumn<string>("OwnerEmail", "TripInquiries", type: "nvarchar(256)", maxLength: 256, nullable: true);
        migrationBuilder.AddColumn<string>("ProposedOffer", "TripInquiries", type: "nvarchar(2000)", maxLength: 2000, nullable: true);
        migrationBuilder.CreateIndex(name: "IX_TripInquiries_OwnerEmail", table: "TripInquiries", column: "OwnerEmail");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "IX_TripInquiries_OwnerEmail", table: "TripInquiries");
        migrationBuilder.DropColumn("AdminNotes", "TripInquiries");
        migrationBuilder.DropColumn("OwnerEmail", "TripInquiries");
        migrationBuilder.DropColumn("ProposedOffer", "TripInquiries");
    }
}
