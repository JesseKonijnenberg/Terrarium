using Microsoft.EntityFrameworkCore;
using Terrarium.Core.Models;
using Terrarium.Core.Models.Kanban;

namespace Terrarium.Data.Contexts;

/// <summary>
/// Represents the database context for the Terrarium application, managing entity sets and database configuration.
/// </summary>
public class TerrariumDbContext : DbContext
{
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
        // Define the Relationship: One Column <-> Many Tasks
        modelBuilder.Entity<TaskEntity>()
            .HasOne(t => t.Column)
            .WithMany(c => c.Tasks)
            .HasForeignKey(t => t.ColumnId)
            .OnDelete(DeleteBehavior.Cascade); // Deleting a column kills its tasks
        
        // Global Query Filter: Automatically hide "Soft Deleted" items from the UI
        modelBuilder.Entity<TaskEntity>().HasQueryFilter(t => !t.IsDeleted);
        modelBuilder.Entity<ColumnEntity>().HasQueryFilter(c => !c.IsDeleted);
        
        var seedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        modelBuilder.Entity<ColumnEntity>().HasData(
            new ColumnEntity { Id = "col-1", Title = "Backlog", Order = 0, LastModifiedUtc = seedDate },
            new ColumnEntity { Id = "col-2", Title = "In Progress", Order = 1, LastModifiedUtc = seedDate },
            new ColumnEntity { Id = "col-3", Title = "Review", Order = 2, LastModifiedUtc = seedDate },
            new ColumnEntity { Id = "col-4", Title = "Complete", Order = 3, LastModifiedUtc = seedDate }
        );
    }
}