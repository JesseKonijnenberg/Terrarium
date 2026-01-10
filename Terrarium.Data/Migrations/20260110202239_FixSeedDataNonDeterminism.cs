using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Terrarium.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixSeedDataNonDeterminism : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Columns",
                keyColumn: "Id",
                keyValue: "col-1",
                column: "LastModifiedUtc",
                value: new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "Columns",
                keyColumn: "Id",
                keyValue: "col-2",
                columns: new[] { "LastModifiedUtc", "Order" },
                values: new object[] { new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1 });

            migrationBuilder.UpdateData(
                table: "Columns",
                keyColumn: "Id",
                keyValue: "col-3",
                columns: new[] { "LastModifiedUtc", "Order" },
                values: new object[] { new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2 });

            migrationBuilder.UpdateData(
                table: "Columns",
                keyColumn: "Id",
                keyValue: "col-4",
                columns: new[] { "LastModifiedUtc", "Order" },
                values: new object[] { new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Columns",
                keyColumn: "Id",
                keyValue: "col-1",
                column: "LastModifiedUtc",
                value: new DateTime(2026, 1, 10, 20, 19, 23, 368, DateTimeKind.Utc).AddTicks(4140));

            migrationBuilder.UpdateData(
                table: "Columns",
                keyColumn: "Id",
                keyValue: "col-2",
                columns: new[] { "LastModifiedUtc", "Order" },
                values: new object[] { new DateTime(2026, 1, 10, 20, 19, 23, 368, DateTimeKind.Utc).AddTicks(4683), 0 });

            migrationBuilder.UpdateData(
                table: "Columns",
                keyColumn: "Id",
                keyValue: "col-3",
                columns: new[] { "LastModifiedUtc", "Order" },
                values: new object[] { new DateTime(2026, 1, 10, 20, 19, 23, 368, DateTimeKind.Utc).AddTicks(4695), 0 });

            migrationBuilder.UpdateData(
                table: "Columns",
                keyColumn: "Id",
                keyValue: "col-4",
                columns: new[] { "LastModifiedUtc", "Order" },
                values: new object[] { new DateTime(2026, 1, 10, 20, 19, 23, 368, DateTimeKind.Utc).AddTicks(4705), 0 });
        }
    }
}
