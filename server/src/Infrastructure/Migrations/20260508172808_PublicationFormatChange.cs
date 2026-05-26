using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PublicationFormatChange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "WasSent",
                table: "Publications",
                newName: "WasPublished");

            migrationBuilder.RenameColumn(
                name: "RemindAt",
                table: "Publications",
                newName: "PublishAt");

            migrationBuilder.AlterColumn<string>(
                name: "PublicationType",
                table: "Publications",
                type: "character varying(13)",
                maxLength: 13,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "WasPublished",
                table: "Publications",
                newName: "WasSent");

            migrationBuilder.RenameColumn(
                name: "PublishAt",
                table: "Publications",
                newName: "RemindAt");

            migrationBuilder.AlterColumn<int>(
                name: "PublicationType",
                table: "Publications",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(13)",
                oldMaxLength: 13);
        }
    }
}
