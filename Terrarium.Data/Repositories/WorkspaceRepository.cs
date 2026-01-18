using Microsoft.EntityFrameworkCore;
using Terrarium.Core.Interfaces.Repositories;
using Terrarium.Core.Models.Hierarchy;
using Terrarium.Data.Contexts;

namespace Terrarium.Data.Repositories;

public class WorkspaceRepository : IWorkspaceRepository
{
    private readonly IDbContextFactory<TerrariumDbContext> _contextFactory;

    public WorkspaceRepository(IDbContextFactory<TerrariumDbContext> contextFactory) => _contextFactory = contextFactory;

    public async Task<WorkspaceEntity?> GetByIdAsync(string id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Workspaces.AsNoTracking().FirstOrDefaultAsync(w => w.Id == id);
    }

    public async Task AddAsync(WorkspaceEntity entity)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        // Prevent disconnected graph errors
        entity.Organization = null;
        
        context.Workspaces.Add(entity);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(string id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var entity = await context.Workspaces.FindAsync(id);
        if (entity != null)
        {
            entity.IsDeleted = true;
            entity.LastModifiedUtc = DateTime.UtcNow;
            await context.SaveChangesAsync();
        }
    }
}