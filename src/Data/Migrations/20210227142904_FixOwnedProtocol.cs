using Microsoft.EntityFrameworkCore.Migrations;

namespace src.Data.Migrations
{
    public partial class FixOwnedProtocol : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActivityProtocols_AspNetUsers_OwnerId",
                table: "ActivityProtocols");

            migrationBuilder.DropForeignKey(
                name: "FK_ProtocolEntries_ActivityProtocols_ProtocolId",
                table: "ProtocolEntries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProtocolEntries",
                table: "ProtocolEntries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ActivityProtocols",
                table: "ActivityProtocols");

            migrationBuilder.RenameTable(
                name: "ProtocolEntries",
                newName: "ProtocolEntry");

            migrationBuilder.RenameTable(
                name: "ActivityProtocols",
                newName: "ActivityProtocol");

            migrationBuilder.RenameIndex(
                name: "IX_ProtocolEntries_ProtocolId",
                table: "ProtocolEntry",
                newName: "IX_ProtocolEntry_ProtocolId");

            migrationBuilder.RenameIndex(
                name: "IX_ActivityProtocols_OwnerId",
                table: "ActivityProtocol",
                newName: "IX_ActivityProtocol_OwnerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProtocolEntry",
                table: "ProtocolEntry",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ActivityProtocol",
                table: "ActivityProtocol",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ActivityProtocol_AspNetUsers_OwnerId",
                table: "ActivityProtocol",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProtocolEntry_ActivityProtocol_ProtocolId",
                table: "ProtocolEntry",
                column: "ProtocolId",
                principalTable: "ActivityProtocol",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActivityProtocol_AspNetUsers_OwnerId",
                table: "ActivityProtocol");

            migrationBuilder.DropForeignKey(
                name: "FK_ProtocolEntry_ActivityProtocol_ProtocolId",
                table: "ProtocolEntry");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProtocolEntry",
                table: "ProtocolEntry");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ActivityProtocol",
                table: "ActivityProtocol");

            migrationBuilder.RenameTable(
                name: "ProtocolEntry",
                newName: "ProtocolEntries");

            migrationBuilder.RenameTable(
                name: "ActivityProtocol",
                newName: "ActivityProtocols");

            migrationBuilder.RenameIndex(
                name: "IX_ProtocolEntry_ProtocolId",
                table: "ProtocolEntries",
                newName: "IX_ProtocolEntries_ProtocolId");

            migrationBuilder.RenameIndex(
                name: "IX_ActivityProtocol_OwnerId",
                table: "ActivityProtocols",
                newName: "IX_ActivityProtocols_OwnerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProtocolEntries",
                table: "ProtocolEntries",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ActivityProtocols",
                table: "ActivityProtocols",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ActivityProtocols_AspNetUsers_OwnerId",
                table: "ActivityProtocols",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProtocolEntries_ActivityProtocols_ProtocolId",
                table: "ProtocolEntries",
                column: "ProtocolId",
                principalTable: "ActivityProtocols",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
