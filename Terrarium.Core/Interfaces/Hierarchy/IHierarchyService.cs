using Terrarium.Core.Models.Hierarchy;

namespace Terrarium.Core.Interfaces.Hierarchy;

public interface IHierarchyService
{
    Task<List<OrganizationEntity>> GetUserHierarchyAsync();
    Task<WorkspaceEntity> CreateWorkspaceAsync(string name, string? orgId);
    OrganizationEntity? ActiveOrganization { get; set; }

    (OrganizationEntity? Org, WorkspaceEntity? Ws, ProjectEntity? Proj) GetDefaultSelection(
        List<OrganizationEntity> hierarchy);
}