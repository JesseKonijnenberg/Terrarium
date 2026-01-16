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
    public async Task<KanbanBoardEntity?> GetBoardAsync(string workspaceId, string? projectId = null)
    {
        if (string.IsNullOrEmpty(projectId)) return null;
        
        var board = await _context.KanbanBoards
            .Include(b => b.Columns.OrderBy(c => c.Order))
            .FirstOrDefaultAsync(b => b.ProjectId == projectId);

        if (board == null) return null;
        
        var columnIds = board.Columns.Select(c => c.Id).ToList();
    
        var tasks = await _context.Tasks
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

        var iterationId = await _context.Columns
            .AsNoTracking()
            .Where(c => c.Id == columnId)
            .Select(c => c.KanbanBoard.CurrentIterationId)
            .FirstOrDefaultAsync();
    
        var maxOrder = await _context.Tasks
            .AsNoTracking()
            .Where(t => t.ColumnId == columnId && t.IterationId == iterationId)
            .MaxAsync(t => (int?)t.Order) ?? -1;

        task.ColumnId = columnId;
        task.IterationId = iterationId;
        task.Order = maxOrder + 1;

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
    }

    /// <inheritdoc />
    public async Task DeleteAllTasksInIterationAsync(string projectId)
    {
        var board = await _context.KanbanBoards
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.ProjectId == projectId);

        if (board == null) return;

        // Scoped delete: Only touches tasks in the board's columns AND current iteration
        await _context.Tasks
            .Where(t => t.IterationId == board.CurrentIterationId &&
                        _context.Columns
                            .Where(c => c.KanbanBoardId == board.Id)
                            .Select(c => c.Id)
                            .Contains(t.ColumnId))
            .ExecuteDeleteAsync();
    }

    /// <inheritdoc />
    public async Task MoveTaskAsync(string taskId, string targetColumnId, int newOrder)
    {
        var task = await _context.Tasks.FindAsync(taskId);
        if (task == null) return;

        var siblings = await _context.Tasks
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

        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task MoveMultipleTasksAsync(List<string> taskIds, string targetColumnId, int startIndex)
    {
        var tasks = await _context.Tasks
            .Where(t => taskIds.Contains(t.Id))
            .ToListAsync();

        foreach (var task in tasks)
        {
            task.ColumnId = targetColumnId;
            task.Order = startIndex++;
        }

        await _context.SaveChangesAsync();
    }
}