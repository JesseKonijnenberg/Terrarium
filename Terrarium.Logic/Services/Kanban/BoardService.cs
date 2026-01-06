using Terrarium.Core.Events.Kanban;
using Terrarium.Core.Interfaces.Kanban;
using Terrarium.Core.Models.Kanban;

namespace Terrarium.Logic.Services.Kanban
{
    public class BoardService : IBoardService
    {
        private readonly IBoardRepository _repository;
        private readonly ITaskParserService _taskParserService;
        
        private List<ColumnEntity> _boardCache = new();
        
        public event EventHandler<BoardChangedEventsArgs> BoardChanged;

        public BoardService(IBoardRepository repository, ITaskParserService taskParserService)
        {
            _repository = repository;
            _taskParserService = taskParserService;
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
            NotifyBoardChanged(new BoardChangedEventsArgs());
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
            NotifyBoardChanged(new BoardChangedEventsArgs());
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
            NotifyBoardChanged(new BoardChangedEventsArgs());
        }
        
        public async Task DeleteMultipleTasksAsync(IEnumerable<string> taskIds)
        {
            await _repository.DeleteTasksAsync(taskIds);
            
            var idSet = new HashSet<string>(taskIds);
            foreach (var col in _boardCache)
            {
                col.Tasks.RemoveAll(t => idSet.Contains(t.Id));
            }

            NotifyBoardChanged(new BoardChangedEventsArgs());
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
            NotifyBoardChanged(new BoardChangedEventsArgs());
        }
        
        public async Task<IEnumerable<TaskEntity>> ProcessSmartPasteAsync(string text)
        {
            var boardData = await _repository.LoadBoardAsync();
            var results = _taskParserService.ParseClipboardText(text).ToList();
            var processedTasks = new List<TaskEntity>();

            foreach (var result in results)
            {
                var targetCol = boardData.FirstOrDefault(c => 
                                    result.TargetColumnName.Contains(c.Title, StringComparison.OrdinalIgnoreCase) ||
                                    c.Title.Contains(result.TargetColumnName, StringComparison.OrdinalIgnoreCase))
                                ?? boardData.FirstOrDefault();

                if (targetCol == null) continue;

                result.Task.ColumnId = targetCol.Id;
                await _repository.AddTaskAsync(result.Task, targetCol.Id);
                processedTasks.Add(result.Task);
            }

            return processedTasks;
        }
        
        public async Task WipeBoardAsync()
        {
            await _repository.DeleteAllTasksAsync();
            foreach (var col in _boardCache)
            {
                col.Tasks.Clear();
            }
            NotifyBoardChanged(new BoardChangedEventsArgs());
        }
        
        public async Task MoveMultipleTasksAsync(IEnumerable<string> taskIds, string targetColumnId, int startIndex)
        {
            var ids = taskIds.ToList();
            
            await _repository.MoveMultipleTasksAsync(ids, targetColumnId, startIndex);
            
            var targetCol = _boardCache.FirstOrDefault(c => c.Id == targetColumnId);
            if (targetCol == null) return;

            foreach (var id in ids)
            {
                var sourceCol = _boardCache.FirstOrDefault(c => c.Tasks.Any(t => t.Id == id));
                if (sourceCol != null)
                {
                    var taskEntity = sourceCol.Tasks.First(t => t.Id == id);
                    sourceCol.Tasks.Remove(taskEntity);
                    
                    targetCol.Tasks.Insert(Math.Min(startIndex++, targetCol.Tasks.Count), taskEntity);
                    taskEntity.ColumnId = targetColumnId;
                }
            }

            NotifyBoardChanged(new BoardChangedEventsArgs());
        }

        private void NotifyBoardChanged(BoardChangedEventsArgs e)
        {
            BoardChanged?.Invoke(this,e);
        }
    }
}