using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ArsamBackend.Migrations
{
    public partial class joinRequestDate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsAccepted",
                table: "EventUserRole",
                newName: "IsDeleted");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfRequest",
                table: "EventUserRole",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "EventUserRole",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateOfRequest",
                table: "EventUserRole");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "EventUserRole");

            migrationBuilder.RenameColumn(
                name: "IsDeleted",
                table: "EventUserRole",
                newName: "IsAccepted");
        }
    }
}
