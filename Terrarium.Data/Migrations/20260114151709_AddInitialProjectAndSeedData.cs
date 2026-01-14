using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Terrarium.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddInitialProjectAndSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Projects",
                columns: new[] { "Id", "Description", "IsDeleted", "LastModifiedUtc", "Name", "WorkspaceId" },
                values: new object[] { "default-project", "", false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "General Tasks", "solo-workspace" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Projects",
                keyColumn: "Id",
                keyValue: "default-project");
        }
    }
}
