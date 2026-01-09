using Microsoft.EntityFrameworkCore;
using Terrarium.Data.Contexts;

namespace Terrarium.Data;

/// <summary>
/// Provides functionality to initialize the database and ensure the environment is ready for data operations.
/// </summary>
public static class DbInitializer
{
    /// <summary>
    /// Initializes the database by ensuring the storage directory exists and applying any pending migrations.
    /// </summary>
    /// <param name="context">The database context to initialize.</param>
    public static void Initialize(TerrariumDbContext context)
    {
        // SMART PATH DETECTION
        // We extract the path directly from the configured connection string.
        // This works even if you change StorageOptions later.
        var connectionString = context.Database.GetConnectionString();

        if (connectionString != null && connectionString.Contains("Data Source="))
        {
            var dbPath = connectionString.Replace("Data Source=", "").Trim();
            var dbFolder = Path.GetDirectoryName(dbPath);

            if (!string.IsNullOrEmpty(dbFolder) && !Directory.Exists(dbFolder))
            {
                Directory.CreateDirectory(dbFolder);
            }
        }

        context.Database.Migrate();
    }
}