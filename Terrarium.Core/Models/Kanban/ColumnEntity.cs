using Terrarium.Core.Models.Hierarchy;

namespace Terrarium.Core.Models.Kanban;

public class ColumnEntity : EntityBase
{
    public string Title { get; set; } = string.Empty;
    public int Order { get; set; }
    
    // If set, this column only appears in a specific Project
    public string? ProjectId { get; set; }
    public virtual ProjectEntity? Project { get; set; }

    // If set, this column is a "Global" column for the entire Workspace
    public string? WorkspaceId { get; set; }
    public virtual WorkspaceEntity? Workspace { get; set; }

    // Optional: Link to a specific iteration (sprint-specific columns)
    public string? IterationId { get; set; }
    public virtual IterationEntity? Iteration { get; set; }

    // Navigation to the tasks inside this column
    public virtual List<TaskEntity> Tasks { get; set; } = new();
}