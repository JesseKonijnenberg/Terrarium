namespace Terrarium.Core.Models.Kanban.DTO
{
    public record ParsedTaskResult(
        TaskEntity Task, 
        string TargetColumnName
    );
}