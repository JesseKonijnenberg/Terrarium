using Terrarium.Core.Models.Kanban;

namespace Terrarium.Core.Interfaces.Kanban;

/// <summary>
/// Defines the data access contract for persisting and retrieving Kanban board data.
/// </summary>
/// <remarks>
/// This repository abstracts the underlying database technology (e.g., SQLite) 
/// from the core business logic.
/// </remarks>
public interface IBoardRepository
{
    /// <summary>
    /// Loads the entire board structure, including all columns and their associated tasks.
    /// </summary>
    /// <returns>A list of column entities with nested task collections.</returns>
    Task<List<ColumnEntity>> LoadBoardAsync();

    /// <summary>
    /// Persists a new task entity to a specific column.
    /// </summary>
    /// <param name="task">The task entity to save.</param>
    /// <param name="columnId">The unique identifier of the parent column.</param>
    Task AddTaskAsync(TaskEntity task, string columnId);

    /// <summary>
    /// Synchronizes changes from an in-memory task entity back to the data store.
    /// </summary>
    /// <param name="task">The task entity containing updated values.</param>
    Task UpdateTaskAsync(TaskEntity task);

    /// <summary>
    /// Permanently removes a task from the data store by its unique identifier.
    /// </summary>
    /// <param name="taskId">The GUID of the task to delete.</param>
    Task DeleteTaskAsync(string taskId);

    /// <summary>
    /// Performs a batch deletion of multiple tasks using a collection of identifiers.
    /// </summary>
    /// <param name="taskIds">A collection of GUIDs representing the tasks to be removed.</param>
    Task DeleteTasksAsync(IEnumerable<string> taskIds);

    /// <summary>
    /// Truncates the tasks table, removing all task data while preserving column definitions.
    /// </summary>
    Task DeleteAllTasksAsync();

    /// <summary>
    /// Updates the column association and sort order for a single task.
    /// </summary>
    /// <param name="taskId">The unique identifier of the task.</param>
    /// <param name="targetColumnId">The ID of the destination column.</param>
    /// <param name="newIndex">The sort order index within the destination column.</param>
    Task MoveTaskAsync(string taskId, string targetColumnId, int newIndex);

    /// <summary>
    /// Performs a batch move of multiple tasks to a target column.
    /// </summary>
    /// <param name="taskIds">Ordered list of task GUIDs to move.</param>
    /// <param name="targetColumnId">The ID of the destination column.</param>
    /// <param name="startIndex">The starting sort order index for the batch.</param>
    Task MoveMultipleTasksAsync(List<string> taskIds, string targetColumnId, int startIndex);
}