using Terrarium.Core.Enums.Kanban;
using Terrarium.Core.Models.Kanban;

namespace Terrarium.Core.Interfaces.Kanban;

public interface IBoardSerializer
{
    /// <summary>
    /// Converts a collection of board columns into a formatted string.
    /// </summary>
    string ToMarkdown(KanbanBoardEntity columns);

    /// <summary>
    /// Updates the formatting style based on a predefined format type.
    /// </summary>
    /// <param name="format">The target formatting style.</param>
    void SetFormat(SerializationFormat format);
}