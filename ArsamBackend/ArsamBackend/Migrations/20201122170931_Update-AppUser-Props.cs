using Microsoft.EntityFrameworkCore.Migrations;

namespace ArsamBackend.Migrations
{
    public partial class UpdateAppUserProps : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UsersImage_UserId",
                table: "UsersImage");

            migrationBuilder.RenameColumn(
                name: "Bio",
                table: "AspNetUsers",
                newName: "Description");

            migrationBuilder.CreateIndex(
                name: "IX_UsersImage_UserId",
                table: "UsersImage",
                column: "UserId",
                unique: true,
                filter: "[UserId] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UsersImage_UserId",
                table: "UsersImage");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "AspNetUsers",
                newName: "Bio");

            migrationBuilder.CreateIndex(
                name: "IX_UsersImage_UserId",
                table: "UsersImage",
                column: "UserId");
        }
    }
}
