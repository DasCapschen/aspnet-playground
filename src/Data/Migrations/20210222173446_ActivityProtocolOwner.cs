using Microsoft.EntityFrameworkCore.Migrations;

namespace src.Data.Migrations
{
    public partial class ActivityProtocolOwner : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                table: "ActivityProtocols",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ActivityProtocols_OwnerId",
                table: "ActivityProtocols",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_ActivityProtocols_AspNetUsers_OwnerId",
                table: "ActivityProtocols",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActivityProtocols_AspNetUsers_OwnerId",
                table: "ActivityProtocols");

            migrationBuilder.DropIndex(
                name: "IX_ActivityProtocols_OwnerId",
                table: "ActivityProtocols");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "ActivityProtocols");
        }
    }
}
