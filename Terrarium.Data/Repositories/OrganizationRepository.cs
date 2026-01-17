using Terrarium.Core.Interfaces.Repositories;
using Terrarium.Core.Models.Hierarchy;
using Terrarium.Data.Contexts;

namespace Terrarium.Data.Repositories;

public class OrganizationRepository : IOrganizationRepository
{
    private readonly TerrariumDbContext _context;

    public OrganizationRepository(TerrariumDbContext context)
    {
        _context = context;
    }

    public async Task<OrganizationEntity?> GetByIdAsync(string id)
    {
        return await _context.Organizations.FindAsync(id);
    }

    public async Task AddAsync(OrganizationEntity entity)
    {
        _context.Organizations.Add(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(string id)
    {
        var entity = await _context.Organizations.FindAsync(id);
        if (entity != null)
        {
            _context.Organizations.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}