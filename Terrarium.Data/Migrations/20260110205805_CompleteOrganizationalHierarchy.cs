using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Terrarium.Data.Migrations
{
    /// <inheritdoc />
    public partial class CompleteOrganizationalHierarchy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProjectId",
                table: "Tasks",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProjectId",
                table: "Columns",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkspaceId",
                table: "Columns",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Organizations",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    LastModifiedUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organizations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Workspaces",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    IsPersonal = table.Column<bool>(type: "INTEGER", nullable: false),
                    OrganizationId = table.Column<string>(type: "TEXT", nullable: true),
                    LastModifiedUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workspaces", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Workspaces_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    WorkspaceId = table.Column<string>(type: "TEXT", nullable: false),
                    LastModifiedUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Projects_Workspaces_WorkspaceId",
                        column: x => x.WorkspaceId,
                        principalTable: "Workspaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Iterations",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ProjectId = table.Column<string>(type: "TEXT", nullable: false),
                    LastModifiedUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Iterations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Iterations_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Columns",
                keyColumn: "Id",
                keyValue: "col-1",
                columns: new[] { "ProjectId", "WorkspaceId" },
                values: new object[] { null, "solo-workspace" });

            migrationBuilder.UpdateData(
                table: "Columns",
                keyColumn: "Id",
                keyValue: "col-2",
                columns: new[] { "ProjectId", "WorkspaceId" },
                values: new object[] { null, "solo-workspace" });

            migrationBuilder.UpdateData(
                table: "Columns",
                keyColumn: "Id",
                keyValue: "col-3",
                columns: new[] { "ProjectId", "WorkspaceId" },
                values: new object[] { null, "solo-workspace" });

            migrationBuilder.UpdateData(
                table: "Columns",
                keyColumn: "Id",
                keyValue: "col-4",
                columns: new[] { "ProjectId", "WorkspaceId" },
                values: new object[] { null, "solo-workspace" });

            migrationBuilder.InsertData(
                table: "Workspaces",
                columns: new[] { "Id", "IsDeleted", "IsPersonal", "LastModifiedUtc", "Name", "OrganizationId" },
                values: new object[] { "solo-workspace", false, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Personal Workspace", null });

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_IterationId",
                table: "Tasks",
                column: "IterationId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_ProjectId",
                table: "Tasks",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Columns_IterationId",
                table: "Columns",
                column: "IterationId");

            migrationBuilder.CreateIndex(
                name: "IX_Columns_ProjectId",
                table: "Columns",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Columns_WorkspaceId",
                table: "Columns",
                column: "WorkspaceId");

            migrationBuilder.CreateIndex(
                name: "IX_Iterations_ProjectId",
                table: "Iterations",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_WorkspaceId",
                table: "Projects",
                column: "WorkspaceId");

            migrationBuilder.CreateIndex(
                name: "IX_Workspaces_OrganizationId",
                table: "Workspaces",
                column: "OrganizationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Columns_Iterations_IterationId",
                table: "Columns",
                column: "IterationId",
                principalTable: "Iterations",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Columns_Projects_ProjectId",
                table: "Columns",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Columns_Workspaces_WorkspaceId",
                table: "Columns",
                column: "WorkspaceId",
                principalTable: "Workspaces",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Iterations_IterationId",
                table: "Tasks",
                column: "IterationId",
                principalTable: "Iterations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Projects_ProjectId",
                table: "Tasks",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Columns_Iterations_IterationId",
                table: "Columns");

            migrationBuilder.DropForeignKey(
                name: "FK_Columns_Projects_ProjectId",
                table: "Columns");

            migrationBuilder.DropForeignKey(
                name: "FK_Columns_Workspaces_WorkspaceId",
                table: "Columns");

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Iterations_IterationId",
                table: "Tasks");

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Projects_ProjectId",
                table: "Tasks");

            migrationBuilder.DropTable(
                name: "Iterations");

            migrationBuilder.DropTable(
                name: "Projects");

            migrationBuilder.DropTable(
                name: "Workspaces");

            migrationBuilder.DropTable(
                name: "Organizations");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_IterationId",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_ProjectId",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Columns_IterationId",
                table: "Columns");

            migrationBuilder.DropIndex(
                name: "IX_Columns_ProjectId",
                table: "Columns");

            migrationBuilder.DropIndex(
                name: "IX_Columns_WorkspaceId",
                table: "Columns");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "Columns");

            migrationBuilder.DropColumn(
                name: "WorkspaceId",
                table: "Columns");
        }
    }
}
