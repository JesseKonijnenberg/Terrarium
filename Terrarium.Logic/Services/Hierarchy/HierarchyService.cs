using Terrarium.Core.Interfaces.Hierarchy;
using Terrarium.Core.Models.Hierarchy;

namespace Terrarium.Logic.Services.Hierarchy;

public class HierarchyService : IHierarchyService
{
    private readonly IHierarchyRepository _repository;

    public HierarchyService(IHierarchyRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<OrganizationEntity>> GetUserHierarchyAsync()
    {
        // Get the real organizations from the DB
        var hierarchy = await _repository.GetFullHierarchyAsync();

        // Get the workspaces that don't belong to an organization (Personal)
        var orphans = await _repository.GetOrphanWorkspacesAsync();

        // If there are any personal workspaces, group them under a "Virtual" Org
        if (orphans.Any())
        {
            var personalOrg = new OrganizationEntity
            {
                Id = "virtual-personal-org",
                Name = "Personal",
                Workspaces = orphans
            };

            // Insert at the top so Personal always comes first in the sidebar
            hierarchy.Insert(0, personalOrg);
        }

        return hierarchy;
    }

    public async Task<WorkspaceEntity> CreateWorkspaceAsync(string name, string? orgId)
    {
        var workspace = new WorkspaceEntity 
        { 
            Id = Guid.NewGuid().ToString(),
            Name = name, 
            OrganizationId = orgId,
            IsPersonal = orgId == null 
        };

        return await _repository.AddWorkspaceAsync(workspace);
    }
}