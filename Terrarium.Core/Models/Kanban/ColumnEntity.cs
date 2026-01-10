namespace Terrarium.Core.Models.Kanban;

public class ColumnEntity : EntityBase
{
    public string Title { get; set; } = string.Empty;
    public int Order { get; set; }
    public List<TaskEntity> Tasks { get; set; } = new();
    public string? IterationId { get; set; }
}