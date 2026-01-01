using System;
using System.IO;

namespace Terrarium.Core.Models.Data
{
    /// <summary>
    /// Configuration options for data storage, managing file paths and connection strings for the application.
    /// </summary>
    public class StorageOptions
    {
        /// <summary>
        /// Gets the base directory path where application data is stored.
        /// This path varies depending on the build configuration (Debug vs Release).
        /// </summary>
        public string BasePath { get; private set; }

        /// <summary>
        /// Gets or sets the name of the database file. Defaults to "local.db".
        /// </summary>
        public string DatabaseFileName { get; set; } = "local.db";

        /// <summary>
        /// Initializes a new instance of the <see cref="StorageOptions"/> class.
        /// Sets up the base path and ensures the storage directory exists.
        /// </summary>
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

        /// <summary>
        /// Gets the connection string for the SQLite database.
        /// </summary>
        public string ConnectionString => $"Data Source={Path.Combine(BasePath, DatabaseFileName)}";

        /// <summary>
        /// Gets the full file path for the board backup file.
        /// </summary>
        public string BackupFilePath => Path.Combine(BasePath, "board_backup.md");
    }
}