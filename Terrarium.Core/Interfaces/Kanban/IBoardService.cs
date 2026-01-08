using Terrarium.Core.Events.Kanban;
using Terrarium.Core.Models.Kanban;

namespace Terrarium.Core.Interfaces.Kanban
{
    public interface IBoardService
    {
        Task<List<ColumnEntity>> LoadBoardAsync();
        List<ColumnEntity> GetCachedBoard();

        Task MoveTaskAsync(TaskEntity task, string toColumnId, int index);
        Task AddTaskAsync(TaskEntity task, string toColumnId);
        Task DeleteTaskAsync(TaskEntity task);
        Task UpdateTaskAsync(TaskEntity task);
        Task<IEnumerable<TaskEntity>> ProcessSmartPasteAsync(string text);
        Task DeleteMultipleTasksAsync(IEnumerable<string> taskIds);
        Task WipeBoardAsync();
        Task MoveMultipleTasksAsync(IEnumerable<string> taskIds, string targetColumnId, int startIndex);
        Task<TaskEntity> CreateDefaultTaskEntity(string columnId);
        Task UpdateTaskFromUiAsync(TaskEntity entity, string title, string description, string tag, string priority, string date);

        Task MoveTasksWithEconomyAsync(List<string> taskIds, string targetColumnId, string targetColumnTitle, int index);
        event EventHandler<BoardChangedEventsArgs> BoardChanged;
    }
}
