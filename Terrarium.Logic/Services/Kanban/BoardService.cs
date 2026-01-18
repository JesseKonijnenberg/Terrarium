using Terrarium.Core.Enums.Kanban;
using Terrarium.Core.Events.Kanban;
using Terrarium.Core.Interfaces.Kanban;
using Terrarium.Core.Interfaces.Services;
using Terrarium.Core.Messages;
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
    private readonly ITerrariumEventBus _eventBus;
    
    /// <inheritdoc />
    public event EventHandler<BoardChangedEventsArgs>? BoardChanged;

    public BoardService(
        IBoardRepository repository, 
        ITaskParserService taskParserService, 
        ITerrariumEventBus eventBus)
    {
        _repository = repository;
        _taskParserService = taskParserService;
        _eventBus = eventBus;
    }

    /// <inheritdoc />
    public async Task<KanbanBoardEntity?> GetBoardAsync(string workspaceId, string? projectId = null)
    {
        return await _repository.GetBoardAsync(workspaceId, projectId);
    }

    /// <inheritdoc />
    public KanbanBoardEntity? GetCachedBoard() => null;

    /// <inheritdoc />
    public async Task AddTaskAsync(TaskEntity task, string columnId, string workspaceId, string? projectId = null)
    {
        task.ProjectId = projectId;
        task.ColumnId = columnId;
    
        await _repository.AddTaskAsync(task, columnId);
        
        NotifyBoardChanged(workspaceId, projectId);
    }

    /// <inheritdoc />
    public async Task UpdateTaskAsync(TaskEntity task)
    {
        await _repository.UpdateTaskAsync(task);
        if (!string.IsNullOrEmpty(task.ProjectId))
        {
             NotifyBoardChanged(null, task.ProjectId);
        }
    }

    /// <inheritdoc />
    public async Task DeleteTaskAsync(TaskEntity task)
    {
        await _repository.DeleteTaskAsync(task.Id);
        NotifyBoardChanged(null, task.ProjectId);
    }

    /// <inheritdoc />
    public async Task DeleteMultipleTasksAsync(IEnumerable<string> taskIds)
    {
        await _repository.DeleteTasksAsync(taskIds);
        // Generic update since we don't have context
        NotifyBoardChanged(null, null);
    }

    /// <inheritdoc />
    public async Task WipeBoardAsync(string projectId)
    {
        await _repository.DeleteAllTasksInIterationAsync(projectId);
        NotifyBoardChanged(null, projectId);
    }

    /// <inheritdoc />
    public async Task MoveTaskAsync(TaskEntity task, string targetColumnId, int index)
    {
        var oldColumnId = task.ColumnId;
        await _repository.MoveTaskAsync(task.Id, targetColumnId, index);
        
        _eventBus.Publish(new TaskMovedMessage(task.Id, targetColumnId, oldColumnId, "Unknown"));
        NotifyBoardChanged(null, task.ProjectId);
    }

    /// <inheritdoc />
    public async Task MoveMultipleTasksAsync(IEnumerable<string> taskIds, string targetColumnId, int startIndex)
    {
        var ids = taskIds.ToList();
        
        await _repository.MoveMultipleTasksAsync(ids, targetColumnId, startIndex);

        NotifyBoardChanged(null, null); 
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
        
        foreach (var taskId in taskIds)
        {
            _eventBus.Publish(new TaskMovedMessage(taskId, targetColumnId, "Unknown", targetColumnTitle));
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<TaskEntity>> ProcessSmartPasteAsync(string text, string workspaceId, string? projectId = null)
    {
        var board = await _repository.GetBoardAsync(workspaceId, projectId);
        if (board == null) return Enumerable.Empty<TaskEntity>();
        
        var results = _taskParserService.ParseClipboardText(text).ToList();
        var processedTasks = new List<TaskEntity>();

        foreach (var dto in results)
        {
            var targetCol = board.Columns.FirstOrDefault(c => 
                                c.Title.Contains(dto.TargetColumnName, StringComparison.OrdinalIgnoreCase))
                            ?? board.Columns.FirstOrDefault();

            if (targetCol == null) continue;
            
            var newTask = new TaskEntity
            {
                Id = Guid.NewGuid().ToString(),
                Title = dto.Title,
                Description = dto.Description,
                Tag = dto.Tag,
                Priority = dto.Priority,
                DueDate = DateTime.UtcNow,
                
                ColumnId = targetCol.Id,
                ProjectId = projectId,
            };
            
            await _repository.AddTaskAsync(newTask, targetCol.Id);
            processedTasks.Add(newTask);
        }
        
        NotifyBoardChanged(workspaceId, projectId);

        return processedTasks;
    }
    
    public async Task<KanbanBoardEntity> CreateBoardAsync(string workspaceId, string projectId)
    {
        var newBoard = new KanbanBoardEntity
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Main Board",
            ProjectId = projectId,
            LastModifiedUtc = DateTime.UtcNow,
            
        };
        newBoard.Columns = CreateDefaultColumns(newBoard);
        
        await _repository.CreateBoardAsync(newBoard);
        
        return newBoard;
    }

    private List<ColumnEntity> CreateDefaultColumns(KanbanBoardEntity board)
    {
        var columns = new List<ColumnEntity>
        {
            new ColumnEntity { Id = Guid.NewGuid().ToString(), KanbanBoardId = board.Id, KanbanBoard = board,Title = "Backlog", Order = 0, LastModifiedUtc = DateTime.UtcNow },
            new ColumnEntity { Id = Guid.NewGuid().ToString(), KanbanBoardId = board.Id, KanbanBoard = board,Title = "To Do", Order = 1, LastModifiedUtc = DateTime.UtcNow },
            new ColumnEntity { Id = Guid.NewGuid().ToString(), KanbanBoardId = board.Id, KanbanBoard = board,Title = "In Progress", Order = 2, LastModifiedUtc = DateTime.UtcNow },
            new ColumnEntity { Id = Guid.NewGuid().ToString(), KanbanBoardId = board.Id, KanbanBoard = board,Title = "Done", Order = 3, LastModifiedUtc = DateTime.UtcNow }
        };
        return columns;
    }

    private void NotifyBoardChanged(string? workspaceId, string? projectId) 
    {
        BoardChanged?.Invoke(this, new BoardChangedEventsArgs());
        _eventBus.Publish(new BoardUpdatedMessage(workspaceId ?? "", projectId ?? ""));
    }
}