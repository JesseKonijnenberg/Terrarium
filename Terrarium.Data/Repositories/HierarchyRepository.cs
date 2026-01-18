using Microsoft.EntityFrameworkCore;
using Terrarium.Core.Interfaces.Repositories;
using Terrarium.Core.Models.Hierarchy;
using Terrarium.Data.Contexts;

namespace Terrarium.Data.Repositories;

public class HierarchyRepository : IHierarchyRepository
{
    private readonly IDbContextFactory<TerrariumDbContext> _contextFactory;

    public HierarchyRepository(IDbContextFactory<TerrariumDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<OrganizationEntity>> GetFullHierarchyAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Organizations
            .AsNoTracking()
            .Include(o => o.Workspaces)
            .ThenInclude(w => w.Projects)
            .ToListAsync();
    }
    
    public async Task<List<WorkspaceEntity>> GetOrphanWorkspacesAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Workspaces
            .AsNoTracking()
            .Include(w => w.Projects)
            .Where(w => w.OrganizationId == null)
            .ToListAsync();
    }
}