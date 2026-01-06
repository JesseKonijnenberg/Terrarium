using System.Text.RegularExpressions;
using Terrarium.Core.Enums.Kanban;
using Terrarium.Core.Interfaces.Kanban;
using Terrarium.Core.Models.Kanban;

namespace Terrarium.Logic.Services.Kanban
{
    public class TaskParserService : ITaskParserService
    {
        private static readonly Regex TagRegex = new(@"#(\w+)", RegexOptions.Compiled);
        private static readonly Regex PriorityRegex = new(@"!(\w+)", RegexOptions.Compiled);
        private static readonly Regex TaskLineRegex = new(@"^[\s-]*\[[x\s]?\]\s*(.*)", RegexOptions.Compiled);
        
        private static readonly Regex ColumnHeaderRegex = new(@"^##\s+([^(]+)", RegexOptions.Compiled);

        public IEnumerable<ParsedTaskResult> ParseClipboardText(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) yield break;

            var lines = text.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
            TaskEntity? currentTask = null;
            string currentColumnName = "";

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                
                if (trimmedLine.StartsWith("[[") || trimmedLine.StartsWith("---")) continue;
                
                var colMatch = ColumnHeaderRegex.Match(trimmedLine);
                if (colMatch.Success)
                {
                    currentColumnName = colMatch.Groups[1].Value.Trim();
                    continue;
                }
                
                var taskMatch = TaskLineRegex.Match(trimmedLine);
                if (taskMatch.Success)
                {
                    if (currentTask != null) 
                        yield return new ParsedTaskResult(currentTask, currentColumnName);

                    currentTask = CreateTaskFromLine(taskMatch.Groups[1].Value);
                    continue;
                }
                
                if (currentTask != null && trimmedLine.StartsWith('>'))
                {
                    var description = trimmedLine.TrimStart('>').Trim();
                    currentTask.Description = string.IsNullOrEmpty(currentTask.Description) 
                        ? description 
                        : currentTask.Description + Environment.NewLine + description;
                }
            }

            if (currentTask != null) 
                yield return new ParsedTaskResult(currentTask, currentColumnName);
        }

        private TaskEntity CreateTaskFromLine(string line)
        {
            var tagMatch = TagRegex.Match(line);
            string tag = tagMatch.Success ? tagMatch.Groups[1].Value : "General";

            var priorityMatch = PriorityRegex.Match(line);
            string priorityStr = priorityMatch.Success ? priorityMatch.Groups[1].Value : "Normal";
            
            string cleanTitle = line;
            if (tagMatch.Success) cleanTitle = cleanTitle.Replace(tagMatch.Value, "");
            if (priorityMatch.Success) cleanTitle = cleanTitle.Replace(priorityMatch.Value, "");
            
            return new TaskEntity
            {
                Title = cleanTitle.Trim(),
                Tag = tag,
                Priority = ParsePriority(priorityStr),
                Description = ""
            };
        }

        private TaskPriority ParsePriority(string priority)
        {
            return priority.ToLower() switch
            {
                "high" => TaskPriority.High,
                "medium" or "med" => TaskPriority.Medium,
                _ => TaskPriority.Low
            };
        }
    }
}