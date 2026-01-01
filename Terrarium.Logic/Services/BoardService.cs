using Terrarium.Core.Interfaces.Kanban;
using Terrarium.Core.Models.Kanban;
// Needed for LINQ

namespace Terrarium.Logic.Services
{
    public class BoardService : IBoardService
    {
        private readonly IBoardRepository _repository;

        private List<ColumnEntity> _boardCache = new();

        public BoardService(IBoardRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<ColumnEntity>> LoadBoardAsync()
        {
            _boardCache = await _repository.LoadBoardAsync();
            return _boardCache;
        }

        public List<ColumnEntity> GetCachedBoard() => _boardCache;

        public async Task AddTaskAsync(TaskEntity task, string columnId)
        {
            await _repository.AddTaskAsync(task, columnId);

            var col = _boardCache.FirstOrDefault(c => c.Id == columnId);
            if (col != null)
            {
                col.Tasks.Insert(0, task);
            }
        }

        public async Task MoveTaskAsync(TaskEntity task, string targetColumnId, int index)
        {
            await _repository.MoveTaskAsync(task.Id, targetColumnId, index);

            var sourceCol = _boardCache.FirstOrDefault(c => c.Tasks.Any(t => t.Id == task.Id));
            if (sourceCol != null)
            {
                var taskInCache = sourceCol.Tasks.First(t => t.Id == task.Id);
                sourceCol.Tasks.Remove(taskInCache);

                var targetCol = _boardCache.FirstOrDefault(c => c.Id == targetColumnId);
                if (targetCol != null)
                {
 
                    if (index < 0) index = 0;
                    if (index > targetCol.Tasks.Count) index = targetCol.Tasks.Count;

                    targetCol.Tasks.Insert(index, taskInCache);
                }
            }
        }

        public async Task DeleteTaskAsync(TaskEntity task)
        {
            await _repository.DeleteTaskAsync(task.Id);

            var col = _boardCache.FirstOrDefault(c => c.Tasks.Any(t => t.Id == task.Id));
            if (col != null)
            {
                var taskInCache = col.Tasks.FirstOrDefault(t => t.Id == task.Id);
                if (taskInCache != null)
                {
                    col.Tasks.Remove(taskInCache);
                }
            }
        }

        public async Task UpdateTaskAsync(TaskEntity task)
        {
            await _repository.UpdateTaskAsync(task);

            var col = _boardCache.FirstOrDefault(c => c.Tasks.Any(t => t.Id == task.Id));
            if (col != null)
            {
                var cachedTask = col.Tasks.FirstOrDefault(t => t.Id == task.Id);
                if (cachedTask != null)
                {
                    cachedTask.Title = task.Title;
                    cachedTask.Description = task.Description;
                    cachedTask.Tag = task.Tag;
                    cachedTask.Priority = task.Priority;
                    cachedTask.DueDate = task.DueDate;
                }
            }
        }
    }
}