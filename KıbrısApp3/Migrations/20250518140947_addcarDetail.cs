using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace KıbrısApp3.Migrations
{
    /// <inheritdoc />
    public partial class addcarDetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BodyType",
                table: "CarAdDetails",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "HorsePower",
                table: "CarAdDetails",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Kilometre",
                table: "CarAdDetails",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "MotorcycleAdDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AdListingId = table.Column<int>(type: "integer", nullable: false),
                    Brand = table.Column<string>(type: "text", nullable: false),
                    Model = table.Column<string>(type: "text", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    Kilometre = table.Column<int>(type: "integer", nullable: false),
                    HorsePower = table.Column<int>(type: "integer", nullable: false),
                    EngineSize = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MotorcycleAdDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MotorcycleAdDetails_AdListings_AdListingId",
                        column: x => x.AdListingId,
                        principalTable: "AdListings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MotorcycleAdDetails_AdListingId",
                table: "MotorcycleAdDetails",
                column: "AdListingId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MotorcycleAdDetails");

            migrationBuilder.DropColumn(
                name: "BodyType",
                table: "CarAdDetails");

            migrationBuilder.DropColumn(
                name: "HorsePower",
                table: "CarAdDetails");

            migrationBuilder.DropColumn(
                name: "Kilometre",
                table: "CarAdDetails");
        }
    }
}
