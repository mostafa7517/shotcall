using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShotCall.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SyncStaffTypesAndMatchSlots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Staff type: 0 = Photographer, 1 = Journalist, 2 = Tv
            migrationBuilder.AddColumn<int>(
                name: "StaffType",
                table: "Users",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StaffType",
                table: "MatchRequests",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StaffType",
                table: "WaitingListEntries",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "MatchSlots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffType = table.Column<int>(type: "integer", nullable: false),
                    Needed = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchSlots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MatchSlots_Matches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "Matches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MatchSlots_MatchId_StaffType",
                table: "MatchSlots",
                columns: new[] { "MatchId", "StaffType" },
                unique: true);

            // Existing data: every staff account so far was a photographer.
            migrationBuilder.Sql(@"UPDATE ""Users"" SET ""StaffType"" = 0 WHERE ""Role"" = 1 AND ""StaffType"" IS NULL;");

            // Existing matches: turn the old ""photographers needed"" number into a Photographer slot.
            migrationBuilder.Sql(@"
                INSERT INTO ""MatchSlots"" (""Id"", ""MatchId"", ""StaffType"", ""Needed"")
                SELECT gen_random_uuid(), ""Id"", 0, ""PhotographersNeeded""
                FROM ""Matches""
                WHERE ""PhotographersNeeded"" > 0;");

            migrationBuilder.DropColumn(
                name: "PhotographersNeeded",
                table: "Matches");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PhotographersNeeded",
                table: "Matches",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(@"
                UPDATE ""Matches"" m
                SET ""PhotographersNeeded"" = COALESCE((SELECT SUM(s.""Needed"") FROM ""MatchSlots"" s WHERE s.""MatchId"" = m.""Id""), 0);");

            migrationBuilder.DropTable(
                name: "MatchSlots");

            migrationBuilder.DropColumn(
                name: "StaffType",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "StaffType",
                table: "MatchRequests");

            migrationBuilder.DropColumn(
                name: "StaffType",
                table: "WaitingListEntries");
        }
    }
}
