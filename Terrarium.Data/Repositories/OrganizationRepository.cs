using Microsoft.EntityFrameworkCore;
using Terrarium.Core.Interfaces.Repositories;
using Terrarium.Core.Models.Hierarchy;
using Terrarium.Data.Contexts;

namespace Terrarium.Data.Repositories;

public class OrganizationRepository : IOrganizationRepository
{
    private readonly IDbContextFactory<TerrariumDbContext> _contextFactory;

    public OrganizationRepository(IDbContextFactory<TerrariumDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<OrganizationEntity?> GetByIdAsync(string id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Organizations.AsNoTracking().FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task AddAsync(OrganizationEntity entity)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        context.Organizations.Add(entity);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(string id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var entity = await context.Organizations.FindAsync(id);
        if (entity != null)
        {
            context.Organizations.Remove(entity);
            await context.SaveChangesAsync();
        }
    }
}