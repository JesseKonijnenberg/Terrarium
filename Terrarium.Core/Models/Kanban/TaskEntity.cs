using Terrarium.Core.Enums.Kanban;
using Terrarium.Core.Models.Hierarchy;

namespace Terrarium.Core.Models.Kanban;

public class TaskEntity : EntityBase
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public DateTime DueDate { get; set; } = DateTime.UtcNow;
    public string Tag { get; set; } = string.Empty; 
    public int Order { get; set; }
    
    public required string ColumnId { get; set; }
    public virtual ColumnEntity Column { get; set; } = null!;
    
    public string? IterationId { get; set; }
    public virtual IterationEntity? Iteration { get; set; }
    
    public string? ProjectId { get; set; }
    public virtual ProjectEntity? Project { get; set; }
}