namespace Terrarium.Core.Interfaces.Update
{
    public interface IUpdateService
    {
        Task<string?> CheckForUpdatesAsync();
        Task DownloadUpdatesAsync(Action<int> progress, CancellationToken token);
        void ApplyUpdatesAndRestart();
    }
}