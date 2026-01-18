using Microsoft.EntityFrameworkCore;
using Terrarium.Core.Models;
using Terrarium.Core.Models.Hierarchy;
using Terrarium.Core.Models.Kanban;

namespace Terrarium.Data.Contexts;

public class TerrariumDbContext : DbContext
{
    public DbSet<OrganizationEntity> Organizations { get; set; }
    public DbSet<WorkspaceEntity> Workspaces { get; set; }
    public DbSet<ProjectEntity> Projects { get; set; }
    public DbSet<IterationEntity> Iterations { get; set; }

    public DbSet<KanbanBoardEntity> KanbanBoards { get; set; } 
    public DbSet<ColumnEntity> Columns { get; set; }
    public DbSet<TaskEntity> Tasks { get; set; }

    public TerrariumDbContext(DbContextOptions<TerrariumDbContext> options)
        : base(options)
    {
    }
    
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker
            .Entries<EntityBase>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            entry.Entity.LastModifiedUtc = DateTime.UtcNow;

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.Version++;
            }
            else if (entry.State == EntityState.Added)
            {
                entry.Entity.Version = 1;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WorkspaceEntity>()
            .HasOne(w => w.Organization)
            .WithMany(o => o.Workspaces)
            .HasForeignKey(w => w.OrganizationId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<ProjectEntity>()
            .HasOne(p => p.Workspace)
            .WithMany(w => w.Projects)
            .HasForeignKey(p => p.WorkspaceId)
            .OnDelete(DeleteBehavior.Cascade);
        

        // Project -> KanbanBoard (One Project can have a Kanban Board "Plugin")
        modelBuilder.Entity<KanbanBoardEntity>()
            .HasOne(b => b.Project)
            .WithMany() 
            .HasForeignKey(b => b.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // KanbanBoard -> Iteration (The "Sprint Switcher")
        modelBuilder.Entity<KanbanBoardEntity>()
            .HasOne(b => b.CurrentIteration)
            .WithMany()
            .HasForeignKey(b => b.CurrentIterationId)
            .OnDelete(DeleteBehavior.SetNull);

        // Column -> KanbanBoard (Columns belong to the Board, not the Project)
        modelBuilder.Entity<ColumnEntity>()
            .HasOne(c => c.KanbanBoard)
            .WithMany(b => b.Columns)
            .HasForeignKey(c => c.KanbanBoardId)
            .OnDelete(DeleteBehavior.Cascade);

        // Task -> Column
        modelBuilder.Entity<TaskEntity>()
            .HasOne(t => t.Column)
            .WithMany(c => c.Tasks)
            .HasForeignKey(t => t.ColumnId)
            .OnDelete(DeleteBehavior.Cascade);

        // Task -> Iteration (For filtering tasks by sprint)
        modelBuilder.Entity<TaskEntity>()
            .HasOne(t => t.Iteration)
            .WithMany()
            .HasForeignKey(t => t.IterationId)
            .OnDelete(DeleteBehavior.SetNull);
        
        // Global Query Filters for Soft Delete
        modelBuilder.Entity<OrganizationEntity>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<WorkspaceEntity>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ProjectEntity>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<IterationEntity>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<KanbanBoardEntity>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ColumnEntity>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<TaskEntity>().HasQueryFilter(e => !e.IsDeleted);
        
        var seedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        
        modelBuilder.Entity<WorkspaceEntity>().HasData(
            new WorkspaceEntity { Id = "solo-workspace", Name = "Personal Workspace", IsPersonal = true, OrganizationId = null, LastModifiedUtc = seedDate }
        );
        
        modelBuilder.Entity<ProjectEntity>().HasData(
            new ProjectEntity { Id = "default-project", Name = "General Tasks", WorkspaceId = "solo-workspace", LastModifiedUtc = seedDate }
        );
        
        modelBuilder.Entity<KanbanBoardEntity>().HasData(
            new KanbanBoardEntity { Id = "main-board", Name = "Development Board", ProjectId = "default-project", LastModifiedUtc = seedDate }
        );
        
        modelBuilder.Entity<ColumnEntity>().HasData(
            new ColumnEntity { Id = "col-1", Title = "Backlog", Order = 0, KanbanBoardId = "main-board", LastModifiedUtc = seedDate },
            new ColumnEntity { Id = "col-2", Title = "In Progress", Order = 1, KanbanBoardId = "main-board", LastModifiedUtc = seedDate },
            new ColumnEntity { Id = "col-3", Title = "Review", Order = 2, KanbanBoardId = "main-board", LastModifiedUtc = seedDate },
            new ColumnEntity { Id = "col-4", Title = "Complete", Order = 3, KanbanBoardId = "main-board", LastModifiedUtc = seedDate }
        );
    }
}