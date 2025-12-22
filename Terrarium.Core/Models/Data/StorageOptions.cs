using System;
using System.IO;

namespace Terrarium.Core.Models.Data
{
    public class StorageOptions
    {
        public string BasePath { get; private set; }
        public string DatabaseFileName { get; set; } = "local.db";

        public StorageOptions()
        {
            string root = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

#if DEBUG
            BasePath = Path.Combine(root, "Terrarium_Dev");
#else
            BasePath = Path.Combine(root, "Terrarium");
#endif

            if (!Directory.Exists(BasePath))             // Ensure the directory exists as soon as the options are created
            {
                Directory.CreateDirectory(BasePath);
            }
        }

        public string ConnectionString => $"Data Source={Path.Combine(BasePath, DatabaseFileName)}";

        public string BackupFilePath => Path.Combine(BasePath, "board_backup.md");
    }
}