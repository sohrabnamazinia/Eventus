using Microsoft.EntityFrameworkCore.Migrations;

namespace ArsamBackend.Migrations
{
    public partial class UserTaskManyToMany : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Tasks_TaskId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_TaskId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "TaskId",
                table: "AspNetUsers");

            migrationBuilder.CreateTable(
                name: "AppUserTask",
                columns: table => new
                {
                    AssignedMembersId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AssignedTasksId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppUserTask", x => new { x.AssignedMembersId, x.AssignedTasksId });
                    table.ForeignKey(
                        name: "FK_AppUserTask_AspNetUsers_AssignedMembersId",
                        column: x => x.AssignedMembersId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppUserTask_Tasks_AssignedTasksId",
                        column: x => x.AssignedTasksId,
                        principalTable: "Tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppUserTask_AssignedTasksId",
                table: "AppUserTask",
                column: "AssignedTasksId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppUserTask");

            migrationBuilder.AddColumn<int>(
                name: "TaskId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_TaskId",
                table: "AspNetUsers",
                column: "TaskId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Tasks_TaskId",
                table: "AspNetUsers",
                column: "TaskId",
                principalTable: "Tasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
