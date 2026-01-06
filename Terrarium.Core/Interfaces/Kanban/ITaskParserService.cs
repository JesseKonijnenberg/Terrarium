using Terrarium.Core.Models.Kanban;

namespace Terrarium.Core.Interfaces.Kanban
{
    public record ParsedTaskResult(
        TaskEntity Task, 
        string TargetColumnName
    );
    
    public interface ITaskParserService
    {
        /// <summary>
        /// Parses a block of text (from clipboard) into a list of TaskEntities.
        /// </summary>
        IEnumerable<ParsedTaskResult> ParseClipboardText(string text);
    }
}

