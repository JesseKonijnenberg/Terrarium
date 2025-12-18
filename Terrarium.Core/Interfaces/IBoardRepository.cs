using System.Collections.Generic;
using System.Threading.Tasks;
using Terrarium.Core.Models.Kanban;

namespace Terrarium.Core.Interfaces
{
    public interface IBoardRepository
    {
        Task<List<ColumnEntity>> LoadBoardAsync();
        Task AddTaskAsync(TaskEntity task, string columnId);
        Task UpdateTaskAsync(TaskEntity task);
        Task DeleteTaskAsync(string taskId);
        Task MoveTaskAsync(string taskId, string targetColumnId, int newIndex);
    }
}