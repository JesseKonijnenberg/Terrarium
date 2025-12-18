using System;
using System.IO;

namespace Terrarium.Core.Models.Data
{
    public class StorageOptions
    {
        public string BasePath { get; set; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Terrarium"
        );

        public string DatabaseFileName { get; set; } = "terrarium.db";

        private string? _connectionString;

        public string ConnectionString
        {
            get
            {
                if (!string.IsNullOrEmpty(_connectionString))
                {
                    return _connectionString;
                }
                return $"Data Source={Path.Combine(BasePath, DatabaseFileName)}";
            }
            set => _connectionString = value;
        }
    }
}