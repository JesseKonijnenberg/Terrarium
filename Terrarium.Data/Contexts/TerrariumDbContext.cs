using Microsoft.EntityFrameworkCore;
using Terrarium.Core.Models;
using Terrarium.Core.Models.Hierarchy;
using Terrarium.Core.Models.Kanban;

namespace Terrarium.Data.Contexts;

/// <summary>
/// Represents the database context for the Terrarium application, managing entity sets and database configuration.
/// </summary>
public class TerrariumDbContext : DbContext
{
    public DbSet<OrganizationEntity> Organizations { get; set; }
    public DbSet<WorkspaceEntity> Workspaces { get; set; }
    public DbSet<ProjectEntity> Projects { get; set; }
    public DbSet<IterationEntity> Iterations { get; set; }
    public DbSet<ColumnEntity> Columns { get; set; }
    public DbSet<TaskEntity> Tasks { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TerrariumDbContext"/> class with the specified options.
    /// </summary>
    /// <param name="options">The options to be used by a <see cref="DbContext"/>.</param>
    public TerrariumDbContext(DbContextOptions<TerrariumDbContext> options)
        : base(options)
    {
    }
    
    /// <summary>
    /// AUTOMATIC SYNC METADATA: 
    /// This interceptor automatically updates LastModifiedUtc before saving to disk.
    /// </summary>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.Entity is EntityBase && (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entityEntry in entries)
        {
            ((EntityBase)entityEntry.Entity).LastModifiedUtc = DateTime.UtcNow;
        }

        return base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Configures the model relationships and seeds initial data.
    /// </summary>
    /// <param name="modelBuilder">The builder being used to construct the model for this context.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Relationships: Task -> Column
        modelBuilder.Entity<TaskEntity>()
            .HasOne(t => t.Column)
            .WithMany(c => c.Tasks)
            .HasForeignKey(t => t.ColumnId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<TaskEntity>()
            .HasOne(t => t.Workspace)
            .WithMany() // Or .WithMany(w => w.Tasks) if you add that collection to WorkspaceEntity
            .HasForeignKey(t => t.WorkspaceId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relationships: Workspace -> Organization (Nullable for Solo users)
        modelBuilder.Entity<WorkspaceEntity>()
            .HasOne(w => w.Organization)
            .WithMany(o => o.Workspaces)
            .HasForeignKey(w => w.OrganizationId)
            .OnDelete(DeleteBehavior.SetNull);

        // Relationships: Project -> Workspace
        modelBuilder.Entity<ProjectEntity>()
            .HasOne(p => p.Workspace)
            .WithMany(w => w.Projects)
            .HasForeignKey(p => p.WorkspaceId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<ColumnEntity>()
            .HasOne(c => c.Workspace)
            .WithMany() // Workspace doesn't necessarily need a List<Column> property
            .HasForeignKey(c => c.WorkspaceId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<ColumnEntity>()
            .HasOne(c => c.Iteration)
            .WithMany()
            .HasForeignKey(c => c.IterationId)
            .OnDelete(DeleteBehavior.SetNull);
        
        var seedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Seed a Default Personal Workspace
        modelBuilder.Entity<WorkspaceEntity>().HasData(
            new WorkspaceEntity 
            { 
                Id = "solo-workspace", 
                Name = "Personal Workspace", 
                IsPersonal = true, 
                LastModifiedUtc = seedDate 
            }
        );

        // Seed Default Columns linked to the Workspace (Optional: Can also link to Project)
        modelBuilder.Entity<ColumnEntity>().HasData(
            new ColumnEntity { Id = "col-1", Title = "Backlog", Order = 0, LastModifiedUtc = seedDate, WorkspaceId = "solo-workspace" },
            new ColumnEntity { Id = "col-2", Title = "In Progress", Order = 1, LastModifiedUtc = seedDate, WorkspaceId = "solo-workspace" },
            new ColumnEntity { Id = "col-3", Title = "Review", Order = 2, LastModifiedUtc = seedDate, WorkspaceId = "solo-workspace" },
            new ColumnEntity { Id = "col-4", Title = "Complete", Order = 3, LastModifiedUtc = seedDate, WorkspaceId = "solo-workspace" }
        );
    }
}