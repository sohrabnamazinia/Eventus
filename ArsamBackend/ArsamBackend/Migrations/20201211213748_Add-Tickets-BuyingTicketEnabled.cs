using Microsoft.EntityFrameworkCore.Migrations;

namespace ArsamBackend.Migrations
{
    public partial class AddTicketsBuyingTicketEnabled : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "BuyingTicketEnabled",
                table: "Events",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BuyingTicketEnabled",
                table: "Events");
        }
    }
}
