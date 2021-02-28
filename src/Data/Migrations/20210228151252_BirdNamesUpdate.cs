using Microsoft.EntityFrameworkCore.Migrations;

namespace src.Data.Migrations
{
    public partial class BirdNamesUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GbifId",
                table: "BirdNames",
                type: "integer",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GbifId",
                table: "BirdNames");
        }
    }
}
