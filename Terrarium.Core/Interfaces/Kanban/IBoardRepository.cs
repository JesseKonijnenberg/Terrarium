using Terrarium.Core.Models.Kanban;

namespace Terrarium.Core.Interfaces.Kanban;

/// <summary>
/// Defines the data access contract for persisting and retrieving Kanban board data.
/// </summary>
/// <remarks>
/// This repository abstracts the underlying database technology (e.g., SQLite) 
/// from the core business logic. It now manages the KanbanBoard as a central plugin container.
/// </remarks>
public interface IBoardRepository
{
    /// <summary>
    /// Retrieves the Kanban board container for a specific project, 
    /// including its columns and tasks filtered by the current iteration.
    /// </summary>
    /// <param name="workspaceId">The ID of the parent workspace.</param>
    /// <param name="projectId">The ID of the specific project.</param>
    /// <returns>A KanbanBoardEntity containing columns and iteration-specific tasks, or null if not found.</returns>
    Task<KanbanBoardEntity?> GetBoardAsync(string workspaceId, string? projectId = null);

    /// <summary>
    /// Persists a new task entity to a specific column, automatically linking it 
    /// to the board's current active iteration.
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
    /// Scopes deletion to only the tasks belonging to the current active iteration of the project's board.
    /// </summary>
    /// <param name="projectId">The ID of the project whose active iteration should be cleared.</param>
    Task DeleteAllTasksInIterationAsync(string projectId);

    /// <summary>
    /// Updates the column association and sort order for a single task within its current iteration.
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