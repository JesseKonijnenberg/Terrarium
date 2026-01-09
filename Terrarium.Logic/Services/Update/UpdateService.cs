using Terrarium.Core.Interfaces.Update;
using Velopack;
using Velopack.Sources;

namespace Terrarium.Logic.Services.Update;

/// <summary>
/// A concrete implementation of <see cref="IUpdateService"/> utilizing the Velopack library 
/// for GitHub-based distribution.
/// </summary>
public class UpdateService : IUpdateService
{
    private readonly UpdateManager _manager;
    private UpdateInfo? _cachedUpdateInfo;

    public UpdateService()
    {
        var source = new GithubSource("https://github.com/JesseKonijnenberg/Terrarium", null, false);
        _manager = new UpdateManager(source);
    }

    /// <inheritdoc />
    public async Task<string?> CheckForUpdatesAsync()
    {
        try
        {
            if (!_manager.IsInstalled) return null;

            _cachedUpdateInfo = await _manager.CheckForUpdatesAsync();

            return _cachedUpdateInfo?.TargetFullRelease?.Version?.ToString();
        }
        catch
        {
            return null;
        }
    }

    /// <inheritdoc />
    public async Task DownloadUpdatesAsync(Action<int> progress, CancellationToken token)
    {
        if (_cachedUpdateInfo == null) return;

        await _manager.DownloadUpdatesAsync(_cachedUpdateInfo, progress, cancelToken: token);
    }

    /// <inheritdoc />
    public void ApplyUpdatesAndRestart()
    {
        if (_cachedUpdateInfo == null) return;
        _manager.ApplyUpdatesAndRestart(_cachedUpdateInfo);
    }
}