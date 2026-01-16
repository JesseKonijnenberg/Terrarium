using Terrarium.Core.Models.Hierarchy;

namespace Terrarium.Core.Models.Kanban;

public class KanbanBoardEntity : EntityBase
{
    public string Name { get; set; } = "Default Board";
    
    public required string ProjectId { get; set; }
    public virtual ProjectEntity Project { get; set; } = null!;
    
    public string? CurrentIterationId { get; set; }
    public virtual IterationEntity? CurrentIteration { get; set; }

    public virtual List<ColumnEntity> Columns { get; set; } = new();
}