using Microsoft.EntityFrameworkCore;
using Terrarium.Core.Interfaces.Kanban;
using Terrarium.Core.Models.Kanban;
using Terrarium.Data.Contexts;

namespace Terrarium.Data
{
    public class SqliteBoardRepository : IBoardRepository
    {
        private readonly TerrariumDbContext _context;

        public SqliteBoardRepository(TerrariumDbContext context)
        {
            _context = context;
        }

        public async Task<List<ColumnEntity>> LoadBoardAsync()
        {
            // Include(c => c.Tasks) tells SQL to "JOIN" the tables 
            // so we get the tasks inside the columns.
            var columns = await _context.Columns
                .Include(c => c.Tasks)
                .AsNoTracking() // Optimization for read-only lists
                .ToListAsync();

            // SQL doesn't guarantee list order, so we sort by our Order index
            foreach (var col in columns)
            {
                col.Tasks = col.Tasks.OrderBy(t => t.Order).ToList();
            }

            return columns;
        }

        public async Task AddTaskAsync(TaskEntity task, string columnId)
        {
            task.ColumnId = columnId; // Link it via FK
            task.Order = 0;           // Put at top

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
        }

        public async Task MoveTaskAsync(string taskId, string targetColumnId, int newIndex)
        {
            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null) return;

            // Simple move logic
            task.ColumnId = targetColumnId;
            task.Order = newIndex;

            // Note: A perfect implementation would also re-index the other tasks 
            // in the column (e.g. shift them down), but this works for basic persistence.

            await _context.SaveChangesAsync();
        }

        public async Task DeleteTaskAsync(string taskId)
        {
            var task = await _context.Tasks.FindAsync(taskId);
            if (task != null)
            {
                _context.Tasks.Remove(task);
                await _context.SaveChangesAsync();
            }
        }
        
        public async Task DeleteTasksAsync(IEnumerable<string> taskIds)
        {
            await _context.Tasks
                .Where(t => taskIds.Contains(t.Id))
                .ExecuteDeleteAsync();
        
            await _context.SaveChangesAsync();
        }
        
        public async Task DeleteAllTasksAsync()
        {
            await _context.Tasks.ExecuteDeleteAsync();
            await _context.SaveChangesAsync();
        }

        public async Task UpdateTaskAsync(TaskEntity incomingTask)
        {
            try 
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
            catch (DbUpdateConcurrencyException ex)
            {
                Console.WriteLine($"[DbUpdateConcurrencyException]: {ex.Message}");
            }
        }
        
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
}