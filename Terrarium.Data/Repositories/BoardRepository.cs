using Microsoft.EntityFrameworkCore;
using Terrarium.Core.Interfaces.Kanban;
using Terrarium.Core.Models.Kanban;
using Terrarium.Data.Contexts;

namespace Terrarium.Data.Repositories;

public class BoardRepository : IBoardRepository
{
    private readonly IDbContextFactory<TerrariumDbContext> _contextFactory;

    public BoardRepository(IDbContextFactory<TerrariumDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    /// <inheritdoc />
    public async Task<KanbanBoardEntity?> GetBoardAsync(string workspaceId, string? projectId = null)
    {
        if (string.IsNullOrEmpty(projectId)) return null;

        await using var context = await _contextFactory.CreateDbContextAsync();
        var board = await context.KanbanBoards
            .AsNoTracking()
            .Include(b => b.Columns.OrderBy(c => c.Order))
            .FirstOrDefaultAsync(b => b.ProjectId == projectId);

        if (board == null) return null;

        var columnIds = board.Columns.Select(c => c.Id).ToList();

        var tasks = await context.Tasks
            .AsNoTracking()
            .Where(t => columnIds.Contains(t.ColumnId))
            .Where(t => string.IsNullOrEmpty(board.CurrentIterationId) || t.IterationId == board.CurrentIterationId)
            .OrderBy(t => t.Order)
            .ToListAsync();

        foreach (var column in board.Columns)
        {
            column.Tasks = tasks.Where(t => t.ColumnId == column.Id).ToList();
        }

        return board;
    }

    /// <inheritdoc />
    public async Task AddTaskAsync(TaskEntity task, string columnId)
    {
        if (task == null) throw new ArgumentNullException(nameof(task));

        await using var context = await _contextFactory.CreateDbContextAsync();
        var iterationId = await context.Columns
            .AsNoTracking()
            .Where(c => c.Id == columnId)
            .Select(c => c.KanbanBoard.CurrentIterationId)
            .FirstOrDefaultAsync();

        var maxOrder = await context.Tasks
            .AsNoTracking()
            .Where(t => t.ColumnId == columnId && t.IterationId == iterationId)
            .MaxAsync(t => (int?)t.Order) ?? -1;

        task.ColumnId = columnId;
        task.IterationId = iterationId;
        task.Order = maxOrder + 1;
        
        // Prevent disconnected graph errors
        task.Column = null!;

        context.Tasks.Add(task);
        await context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task UpdateTaskAsync(TaskEntity incomingTask)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var existingTask = await context.Tasks.FindAsync(incomingTask.Id);
        if (existingTask != null)
        {
            existingTask.Title = incomingTask.Title;
            existingTask.Description = incomingTask.Description;
            existingTask.Tag = incomingTask.Tag;
            existingTask.Priority = incomingTask.Priority;
            existingTask.DueDate = incomingTask.DueDate;

            await context.SaveChangesAsync();
        }
    }

    /// <inheritdoc />
    public async Task DeleteTaskAsync(string taskId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var task = await context.Tasks.FindAsync(taskId);
        if (task != null)
        {
            context.Tasks.Remove(task);
            await context.SaveChangesAsync();
        }
    }

    /// <inheritdoc />
    public async Task DeleteTasksAsync(IEnumerable<string> taskIds)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        await context.Tasks
            .Where(t => taskIds.Contains(t.Id))
            .ExecuteDeleteAsync();
    }

    /// <inheritdoc />
    public async Task DeleteAllTasksInIterationAsync(string projectId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var board = await context.KanbanBoards
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.ProjectId == projectId);

        if (board == null) return;

        // Scoped delete: Only touches tasks in the board's columns AND current iteration
        var columnIds = await context.Columns
            .Where(c => c.KanbanBoardId == board.Id)
            .Select(c => c.Id)
            .ToListAsync();

        await context.Tasks
            .Where(t => t.IterationId == board.CurrentIterationId &&
                        columnIds.Contains(t.ColumnId))
            .ExecuteDeleteAsync();
    }

    /// <inheritdoc />
    public async Task MoveTaskAsync(string taskId, string targetColumnId, int newOrder)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var task = await context.Tasks.FindAsync(taskId);
        if (task == null) return;

        var siblings = await context.Tasks
            .Where(t => t.ColumnId == targetColumnId
                        && t.IterationId == task.IterationId
                        && t.Order >= newOrder
                        && t.Id != taskId)
            .ToListAsync();

        foreach (var sibling in siblings)
        {
            sibling.Order++;
        }

        task.ColumnId = targetColumnId;
        task.Order = newOrder;

        await context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task MoveMultipleTasksAsync(List<string> taskIds, string targetColumnId, int startIndex)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var tasks = await context.Tasks
            .Where(t => taskIds.Contains(t.Id))
            .ToListAsync();

        foreach (var task in tasks)
        {
            task.ColumnId = targetColumnId;
            task.Order = startIndex++;
        }

        await context.SaveChangesAsync();
    }

    public async Task CreateBoardAsync(KanbanBoardEntity board)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        // Prevent disconnected graph errors
        board.Project = null!;
        
        context.KanbanBoards.Add(board);
        await context.SaveChangesAsync();
    }

}