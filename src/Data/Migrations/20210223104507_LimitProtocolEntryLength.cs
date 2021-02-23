using Microsoft.EntityFrameworkCore.Migrations;

namespace src.Data.Migrations
{
    public partial class LimitProtocolEntryLength : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActivityProtocols_AspNetUsers_OwnerId",
                table: "ActivityProtocols");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "ProtocolEntries",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OwnerId",
                table: "ActivityProtocols",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ActivityProtocols_AspNetUsers_OwnerId",
                table: "ActivityProtocols",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActivityProtocols_AspNetUsers_OwnerId",
                table: "ActivityProtocols");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "ProtocolEntries",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OwnerId",
                table: "ActivityProtocols",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddForeignKey(
                name: "FK_ActivityProtocols_AspNetUsers_OwnerId",
                table: "ActivityProtocols",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
