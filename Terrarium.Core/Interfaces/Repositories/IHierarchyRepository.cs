using Terrarium.Core.Models.Hierarchy;

namespace Terrarium.Core.Interfaces.Repositories;

public interface IHierarchyRepository
{
    Task<List<OrganizationEntity>> GetFullHierarchyAsync();
    Task<List<WorkspaceEntity>> GetOrphanWorkspacesAsync();
}