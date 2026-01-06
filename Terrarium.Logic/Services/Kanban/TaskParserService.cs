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

        public IEnumerable<TaskEntity> ParseClipboardText(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) yield break;

            var lines = text.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
            TaskEntity? currentTask = null;

            foreach (var line in lines)
            {
                var trimmedLine = line.TrimStart();
                var taskMatch = TaskLineRegex.Match(trimmedLine);

                if (taskMatch.Success)
                {
                    if (currentTask != null) yield return currentTask;

                    var fullTitleLine = taskMatch.Groups[1].Value;
                    currentTask = CreateTaskFromLine(fullTitleLine);
                    continue;
                }

                if (currentTask != null && trimmedLine.StartsWith('>'))
                {
                    var description = trimmedLine[1..].Trim();
                    currentTask.Description = string.IsNullOrEmpty(currentTask.Description) 
                        ? description 
                        : currentTask.Description + Environment.NewLine + description;
                }
            }

            if (currentTask != null) yield return currentTask;
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