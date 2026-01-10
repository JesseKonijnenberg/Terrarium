namespace Terrarium.Core.Models.Hierarchy;

public class ProjectEntity : EntityBase
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    public string WorkspaceId { get; set; } = string.Empty;
    public virtual WorkspaceEntity Workspace { get; set; } = null!;

    public List<IterationEntity> Iterations { get; set; } = new();
}