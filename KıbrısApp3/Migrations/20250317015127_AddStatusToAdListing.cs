using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KıbrısApp3.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusToAdListing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "AdListings",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "AdListings");
        }
    }
}
