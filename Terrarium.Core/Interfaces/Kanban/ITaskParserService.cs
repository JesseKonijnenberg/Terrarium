using Terrarium.Core.Models.Kanban.DTO;

namespace Terrarium.Core.Interfaces.Kanban;

public interface ITaskParserService
{
    /// <summary>
    /// Parses a block of text (from clipboard) into a list of TaskEntities.
    /// </summary>
    IEnumerable<ParsedTaskResult> ParseClipboardText(string text);
}