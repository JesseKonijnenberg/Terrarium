namespace Terrarium.Core.Interfaces.Update;

/// <summary>
/// Defines a contract for checking, downloading, and applying application updates.
/// </summary>
/// <remarks>
/// This service handles the orchestration of the update lifecycle, ensuring the application 
/// stays current with the latest releases from the distribution source.
/// </remarks>
public interface IUpdateService
{
    /// <summary>
    /// Queries the update source to determine if a newer version of the application is available.
    /// </summary>
    /// <returns>
    /// A string representing the new version number if an update is available; 
    /// otherwise, <see langword="null"/>.
    /// </returns>
    Task<string?> CheckForUpdatesAsync();

    /// <summary>
    /// Initiates the download of the update package.
    /// </summary>
    /// <param name="progress">A callback action used to report download progress (0-100).</param>
    /// <param name="token">A token to monitor for cancellation requests.</param>
    Task DownloadUpdatesAsync(Action<int> progress, CancellationToken token);

    /// <summary>
    /// Applies the downloaded update and restarts the application to complete the process.
    /// </summary>
    /// <remarks>
    /// This should only be called after <see cref="DownloadUpdatesAsync"/> has completed successfully.
    /// </remarks>
    void ApplyUpdatesAndRestart();
}