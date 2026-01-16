namespace Terrarium.Core.Models.Context;

public record ProjectContext(string? OrgId, string? WorkspaceId, string? ProjectId)
{
    public bool IsProjectReady => !string.IsNullOrEmpty(OrgId) && 
                                  !string.IsNullOrEmpty(WorkspaceId) && 
                                  !string.IsNullOrEmpty(ProjectId);
                                  
    public static ProjectContext Empty() => new(null, null, null);
}