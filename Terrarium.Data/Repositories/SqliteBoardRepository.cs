using Microsoft.EntityFrameworkCore;
using Terrarium.Core.Interfaces.Kanban;
using Terrarium.Core.Models.Kanban;
using Terrarium.Data.Contexts;

namespace Terrarium.Data.Repositories;

public class SqliteBoardRepository : IBoardRepository
{
    private readonly TerrariumDbContext _context;

    public SqliteBoardRepository(TerrariumDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<List<ColumnEntity>> LoadBoardAsync(string workspaceId, string? projectId = null)
    {
        var board = await _context.Columns
            .Where(c => c.WorkspaceId == workspaceId && c.ProjectId == projectId)
            .Include(c => c.Tasks)
            .OrderBy(c => c.Order)
            .ToListAsync();

        // Explicitly sort tasks in memory to guarantee the order for the UI/Tests
        foreach (var column in board)
        {
            column.Tasks = column.Tasks.OrderBy(t => t.Order).ToList();
        }

        return board;
    }

    /// <inheritdoc />
    public async Task AddTaskAsync(TaskEntity task, string columnId)
    {
        task.ColumnId = columnId;
        task.Order = 0;
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task UpdateTaskAsync(TaskEntity incomingTask)
    {
        var existingTask = await _context.Tasks.FindAsync(incomingTask.Id);
        if (existingTask != null)
        {
            existingTask.Title = incomingTask.Title;
            existingTask.Description = incomingTask.Description;
            existingTask.Tag = incomingTask.Tag;
            existingTask.Priority = incomingTask.Priority;
            existingTask.DueDate = incomingTask.DueDate;
            await _context.SaveChangesAsync();
        }
    }

    /// <inheritdoc />
    public async Task DeleteTaskAsync(string taskId)
    {
        var task = await _context.Tasks.FindAsync(taskId);
        if (task != null)
        {
            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
        }
    }

    /// <inheritdoc />
    public async Task DeleteTasksAsync(IEnumerable<string> taskIds)
    {
        await _context.Tasks
            .Where(t => taskIds.Contains(t.Id))
            .ExecuteDeleteAsync();
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task DeleteAllTasksAsync()
    {
        await _context.Tasks.ExecuteDeleteAsync();
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task MoveTaskAsync(string taskId, string targetColumnId, int newOrder)
    {
        var task = await _context.Tasks.FindAsync(taskId);
        if (task == null) return;

        // Shift all existing tasks in the target column that are at or below the new index
        var siblings = await _context.Tasks
            .Where(t => t.ColumnId == targetColumnId && t.Order >= newOrder && t.Id != taskId)
            .ToListAsync();

        foreach (var sibling in siblings)
        {
            sibling.Order++; // Make room
        }

        // Assign the new position
        task.ColumnId = targetColumnId;
        task.Order = newOrder;

        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task MoveMultipleTasksAsync(List<string> taskIds, string targetColumnId, int startIndex)
    {
        foreach (var id in taskIds)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task != null)
            {
                task.ColumnId = targetColumnId;
                task.Order = startIndex++;
            }
        }
        await _context.SaveChangesAsync();
    }
}