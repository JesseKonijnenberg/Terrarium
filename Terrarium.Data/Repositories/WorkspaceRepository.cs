using Terrarium.Core.Interfaces.Repositories;
using Terrarium.Core.Models.Hierarchy;
using Terrarium.Data.Contexts;

namespace Terrarium.Data.Repositories;

public class WorkspaceRepository : IWorkspaceRepository
{
    private readonly TerrariumDbContext _context;

    public WorkspaceRepository(TerrariumDbContext context) => _context = context;

    public async Task<WorkspaceEntity?> GetByIdAsync(string id) => await _context.Workspaces.FindAsync(id);

    public async Task AddAsync(WorkspaceEntity entity)
    {
        _context.Workspaces.Add(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(string id)
    {
        var entity = await _context.Workspaces.FindAsync(id);
        if (entity != null)
        {
            _context.Workspaces.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}