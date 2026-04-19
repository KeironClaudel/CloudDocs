using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CloudDocs.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class OptimizeDocumentQueriesAndStorage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Documents_ClientId",
                table: "Documents");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,");

            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS pg_trgm;");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_CategoryId_IsActive_CreatedAt",
                table: "Documents",
                columns: new[] { "CategoryId", "IsActive", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Documents_ClientId_IsActive_CreatedAt",
                table: "Documents",
                columns: new[] { "ClientId", "IsActive", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Documents_DocumentTypeId_IsActive_CreatedAt",
                table: "Documents",
                columns: new[] { "DocumentTypeId", "IsActive", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Documents_IsActive_CreatedAt",
                table: "Documents",
                columns: new[] { "IsActive", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Documents_Year_Month_IsActive_CreatedAt",
                table: "Documents",
                columns: new[] { "Year", "Month", "IsActive", "CreatedAt" });

            migrationBuilder.Sql(
                """
                CREATE INDEX IF NOT EXISTS "IX_Documents_OriginalFileName_Trgm"
                ON "Documents"
                USING GIN ("OriginalFileName" gin_trgm_ops);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Documents_CategoryId_IsActive_CreatedAt",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Documents_ClientId_IsActive_CreatedAt",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Documents_DocumentTypeId_IsActive_CreatedAt",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Documents_IsActive_CreatedAt",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Documents_Year_Month_IsActive_CreatedAt",
                table: "Documents");

            migrationBuilder.Sql("""
                DROP INDEX IF EXISTS "IX_Documents_OriginalFileName_Trgm";
                """);

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:pg_trgm", ",,");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_ClientId",
                table: "Documents",
                column: "ClientId");
        }
    }
}
