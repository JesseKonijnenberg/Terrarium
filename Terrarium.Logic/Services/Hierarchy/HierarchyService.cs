using Terrarium.Core.Interfaces.Hierarchy;
using Terrarium.Core.Interfaces.Repositories;
using Terrarium.Core.Models.Hierarchy;

namespace Terrarium.Logic.Services.Hierarchy;

public class HierarchyService : IHierarchyService
{
    private readonly IHierarchyRepository _repository;

    public HierarchyService(IHierarchyRepository repository)
    {
        _repository = repository;
    }
    
    public OrganizationEntity? ActiveOrganization { get; set; }

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
        
        if (ActiveOrganization == null && hierarchy.Any())
        {
            ActiveOrganization = hierarchy.First();
        }

        return hierarchy;
    }
    
    public (OrganizationEntity? Org, WorkspaceEntity? Ws, ProjectEntity? Proj) GetDefaultSelection(List<OrganizationEntity> hierarchy)
    {
        var org = hierarchy.FirstOrDefault();
        if (org == null) return (null, null, null);

        // Find the first workspace that actually has a project
        var workspaceWithProject = org.Workspaces?.FirstOrDefault(ws => ws.Projects?.Any() == true);
        
        // If no workspace has a project, just take the first workspace
        var ws = workspaceWithProject ?? org.Workspaces?.FirstOrDefault();
        var proj = ws?.Projects?.FirstOrDefault();

        return (org, ws, proj);
    }
}