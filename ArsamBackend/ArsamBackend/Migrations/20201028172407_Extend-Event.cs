using Microsoft.EntityFrameworkCore.Migrations;

namespace ArsamBackend.Migrations
{
    public partial class ExtendEvent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatorAppUserId",
                table: "Events",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Events",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Events_CreatorAppUserId",
                table: "Events",
                column: "CreatorAppUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_AspNetUsers_CreatorAppUserId",
                table: "Events",
                column: "CreatorAppUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_AspNetUsers_CreatorAppUserId",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Events_CreatorAppUserId",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "CreatorAppUserId",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Events");
        }
    }
}
