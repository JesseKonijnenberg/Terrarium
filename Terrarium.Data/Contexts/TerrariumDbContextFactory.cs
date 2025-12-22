using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Terrarium.Core.Models.Data;
using Terrarium.Data.Contexts;

namespace Terrarium.Data
{
    public class TerrariumDbContextFactory : IDesignTimeDbContextFactory<TerrariumDbContext>
    {
        public TerrariumDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<TerrariumDbContext>();

            var options = new StorageOptions();
            optionsBuilder.UseSqlite(options.ConnectionString);

            return new TerrariumDbContext(optionsBuilder.Options);
        }
    }
}