using Terrarium.Core.Models;

namespace Terrarium.Core.Interfaces
{
    public interface IBoardService
    {
        void AddTaskToColumn(ColumnEntity column, TaskEntity task);
        void RemoveTaskFromColumn(ColumnEntity column, TaskEntity task);
        List<ColumnEntity> GetFullBoard();
    }
}
