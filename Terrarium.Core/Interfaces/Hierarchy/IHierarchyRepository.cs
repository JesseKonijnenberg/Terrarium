using Terrarium.Core.Models.Hierarchy;

namespace Terrarium.Core.Interfaces.Hierarchy;

public interface IHierarchyRepository
{
    Task<List<OrganizationEntity>> GetFullHierarchyAsync();
    Task<WorkspaceEntity> AddWorkspaceAsync(WorkspaceEntity workspace);
    Task<OrganizationEntity> AddOrganizationAsync(OrganizationEntity org);
    Task<List<WorkspaceEntity>> GetOrphanWorkspacesAsync();
}