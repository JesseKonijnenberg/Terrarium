using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Terrarium.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddKanbanPluginHierarchy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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
                name: "FK_Tasks_Workspaces_WorkspaceId",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_WorkspaceId",
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
                name: "WorkspaceId",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "IterationId",
                table: "Columns");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "Columns");

            migrationBuilder.DropColumn(
                name: "WorkspaceId",
                table: "Columns");

            migrationBuilder.AddColumn<string>(
                name: "KanbanBoardId",
                table: "Columns",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "KanbanBoards",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<string>(type: "TEXT", nullable: false),
                    CurrentIterationId = table.Column<string>(type: "TEXT", nullable: true),
                    LastModifiedUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KanbanBoards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KanbanBoards_Iterations_CurrentIterationId",
                        column: x => x.CurrentIterationId,
                        principalTable: "Iterations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_KanbanBoards_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Columns",
                keyColumn: "Id",
                keyValue: "col-1",
                column: "KanbanBoardId",
                value: "main-board");

            migrationBuilder.UpdateData(
                table: "Columns",
                keyColumn: "Id",
                keyValue: "col-2",
                column: "KanbanBoardId",
                value: "main-board");

            migrationBuilder.UpdateData(
                table: "Columns",
                keyColumn: "Id",
                keyValue: "col-3",
                column: "KanbanBoardId",
                value: "main-board");

            migrationBuilder.UpdateData(
                table: "Columns",
                keyColumn: "Id",
                keyValue: "col-4",
                column: "KanbanBoardId",
                value: "main-board");

            migrationBuilder.InsertData(
                table: "KanbanBoards",
                columns: new[] { "Id", "CurrentIterationId", "IsDeleted", "LastModifiedUtc", "Name", "ProjectId" },
                values: new object[] { "main-board", null, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Development Board", "default-project" });

            migrationBuilder.CreateIndex(
                name: "IX_Columns_KanbanBoardId",
                table: "Columns",
                column: "KanbanBoardId");

            migrationBuilder.CreateIndex(
                name: "IX_KanbanBoards_CurrentIterationId",
                table: "KanbanBoards",
                column: "CurrentIterationId");

            migrationBuilder.CreateIndex(
                name: "IX_KanbanBoards_ProjectId",
                table: "KanbanBoards",
                column: "ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_Columns_KanbanBoards_KanbanBoardId",
                table: "Columns",
                column: "KanbanBoardId",
                principalTable: "KanbanBoards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Iterations_IterationId",
                table: "Tasks",
                column: "IterationId",
                principalTable: "Iterations",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Columns_KanbanBoards_KanbanBoardId",
                table: "Columns");

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Iterations_IterationId",
                table: "Tasks");

            migrationBuilder.DropTable(
                name: "KanbanBoards");

            migrationBuilder.DropIndex(
                name: "IX_Columns_KanbanBoardId",
                table: "Columns");

            migrationBuilder.DropColumn(
                name: "KanbanBoardId",
                table: "Columns");

            migrationBuilder.AddColumn<string>(
                name: "WorkspaceId",
                table: "Tasks",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IterationId",
                table: "Columns",
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

            migrationBuilder.UpdateData(
                table: "Columns",
                keyColumn: "Id",
                keyValue: "col-1",
                columns: new[] { "IterationId", "ProjectId", "WorkspaceId" },
                values: new object[] { null, null, "solo-workspace" });

            migrationBuilder.UpdateData(
                table: "Columns",
                keyColumn: "Id",
                keyValue: "col-2",
                columns: new[] { "IterationId", "ProjectId", "WorkspaceId" },
                values: new object[] { null, null, "solo-workspace" });

            migrationBuilder.UpdateData(
                table: "Columns",
                keyColumn: "Id",
                keyValue: "col-3",
                columns: new[] { "IterationId", "ProjectId", "WorkspaceId" },
                values: new object[] { null, null, "solo-workspace" });

            migrationBuilder.UpdateData(
                table: "Columns",
                keyColumn: "Id",
                keyValue: "col-4",
                columns: new[] { "IterationId", "ProjectId", "WorkspaceId" },
                values: new object[] { null, null, "solo-workspace" });

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_WorkspaceId",
                table: "Tasks",
                column: "WorkspaceId");

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
                name: "FK_Tasks_Workspaces_WorkspaceId",
                table: "Tasks",
                column: "WorkspaceId",
                principalTable: "Workspaces",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
