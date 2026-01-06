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
        event EventHandler<BoardChangedEventsArgs> BoardChanged;
    }
}
