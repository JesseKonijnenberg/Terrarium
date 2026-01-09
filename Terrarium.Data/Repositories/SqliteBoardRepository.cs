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
    public async Task<List<ColumnEntity>> LoadBoardAsync()
    {
        var columns = await _context.Columns
            .Include(c => c.Tasks)
            .AsNoTracking()
            .ToListAsync();

        foreach (var col in columns)
        {
            col.Tasks = col.Tasks.OrderBy(t => t.Order).ToList();
        }

        return columns;
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
    public async Task MoveTaskAsync(string taskId, string targetColumnId, int newIndex)
    {
        var task = await _context.Tasks.FindAsync(taskId);
        if (task == null) return;

        task.ColumnId = targetColumnId;
        task.Order = newIndex;
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