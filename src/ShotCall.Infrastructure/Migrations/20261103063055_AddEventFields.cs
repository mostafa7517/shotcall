using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShotCall.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEventFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HostName",
                table: "Matches",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsEvent",
                table: "Matches",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HostName",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "IsEvent",
                table: "Matches");
        }
    }
}
