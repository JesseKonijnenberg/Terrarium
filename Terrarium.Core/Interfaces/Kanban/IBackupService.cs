namespace Terrarium.Core.Interfaces.Kanban;

/// <summary>
/// Defines a contract for a service that monitors board changes and creates automated backups.
/// </summary>
/// <remarks>
/// This service is designed to trigger background snapshots after a period of user inactivity 
/// (debouncing) to ensure data safety without performance degradation.
/// </remarks>
public interface IBackupService
{
    /// <summary>
    /// Subscribes to board events and prepares the service for background operations.
    /// </summary>
    void Initialize();
}