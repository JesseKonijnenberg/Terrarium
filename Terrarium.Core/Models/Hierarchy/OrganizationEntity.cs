namespace Terrarium.Core.Models.Hierarchy;

public class OrganizationEntity : EntityBase
{
    public string Name { get; set; } = string.Empty;
    
    // Navigation: One Org -> Many Workspaces (Departments)
    public virtual List<WorkspaceEntity> Workspaces { get; set; } = new();
}