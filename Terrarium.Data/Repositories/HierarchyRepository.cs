using Microsoft.EntityFrameworkCore;
using Terrarium.Core.Interfaces.Hierarchy;
using Terrarium.Core.Models.Hierarchy;
using Terrarium.Data.Contexts;

namespace Terrarium.Data.Repositories;

public class HierarchyRepository : IHierarchyRepository
{
    private readonly TerrariumDbContext _context;

    public HierarchyRepository(TerrariumDbContext context)
    {
        _context = context;
    }

    public async Task<List<OrganizationEntity>> GetFullHierarchyAsync()
    {
        return await _context.Organizations
            .Include(o => o.Workspaces)
            .ThenInclude(w => w.Projects)
            .ToListAsync();
    }

    public async Task<WorkspaceEntity> AddWorkspaceAsync(WorkspaceEntity workspace)
    {
        _context.Workspaces.Add(workspace);
        await _context.SaveChangesAsync();
        return workspace;
    }

    public async Task<OrganizationEntity> AddOrganizationAsync(OrganizationEntity org)
    {
        _context.Organizations.Add(org);
        await _context.SaveChangesAsync();
        return org;
    }
    
    public async Task<List<WorkspaceEntity>> GetOrphanWorkspacesAsync()
    {
        return await _context.Workspaces
            .Include(w => w.Projects)
            .Where(w => w.OrganizationId == null)
            .ToListAsync();
    }
}