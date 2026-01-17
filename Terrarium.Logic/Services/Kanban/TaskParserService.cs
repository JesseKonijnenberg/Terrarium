using System.Text.RegularExpressions;
using Terrarium.Core.Enums.Kanban;
using Terrarium.Core.Interfaces.Kanban;
using Terrarium.Core.Models.Kanban.DTO;

namespace Terrarium.Logic.Services.Kanban;

public class TaskParserService : ITaskParserService
{
    private static readonly Regex TagRegex = new(@"#(\w+)", RegexOptions.Compiled);
    private static readonly Regex PriorityRegex = new(@"!(\w+)", RegexOptions.Compiled);
    private static readonly Regex TaskLineRegex = new(@"^[\s-]*\[[x\s]?\]\s*(.*)", RegexOptions.Compiled);
        
    private static readonly Regex ColumnHeaderRegex = 
        new(@"^##\s+([\w\s🏔️🏗️🧪✅]+?)(?:\s*\(.*\))?$", RegexOptions.Compiled);

    /// <inheritdoc />
    public IEnumerable<ParsedTaskResult> ParseClipboardText(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) yield break;

        var lines = text.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        
        ParsedTaskResult? currentTaskDto = null;
        string currentColumnName = "Backlog"; 

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            if (trimmedLine.StartsWith("[[") || trimmedLine.StartsWith("---")) continue;

            var colMatch = ColumnHeaderRegex.Match(trimmedLine);
            if (colMatch.Success)
            {
                if (currentTaskDto != null)
                {
                    yield return currentTaskDto with { TargetColumnName = currentColumnName };
                    currentTaskDto = null;
                }

                var rawName = colMatch.Groups[1].Value.Trim();
                var cleaned = Regex.Replace(rawName, @"[^\w\s]", "").Trim();
                currentColumnName = Regex.Replace(cleaned, @"\p{M}", "").Trim();
                continue;
            }

            var taskMatch = TaskLineRegex.Match(trimmedLine);
            if (taskMatch.Success)
            {
                if (currentTaskDto != null)
                {
                    yield return currentTaskDto with { TargetColumnName = currentColumnName };
                }
                
                currentTaskDto = CreateDtoFromLine(taskMatch.Groups[1].Value, currentColumnName);
                continue;
            }

            if (currentTaskDto != null && trimmedLine.StartsWith('>'))
            {
                var description = trimmedLine.TrimStart('>').Trim();
                var newDesc = string.IsNullOrEmpty(currentTaskDto.Description)
                    ? description
                    : currentTaskDto.Description + Environment.NewLine + description;
                
                currentTaskDto = currentTaskDto with { Description = newDesc };
            }
        }

        if (currentTaskDto != null)
        {
            yield return currentTaskDto with { TargetColumnName = currentColumnName };
        }
    }

    private ParsedTaskResult CreateDtoFromLine(string line, string currentColumnName)
    {
        var tagMatch = TagRegex.Match(line);
        string tag = tagMatch.Success ? tagMatch.Groups[1].Value : "General";

        var priorityMatch = PriorityRegex.Match(line);
        string priorityStr = priorityMatch.Success ? priorityMatch.Groups[1].Value : "Normal";
        
        string cleanTitle = line;
        if (tagMatch.Success) cleanTitle = cleanTitle.Replace(tagMatch.Value, "");
        if (priorityMatch.Success) cleanTitle = cleanTitle.Replace(priorityMatch.Value, "");
        
        return new ParsedTaskResult(
            Title: cleanTitle.Trim(),
            Description: "",
            Tag: tag,
            Priority: ParsePriority(priorityStr),
            TargetColumnName: currentColumnName
        );
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