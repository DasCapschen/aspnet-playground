using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace src.Data.Migrations
{
    public partial class UserBirdData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BirdNames",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    de = table.Column<string>(type: "text", nullable: true),
                    latin = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BirdNames", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "UserActiveBirds",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BirdId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserActiveBirds", x => new { x.UserId, x.Id });
                    table.ForeignKey(
                        name: "FK_UserActiveBirds_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserActiveBirds_BirdNames_BirdId",
                        column: x => x.BirdId,
                        principalTable: "BirdNames",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserBirdStats",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BirdId = table.Column<int>(type: "integer", nullable: false),
                    AnswersCorrect = table.Column<int>(type: "integer", nullable: false),
                    AnswersWrong = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserBirdStats", x => new { x.UserId, x.Id });
                    table.ForeignKey(
                        name: "FK_UserBirdStats_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserBirdStats_BirdNames_BirdId",
                        column: x => x.BirdId,
                        principalTable: "BirdNames",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserActiveBirds_BirdId",
                table: "UserActiveBirds",
                column: "BirdId");

            migrationBuilder.CreateIndex(
                name: "IX_UserBirdStats_BirdId",
                table: "UserBirdStats",
                column: "BirdId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserActiveBirds");

            migrationBuilder.DropTable(
                name: "UserBirdStats");

            migrationBuilder.DropTable(
                name: "BirdNames");
        }
    }
}
