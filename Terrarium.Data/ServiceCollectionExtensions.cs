using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Terrarium.Core.Interfaces.Kanban;
using Terrarium.Core.Models.Data;
using Terrarium.Data.Contexts;
using Terrarium.Data.Repositories;

namespace Terrarium.Data;

/// <summary>
/// Extension methods for setting up data layer services in an <see cref="IServiceCollection" />.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers Terrarium data services, including the database context and repositories.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <param name="options">Configuration options for data storage.</param>
    /// <returns>The <see cref="IServiceCollection" /> so that additional calls can be chained.</returns>
    public static IServiceCollection AddTerrariumData(this IServiceCollection services, StorageOptions options)
    {
        // Register Database Context
        services.AddDbContext<TerrariumDbContext>(opts =>
            opts.UseSqlite(options.ConnectionString), ServiceLifetime.Transient);

        // Register Repositories
        // Always register these as Transient when using EF Core in this setup
        services.AddTransient<IBoardRepository, BoardRepository>();

        // Future Garden Repository will go here:
        // services.AddTransient<IGardenRepository, SqliteGardenRepository>();

        return services;
    }
}