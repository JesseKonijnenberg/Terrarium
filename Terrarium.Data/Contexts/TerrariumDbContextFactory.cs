using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Terrarium.Core.Models.Data;
using Terrarium.Data.Contexts;

namespace Terrarium.Data;

/// <summary>
/// A factory for creating instances of <see cref="TerrariumDbContext"/> at design time.
/// This is primarily used by Entity Framework Core tools for migrations.
/// </summary>
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