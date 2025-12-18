using System.IO;
using Microsoft.EntityFrameworkCore;
using Terrarium.Data.Contexts;

namespace Terrarium.Data
{
    public static class DbInitializer
    {
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
}