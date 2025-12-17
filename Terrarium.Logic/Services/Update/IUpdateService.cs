using System;
using System.Threading;
using System.Threading.Tasks;

namespace Terrarium.Logic.Services.Update
{
    public interface IUpdateService
    {
        Task<string?> CheckForUpdatesAsync();
        Task DownloadUpdatesAsync(Action<int> progress, CancellationToken token);
        void ApplyUpdatesAndRestart();
    }
}