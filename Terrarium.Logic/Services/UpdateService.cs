using System;
using System.Threading.Tasks;
using Velopack;
using Velopack.Sources;

namespace Terrarium.Logic.Services
{
    public class UpdateService
    {
        private readonly UpdateManager _manager;

        private UpdateInfo? _cachedUpdateInfo;

        public UpdateService()
        {
            var source = new GithubSource("https://github.com/JesseKonijnenberg/Terrarium", null, false);
            _manager = new UpdateManager(source);
        }

        /// <summary>
        /// Checks for updates.
        /// Returns the Version String (e.g. "1.2.0") if an update is found.
        /// Returns NULL if no update is found.
        /// </summary>
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

        /// <summary>
        /// Downloads the update found in CheckForUpdatesAsync and restarts.
        /// </summary>
        public async Task DownloadAndRestartAsync(Action<int> progress, CancellationToken token)
        {
            if (_cachedUpdateInfo == null) return;

            await _manager.DownloadUpdatesAsync(_cachedUpdateInfo, progress, cancelToken: token);

            if (!token.IsCancellationRequested)
            {
                _manager.ApplyUpdatesAndRestart(_cachedUpdateInfo);
            }
        }
    }
}