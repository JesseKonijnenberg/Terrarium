using System.Text;
using System.Text.RegularExpressions;
using Terrarium.Core.Interfaces.Kanban;
using Terrarium.Core.Models.Kanban;

namespace Terrarium.Logic.Services.Kanban.Strategies
{
  public class TemplateMarkdownStrategy(string templatePath) : IBoardFormattingStrategy
    {
        public string Serialize(IEnumerable<ColumnEntity> columns)
        {
            if (!File.Exists(templatePath))
                return "# Error: Template file not found.";

            var rawTemplate = File.ReadAllText(templatePath);
            
            if (string.IsNullOrWhiteSpace(rawTemplate))
                return "# Error: Template file is empty.";
            
            rawTemplate = rawTemplate.Replace("{{Date}}", DateTime.Now.ToString("f"));
            
            var colRegex = new Regex(@"(?s)\[\[COLUMN_START\]\](.*?)\[\[COLUMN_END\]\]");
            var colMatch = colRegex.Match(rawTemplate);

            if (!colMatch.Success) return rawTemplate;

            var columnTemplate = colMatch.Groups[1].Value;
            
            var allColumnsBuilder = new StringBuilder();

            foreach (var col in columns)
            {
                var currentColText = ProcessColumn(columnTemplate, col);
                allColumnsBuilder.Append(currentColText);
            }
            
            return colRegex.Replace(rawTemplate, allColumnsBuilder.ToString());
        }

        private string ProcessColumn(string template, ColumnEntity column)
        {
            var sb = new StringBuilder(template);
            sb.Replace("{{ColumnName}}", column.Title);
            sb.Replace("{{TaskCount}}", column.Tasks?.Count.ToString() ?? "0");
            
            var taskRegex = new Regex(@"(?s)\[\[TASK_START\]\](.*?)\[\[TASK_END\]\]");
            var taskMatch = taskRegex.Match(template);

            if (taskMatch.Success && column.Tasks != null)
            {
                var taskTemplate = taskMatch.Groups[1].Value;
                var allTasksBuilder = new StringBuilder();

                foreach (var task in column.Tasks)
                {
                    allTasksBuilder.Append(ProcessTask(taskTemplate, task));
                }
                
                var result = taskRegex.Replace(sb.ToString(), allTasksBuilder.ToString());
                return result;
            }
            
            return taskRegex.Replace(sb.ToString(), ""); 
        }

        private string ProcessTask(string template, TaskEntity task)
        {
            var sb = new StringBuilder(template);
            
            sb.Replace("{{TaskTitle}}", task.Title);
            sb.Replace("{{TaskTag}}", task.Tag);
            sb.Replace("{{TaskPriority}}", task.Priority.ToString());
            
            var descRegex = new Regex(@"(?s)\[\[DESC_START\]\](.*?)\[\[DESC_END\]\]");
            var descMatch = descRegex.Match(template);

            if (descMatch.Success)
            {
                if (string.IsNullOrWhiteSpace(task.Description))
                {
                    return descRegex.Replace(sb.ToString(), "");
                }
                else
                {
                    var innerContent = descMatch.Groups[1].Value;
                    var filledContent = innerContent.Replace("{{TaskDescription}}", task.Description);
                    
                    filledContent = filledContent.Replace("\n", "\n> "); 
                    
                    return descRegex.Replace(sb.ToString(), filledContent);
                }
            }

            return sb.ToString();
        }
    }  
}

