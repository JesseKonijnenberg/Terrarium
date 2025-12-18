using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Terrarium.Data.Contexts;

namespace Terrarium.Data
{
    public class TerrariumDbContextFactory : IDesignTimeDbContextFactory<TerrariumDbContext>
    {
        public TerrariumDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<TerrariumDbContext>();
            optionsBuilder.UseSqlite("Data Source=terrarium.db");

            return new TerrariumDbContext(optionsBuilder.Options);
        }
    }
}