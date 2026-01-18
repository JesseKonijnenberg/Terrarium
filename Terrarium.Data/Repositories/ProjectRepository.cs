using Microsoft.EntityFrameworkCore;
using Terrarium.Core.Interfaces.Repositories;
using Terrarium.Core.Models.Hierarchy;
using Terrarium.Data.Contexts;

namespace Terrarium.Data.Repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly IDbContextFactory<TerrariumDbContext> _contextFactory;

    public ProjectRepository(IDbContextFactory<TerrariumDbContext> contextFactory) => _contextFactory = contextFactory;

    public async Task<ProjectEntity?> GetByIdAsync(string id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task AddAsync(ProjectEntity entity)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        // Prevent disconnected graph errors
        entity.Workspace = null!;
        
        context.Projects.Add(entity);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(string id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var entity = await context.Projects.FindAsync(id);
        if (entity != null)
        {
            entity.IsDeleted = true;
            entity.LastModifiedUtc = DateTime.UtcNow;
            await context.SaveChangesAsync();
        }
    }
}