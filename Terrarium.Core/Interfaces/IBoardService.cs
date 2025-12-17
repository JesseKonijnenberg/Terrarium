using Terrarium.Core.Models;

namespace Terrarium.Core.Interfaces
{
    public interface IBoardService
    {
        List<ColumnEntity> GetFullBoard();

        Task MoveTaskAsync(TaskEntity task, string toColumnId, int index);
        Task AddTaskAsync(TaskEntity task, string toColumnId);
        Task DeleteTaskAsync(TaskEntity task);
    }
}
