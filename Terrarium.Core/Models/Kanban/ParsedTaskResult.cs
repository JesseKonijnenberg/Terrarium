namespace Terrarium.Core.Models.Kanban
{
    public record ParsedTaskResult(
        TaskEntity Task, 
        string TargetColumnName
    );
}