#region File Header & Imports

using Terrarium.Core.Events.Kanban;
using Terrarium.Core.Models.Kanban;

#endregion

namespace Terrarium.Core.Interfaces.Kanban;

/// <summary>
/// Provides a contract for managing Kanban board state, task orchestration, 
/// and coordination between persistence and domain logic.
/// </summary>
public interface IBoardService
{
#region Events
    /// <summary>
    /// Triggered whenever the board structure or task data is modified.
    /// </summary>
    event EventHandler<BoardChangedEventsArgs> BoardChanged;
#endregion

#region Retrieval
    /// <summary>
    /// Fetches the board configuration and tasks from the underlying data store.
    /// </summary>
    Task<List<ColumnEntity>> LoadBoardAsync();

    /// <summary>
    /// Retrieves the board data currently held in memory.
    /// </summary>
    List<ColumnEntity> GetCachedBoard();
#endregion

#region Task Persistence
    /// <summary>
    /// Persists a new task to the database and updates the local cache.
    /// </summary>
    Task AddTaskAsync(TaskEntity task, string toColumnId);

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
#endregion

#region Orchestration & Logic
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
    /// Processes clipboard text into new Task entities and assigns them to relevant columns.
    /// </summary>
    Task<IEnumerable<TaskEntity>> ProcessSmartPasteAsync(string text);

    /// <summary>
    /// Creates a new TaskEntity with default values specialized for a specific column.
    /// </summary>
    Task<TaskEntity> CreateDefaultTaskEntity(string columnId);

    /// <summary>
    /// Parses UI-provided strings into valid entity data and triggers an update.
    /// </summary>
    Task UpdateTaskFromUiAsync(TaskEntity entity, string title, string description, string tag, string priority, string date);
#endregion
}