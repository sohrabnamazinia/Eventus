using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ArsamBackend.Migrations
{
    public partial class CompleteStructureEvent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Events",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "Events",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "EventMembersEmail",
                table: "Events",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImagesFilePath",
                table: "Events",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsLimitedMember",
                table: "Events",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsProject",
                table: "Events",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MaximumNumberOfMembers",
                table: "Events",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "Events",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "EventMembersEmail",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "ImagesFilePath",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "IsLimitedMember",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "IsProject",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "MaximumNumberOfMembers",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "Events");

            migrationBuilder.AddColumn<string>(
                name: "CreatorAppUserId",
                table: "Events",
                type: "nvarchar(450)",
                nullable: true);

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
    }
}
