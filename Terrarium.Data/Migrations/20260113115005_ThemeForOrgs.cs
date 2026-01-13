using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Terrarium.Data.Migrations
{
    /// <inheritdoc />
    public partial class ThemeForOrgs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ActiveThemeId",
                table: "Organizations",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "LockTheme",
                table: "Organizations",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActiveThemeId",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "LockTheme",
                table: "Organizations");
        }
    }
}
