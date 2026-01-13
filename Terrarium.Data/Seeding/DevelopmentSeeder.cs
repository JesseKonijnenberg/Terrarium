using Terrarium.Core.Interfaces.Data;
using Terrarium.Core.Models.Hierarchy;
using Terrarium.Data.Contexts;

namespace Terrarium.Data.Seeding;

public class DevelopmentSeeder : IDatabaseSeeder
{
    private readonly TerrariumDbContext _context;

    public DevelopmentSeeder(TerrariumDbContext context)
    {
        _context = context;
    }

    public void Seed()
    {
        // Avoid duplicate seeding
        if (_context.Organizations.Any(o => o.Id == "dev-corp-id")) return;

        var devOrg = new OrganizationEntity
        {
            Id = "dev-corp-id",
            Name = "Dev Solutions Inc.",
            ActiveThemeId = "sakura-night", 
            LockTheme = false,
            Workspaces = new List<WorkspaceEntity>
            {
                new WorkspaceEntity 
                { 
                    Id = Guid.NewGuid().ToString(), 
                    Name = "Research & Development",
                    IsPersonal = false
                }
            }
        };

        _context.Organizations.Add(devOrg);
        _context.SaveChanges();
    }
}