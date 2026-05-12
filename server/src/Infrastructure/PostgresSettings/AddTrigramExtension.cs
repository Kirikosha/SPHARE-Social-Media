using Microsoft.EntityFrameworkCore.Migrations;

namespace Infrastructure.PostgresSettings;

public partial class AddTrigramExtension : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS pg_trgm;");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP EXTENSION IF EXISTS pg_trgm;");
    }
}