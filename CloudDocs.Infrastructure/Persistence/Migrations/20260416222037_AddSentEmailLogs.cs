using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CloudDocs.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSentEmailLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SentEmailLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SentByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: true),
                    RecipientEmail = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Subject = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SentEmailLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SentEmailLogs_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SentEmailLogs_Users_SentByUserId",
                        column: x => x.SentByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SentEmailLogs_DocumentId",
                table: "SentEmailLogs",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_SentEmailLogs_SentAt",
                table: "SentEmailLogs",
                column: "SentAt");

            migrationBuilder.CreateIndex(
                name: "IX_SentEmailLogs_SentByUserId",
                table: "SentEmailLogs",
                column: "SentByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SentEmailLogs");
        }
    }
}
