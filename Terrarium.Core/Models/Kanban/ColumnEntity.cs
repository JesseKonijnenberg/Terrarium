namespace Terrarium.Core.Models.Kanban;

public class ColumnEntity : EntityBase
{
    public string Title { get; set; } = string.Empty;
    public int Order { get; set; }
    
    public required string KanbanBoardId { get; set; }
    public virtual KanbanBoardEntity KanbanBoard { get; set; } = null!;
    
    public virtual List<TaskEntity> Tasks { get; set; } = new();
}