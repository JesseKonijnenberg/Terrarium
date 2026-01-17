using Terrarium.Core.Interfaces.Repositories;
using Terrarium.Core.Models.Hierarchy;
using Terrarium.Data.Contexts;

namespace Terrarium.Data.Repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly TerrariumDbContext _context;

    public ProjectRepository(TerrariumDbContext context) => _context = context;

    public async Task<ProjectEntity?> GetByIdAsync(string id) => await _context.Projects.FindAsync(id);

    public async Task AddAsync(ProjectEntity entity)
    {
        _context.Projects.Add(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(string id)
    {
        var entity = await _context.Projects.FindAsync(id);
        if (entity != null)
        {
            _context.Projects.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}