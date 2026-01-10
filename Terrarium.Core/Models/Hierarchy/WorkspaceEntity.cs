namespace Terrarium.Core.Models.Hierarchy;

public class WorkspaceEntity : EntityBase
{
    public string Name { get; set; } = string.Empty;
    public bool IsPersonal { get; set; } = true;
    
    // Nullable because a Solo user doesn't belong to an Org
    public string? OrganizationId { get; set; }
    public virtual OrganizationEntity? Organization { get; set; }

    public virtual List<ProjectEntity> Projects { get; set; } = new();
}