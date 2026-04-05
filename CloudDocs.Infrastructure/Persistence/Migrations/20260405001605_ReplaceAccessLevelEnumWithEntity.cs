using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CloudDocs.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceAccessLevelEnumWithEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccessLevel",
                table: "Documents");

            migrationBuilder.AddColumn<Guid>(
                name: "AccessLevelId",
                table: "Documents",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "AccessLevels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessLevels", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Documents_AccessLevelId",
                table: "Documents",
                column: "AccessLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_AccessLevels_Code",
                table: "AccessLevels",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccessLevels_Name",
                table: "AccessLevels",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_AccessLevels_AccessLevelId",
                table: "Documents",
                column: "AccessLevelId",
                principalTable: "AccessLevels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Documents_AccessLevels_AccessLevelId",
                table: "Documents");

            migrationBuilder.DropTable(
                name: "AccessLevels");

            migrationBuilder.DropIndex(
                name: "IX_Documents_AccessLevelId",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "AccessLevelId",
                table: "Documents");

            migrationBuilder.AddColumn<int>(
                name: "AccessLevel",
                table: "Documents",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
