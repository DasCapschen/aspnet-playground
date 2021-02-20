using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace src.Data.Migrations
{
    public partial class activityprotocol : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ActivityProtocols",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Date = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    JournalEntry = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityProtocols", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProtocolEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Time = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    ProtocolId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProtocolEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProtocolEntries_ActivityProtocols_ProtocolId",
                        column: x => x.ProtocolId,
                        principalTable: "ActivityProtocols",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProtocolEntries_ProtocolId",
                table: "ProtocolEntries",
                column: "ProtocolId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProtocolEntries");

            migrationBuilder.DropTable(
                name: "ActivityProtocols");
        }
    }
}
