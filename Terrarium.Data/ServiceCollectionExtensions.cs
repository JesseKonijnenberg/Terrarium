using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Terrarium.Core.Interfaces;
using Terrarium.Core.Models.Data;
using Terrarium.Data.Contexts;
using Terrarium.Data.Repositories;

namespace Terrarium.Data
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddTerrariumData(this IServiceCollection services, StorageOptions options)
        {
            // Register Database Context
            services.AddDbContext<TerrariumDbContext>(opts =>
                opts.UseSqlite(options.ConnectionString), ServiceLifetime.Transient);

            // Register Repositories
            // Always register these as Transient when using EF Core in this setup
            services.AddTransient<IBoardRepository, SqliteBoardRepository>();

            // Future Garden Repository will go here:
            // services.AddTransient<IGardenRepository, SqliteGardenRepository>();

            return services;
        }
    }
}