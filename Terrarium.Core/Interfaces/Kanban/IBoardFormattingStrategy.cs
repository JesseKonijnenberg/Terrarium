using Terrarium.Core.Models.Kanban;

namespace Terrarium.Core.Interfaces.Kanban;

/// <summary>
/// Defines a pluggable strategy for converting Kanban board data into a specific string format.
/// </summary>
/// <remarks>
/// This interface allows the <c>BoardSerializer</c> to switch between different output styles 
/// (e.g., Markdown tables, lists, or custom templates) at runtime.
/// </remarks>
public interface IBoardFormattingStrategy
{
    /// <summary>
    /// Transforms a collection of columns and tasks into a single formatted string.
    /// </summary>
    /// <param name="board"></param>
    /// <returns>A string representation of the board state.</returns>
    string Serialize(KanbanBoardEntity board);
}