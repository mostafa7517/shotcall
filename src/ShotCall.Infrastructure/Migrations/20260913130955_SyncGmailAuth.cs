using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShotCall.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SyncGmailAuth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GmailAuthorizations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AdminUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    EncryptedRefreshToken = table.Column<string>(type: "text", nullable: false),
                    ConnectedEmail = table.Column<string>(type: "text", nullable: false),
                    ConnectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GmailAuthorizations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GmailAuthorizations_Users_AdminUserId",
                        column: x => x.AdminUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GmailAuthorizations_AdminUserId",
                table: "GmailAuthorizations",
                column: "AdminUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GmailAuthorizations");
        }
    }
}
