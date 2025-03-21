using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KıbrısApp3.Migrations
{
    /// <inheritdoc />
    public partial class AddUserToAdListing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_AdListings_UserId",
                table: "AdListings",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AdListings_AspNetUsers_UserId",
                table: "AdListings",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdListings_AspNetUsers_UserId",
                table: "AdListings");

            migrationBuilder.DropIndex(
                name: "IX_AdListings_UserId",
                table: "AdListings");
        }
    }
}
