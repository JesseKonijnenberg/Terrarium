using Terrarium.Core.Models.Kanban;

namespace Terrarium.Core.Interfaces.Kanban
{
    public interface IBoardRepository
    {
        Task<List<ColumnEntity>> LoadBoardAsync();
        Task AddTaskAsync(TaskEntity task, string columnId);
        Task UpdateTaskAsync(TaskEntity task);
        Task DeleteTaskAsync(string taskId);
        Task DeleteTasksAsync(IEnumerable<string> taskIds);
        Task DeleteAllTasksAsync();
        Task MoveTaskAsync(string taskId, string targetColumnId, int newIndex);
        Task MoveMultipleTasksAsync(List<string> taskIds, string targetColumnId, int startIndex);
    }
}