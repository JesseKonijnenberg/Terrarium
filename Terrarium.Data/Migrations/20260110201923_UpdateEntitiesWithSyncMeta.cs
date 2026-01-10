using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Terrarium.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEntitiesWithSyncMeta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Tasks",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "IterationId",
                table: "Tasks",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedUtc",
                table: "Tasks",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Columns",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "IterationId",
                table: "Columns",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedUtc",
                table: "Columns",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "Columns",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Columns",
                keyColumn: "Id",
                keyValue: "col-1",
                columns: new[] { "IsDeleted", "IterationId", "LastModifiedUtc", "Order" },
                values: new object[] { false, null, new DateTime(2026, 1, 10, 20, 19, 23, 368, DateTimeKind.Utc).AddTicks(4140), 0 });

            migrationBuilder.UpdateData(
                table: "Columns",
                keyColumn: "Id",
                keyValue: "col-2",
                columns: new[] { "IsDeleted", "IterationId", "LastModifiedUtc", "Order" },
                values: new object[] { false, null, new DateTime(2026, 1, 10, 20, 19, 23, 368, DateTimeKind.Utc).AddTicks(4683), 0 });

            migrationBuilder.UpdateData(
                table: "Columns",
                keyColumn: "Id",
                keyValue: "col-3",
                columns: new[] { "IsDeleted", "IterationId", "LastModifiedUtc", "Order" },
                values: new object[] { false, null, new DateTime(2026, 1, 10, 20, 19, 23, 368, DateTimeKind.Utc).AddTicks(4695), 0 });

            migrationBuilder.UpdateData(
                table: "Columns",
                keyColumn: "Id",
                keyValue: "col-4",
                columns: new[] { "IsDeleted", "IterationId", "LastModifiedUtc", "Order" },
                values: new object[] { false, null, new DateTime(2026, 1, 10, 20, 19, 23, 368, DateTimeKind.Utc).AddTicks(4705), 0 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "IterationId",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "LastModifiedUtc",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Columns");

            migrationBuilder.DropColumn(
                name: "IterationId",
                table: "Columns");

            migrationBuilder.DropColumn(
                name: "LastModifiedUtc",
                table: "Columns");

            migrationBuilder.DropColumn(
                name: "Order",
                table: "Columns");
        }
    }
}
