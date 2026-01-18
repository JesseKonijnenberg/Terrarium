using Microsoft.EntityFrameworkCore;
using Terrarium.Core.Interfaces.Data;
using Terrarium.Core.Models.Hierarchy;
using Terrarium.Data.Contexts;

namespace Terrarium.Data.Seeding;

public class DevelopmentSeeder : IDatabaseSeeder
{
    private readonly IDbContextFactory<TerrariumDbContext> _contextFactory;

    public DevelopmentSeeder(IDbContextFactory<TerrariumDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public void Seed()
    {
        using var context = _contextFactory.CreateDbContext();
        if (context.Organizations.Any(o => o.Id == "dev-corp-id")) return;

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
                    Id = "dev-workspace-id", 
                    Name = "Research & Development",
                    IsPersonal = false,
                    // ADD PROJECTS HERE
                    Projects = new List<ProjectEntity>
                    {
                        new ProjectEntity 
                        { 
                            Id = "default-project-id", 
                            Name = "Main Project",
                            Description = "Auto-generated project for development"
                        },
                        new ProjectEntity 
                        { 
                            Id = "default-project-id2", 
                            Name = "Second Project",
                            Description = "Auto-generated second project for development"
                        }
                    }
                    
                }
            }
        };

        context.Organizations.Add(devOrg);
        context.SaveChanges();
    }
}