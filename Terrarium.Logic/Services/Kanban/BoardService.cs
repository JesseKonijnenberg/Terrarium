using Terrarium.Core.Enums.Kanban;
using Terrarium.Core.Events.Kanban;
using Terrarium.Core.Interfaces.Garden;
using Terrarium.Core.Interfaces.Kanban;
using Terrarium.Core.Models.Kanban;

namespace Terrarium.Logic.Services.Kanban;

/// <summary>
/// Provides a concrete implementation of <see cref="IBoardService"/>, 
/// managing the lifecycle of Kanban tasks and synchronizing state between the UI and persistence layers.
/// </summary>
public class BoardService : IBoardService
{
    private readonly IBoardRepository _repository;
    private readonly ITaskParserService _taskParserService;
    private readonly IGardenEconomyService _gardenEconomyService;
    
    // Cache the whole Board object instead of just the columns
    private KanbanBoardEntity? _boardCache;

    /// <inheritdoc />
    public event EventHandler<BoardChangedEventsArgs>? BoardChanged;

    public BoardService(
        IBoardRepository repository, 
        ITaskParserService taskParserService, 
        IGardenEconomyService gardenEconomyService)
    {
        _repository = repository;
        _taskParserService = taskParserService;
        _gardenEconomyService = gardenEconomyService;
    }

    /// <inheritdoc />
    public async Task<KanbanBoardEntity?> GetBoardAsync(string workspaceId, string? projectId = null)
    {
        // Repository now returns the full Board entity (including filtered Columns/Tasks)
        _boardCache = await _repository.GetBoardAsync(workspaceId, projectId);
        return _boardCache;
    }

    /// <inheritdoc />
    public KanbanBoardEntity? GetCachedBoard() => _boardCache;

    /// <inheritdoc />
    public async Task AddTaskAsync(TaskEntity task, string columnId, string workspaceId, string? projectId = null)
    {
        task.ProjectId = projectId;
        task.ColumnId = columnId;
    
        await _repository.AddTaskAsync(task, columnId);
        
        var columnInCache = _boardCache?.Columns.FirstOrDefault(c => c.Id == columnId);
        if (columnInCache != null)
        {
            columnInCache.Tasks.Add(task);
        }
    
        NotifyBoardChanged();
    }

    /// <inheritdoc />
    public async Task UpdateTaskAsync(TaskEntity task)
    {
        await _repository.UpdateTaskAsync(task);

        var cachedTask = FindTaskInCache(task.Id);
        if (cachedTask != null)
        {
            cachedTask.Title = task.Title;
            cachedTask.Description = task.Description;
            cachedTask.Tag = task.Tag;
            cachedTask.Priority = task.Priority;
            cachedTask.DueDate = task.DueDate;
        }
        NotifyBoardChanged();
    }

    /// <inheritdoc />
    public async Task DeleteTaskAsync(TaskEntity task)
    {
        await _repository.DeleteTaskAsync(task.Id);

        var col = FindColumnContainingTask(task.Id);
        if (col != null)
        {
            col.Tasks.RemoveAll(t => t.Id == task.Id);
        }
        NotifyBoardChanged();
    }

    /// <inheritdoc />
    public async Task DeleteMultipleTasksAsync(IEnumerable<string> taskIds)
    {
        await _repository.DeleteTasksAsync(taskIds);
        
        var idSet = new HashSet<string>(taskIds);
        if (_boardCache != null)
        {
            foreach (var col in _boardCache.Columns) 
            {
                col.Tasks.RemoveAll(t => idSet.Contains(t.Id));
            }
        }

        NotifyBoardChanged();
    }

    /// <inheritdoc />
    public async Task WipeBoardAsync(string projectId)
    {
        await _repository.DeleteAllTasksInIterationAsync(projectId);
        
        if (_boardCache != null)
        {
            foreach (var col in _boardCache.Columns) 
            {
                col.Tasks.Clear();
            }
        }
        
        NotifyBoardChanged();
    }

    /// <inheritdoc />
    public async Task MoveTaskAsync(TaskEntity task, string targetColumnId, int index)
    {
        await _repository.MoveTaskAsync(task.Id, targetColumnId, index);

        var sourceCol = FindColumnContainingTask(task.Id);
        if (sourceCol != null)
        {
            var taskInCache = sourceCol.Tasks.FirstOrDefault(t => t.Id == task.Id);
            if (taskInCache == null) return;
            
            sourceCol.Tasks.Remove(taskInCache);

            var targetCol = _boardCache?.Columns.FirstOrDefault(c => c.Id == targetColumnId);
            if (targetCol != null)
            {
                index = Math.Clamp(index, 0, targetCol.Tasks.Count);
                taskInCache.ColumnId = targetColumnId; 
                targetCol.Tasks.Insert(index, taskInCache);
            }
        }
        NotifyBoardChanged();
    }

    /// <inheritdoc />
    public async Task MoveMultipleTasksAsync(IEnumerable<string> taskIds, string targetColumnId, int startIndex)
    {
        var ids = taskIds.ToList();
        await _repository.MoveMultipleTasksAsync(ids, targetColumnId, startIndex);
        
        var targetCol = _boardCache?.Columns.FirstOrDefault(c => c.Id == targetColumnId);
        if (targetCol == null) return;

        foreach (var id in ids)
        {
            var sourceCol = FindColumnContainingTask(id);
            if (sourceCol != null)
            {
                var taskEntity = sourceCol.Tasks.FirstOrDefault(t => t.Id == id);
                if (taskEntity == null) continue;
                
                sourceCol.Tasks.Remove(taskEntity);
                
                targetCol.Tasks.Insert(Math.Min(startIndex++, targetCol.Tasks.Count), taskEntity);
                taskEntity.ColumnId = targetColumnId;
            }
        }
        NotifyBoardChanged();
    }

    /// <inheritdoc />
    public async Task<TaskEntity> CreateDefaultTaskEntity(string columnId, string workspaceId, string? projectId = null)
    {
        return new TaskEntity
        {
            Id = Guid.NewGuid().ToString(),
            Title = "New Task",
            Tag = "New",
            Priority = TaskPriority.Low,
            DueDate = DateTime.Now,
            ColumnId = columnId,
            ProjectId = projectId
        };
    }

    /// <inheritdoc />
    public async Task UpdateTaskFromUiAsync(
        TaskEntity entity, 
        string title, 
        string description, 
        string tag, 
        TaskPriority priority, 
        DateTime dueDate)
    {
        entity.Title = title;
        entity.Description = description;
        entity.Tag = tag;
        entity.Priority = priority;
        entity.DueDate = dueDate;

        await UpdateTaskAsync(entity);
    }

    /// <inheritdoc />
    public async Task MoveTasksWithEconomyAsync(List<string> taskIds, string targetColumnId, string targetColumnTitle, int index)
    {
        await MoveMultipleTasksAsync(taskIds, targetColumnId, index);
        
        if (targetColumnTitle.Equals("Complete", StringComparison.OrdinalIgnoreCase) || 
            targetColumnTitle.Equals("Done", StringComparison.OrdinalIgnoreCase))
        {
            _gardenEconomyService.EarnWater(20 * taskIds.Count);
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<TaskEntity>> ProcessSmartPasteAsync(string text, string workspaceId, string? projectId = null)
    {
        var board = await _repository.GetBoardAsync(workspaceId, projectId);
        if (board == null) return Enumerable.Empty<TaskEntity>();

        var results = _taskParserService.ParseClipboardText(text).ToList();
        var processedTasks = new List<TaskEntity>();

        foreach (var result in results)
        {
            var targetCol = board.Columns.FirstOrDefault(c => 
                                result.TargetColumnName.Contains(c.Title, StringComparison.OrdinalIgnoreCase) ||
                                c.Title.Contains(result.TargetColumnName, StringComparison.OrdinalIgnoreCase))
                            ?? board.Columns.FirstOrDefault();

            if (targetCol == null) continue;

            result.Task.ColumnId = targetCol.Id;
            result.Task.ProjectId = projectId;

            await _repository.AddTaskAsync(result.Task, targetCol.Id);
            processedTasks.Add(result.Task);
        }

        return processedTasks;
    }

    private void NotifyBoardChanged() 
        => BoardChanged?.Invoke(this, new BoardChangedEventsArgs());

    private ColumnEntity? FindColumnContainingTask(string taskId) 
        => _boardCache?.Columns.FirstOrDefault(c => c.Tasks.Any(t => t.Id == taskId));

    private TaskEntity? FindTaskInCache(string taskId)
        => _boardCache?.Columns.SelectMany(c => c.Tasks).FirstOrDefault(t => t.Id == taskId);
}