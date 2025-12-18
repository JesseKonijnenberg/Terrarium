using Microsoft.EntityFrameworkCore;
using Terrarium.Core.Models.Kanban;

namespace Terrarium.Data.Contexts
{
    public class TerrariumDbContext : DbContext
    {
        public DbSet<ColumnEntity> Columns { get; set; }
        public DbSet<TaskEntity> Tasks { get; set; }

        public TerrariumDbContext(DbContextOptions<TerrariumDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Define the Relationship: One Column <-> Many Tasks
            modelBuilder.Entity<TaskEntity>()
                .HasOne(t => t.Column)
                .WithMany(c => c.Tasks)
                .HasForeignKey(t => t.ColumnId)
                .OnDelete(DeleteBehavior.Cascade); // Deleting a column kills its tasks

            // Seed Default Data (So the board isn't empty on first run)
            modelBuilder.Entity<ColumnEntity>().HasData(
                new ColumnEntity { Id = "col-1", Title = "Backlog" },
                new ColumnEntity { Id = "col-2", Title = "In Progress" },
                new ColumnEntity { Id = "col-3", Title = "Review" },
                new ColumnEntity { Id = "col-4", Title = "Complete" }
            );
        }
    }
}