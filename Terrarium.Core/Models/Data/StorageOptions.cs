namespace Terrarium.Core.Models.Data
{
    /// <summary>
    /// Configuration options for data storage, managing file paths, connection strings, 
    /// and template locations for the application.
    /// </summary>
    public class StorageOptions
    {
        private const string DefaultTemplateFileName = "default_board.md";
        private const string CustomTemplateFileName = "custom_board.md";

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
            
            if (!Directory.Exists(BasePath))
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

        /// <summary>
        /// Gets the path to the Markdown template to be used for backups.
        /// Prioritizes a user-defined "custom_board.md" in the AppData folder,
        /// falling back to the "default_board.md" in the application assets.
        /// </summary>
        public string TemplateFilePath
        {
            get
            {
                string customPath = Path.Combine(BasePath, CustomTemplateFileName);
                if (File.Exists(customPath))
                {
                    return customPath;
                }
                
                return Path.Combine(AppContext.BaseDirectory, "Assets", "Templates", DefaultTemplateFileName);
            }
        }
    }
}