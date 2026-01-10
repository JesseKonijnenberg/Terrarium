using Terrarium.Core.Enums.Kanban;
using Terrarium.Core.Events.Kanban;
using Terrarium.Core.Models.Kanban;

namespace Terrarium.Core.Interfaces.Kanban;

/// <summary>
/// Provides a contract for managing Kanban board state, task orchestration, 
/// and coordination between persistence and domain logic.
/// </summary>
public interface IBoardService
{
    /// <summary>
    /// Triggered whenever the board structure or task data is modified.
    /// </summary>
    event EventHandler<BoardChangedEventsArgs> BoardChanged;

    /// <summary>
    /// Loads the board for a specific workspace. 
    /// If projectId is provided, it filters for project-specific columns.
    /// </summary>
    Task<List<ColumnEntity>> LoadBoardAsync(string workspaceId, string? projectId = null);

    /// <summary>
    /// Retrieves the board data currently held in memory.
    /// </summary>
    List<ColumnEntity> GetCachedBoard();

    /// <summary>
    /// Persists a new task to the database and updates the local cache.
    /// </summary>
    Task AddTaskAsync(TaskEntity task, string toColumnId, string workspaceId, string? projectId = null);

    /// <summary>
    /// Updates an existing task's properties in the database.
    /// </summary>
    Task UpdateTaskAsync(TaskEntity task);

    /// <summary>
    /// Removes a single task from the board and data store.
    /// </summary>
    Task DeleteTaskAsync(TaskEntity task);

    /// <summary>
    /// Removes a collection of tasks based on their unique identifiers.
    /// </summary>
    Task DeleteMultipleTasksAsync(IEnumerable<string> taskIds);

    /// <summary>
    /// Wipes all task data from the board while preserving column structure.
    /// </summary>
    Task WipeBoardAsync();

    /// <summary>
    /// Moves a task to a new position or column.
    /// </summary>
    Task MoveTaskAsync(TaskEntity task, string toColumnId, int index);

    /// <summary>
    /// Moves multiple tasks to a target column starting at a specific index.
    /// </summary>
    Task MoveMultipleTasksAsync(IEnumerable<string> taskIds, string targetColumnId, int startIndex);

    /// <summary>
    /// Moves multiple tasks and handles potential rewards based on the destination column title.
    /// </summary>
    Task MoveTasksWithEconomyAsync(List<string> taskIds, string targetColumnId, string targetColumnTitle, int index);

    /// <summary>
    /// Processes smart paste and links results to the active hierarchy scope.
    /// </summary>
    Task<IEnumerable<TaskEntity>> ProcessSmartPasteAsync(string text, string workspaceId, string? projectId = null);

    /// <summary>
    /// Creates a new TaskEntity with default values specialized for a specific column.
    /// </summary>
    Task<TaskEntity> CreateDefaultTaskEntity(string columnId, string workspaceId, string? projectId = null);

    /// <summary>
    /// Updates a task's properties using native types.
    /// </summary>
    Task UpdateTaskFromUiAsync(
        TaskEntity entity, 
        string title, 
        string description, 
        string tag, 
        TaskPriority priority, // Changed from string
        DateTime dueDate);
}