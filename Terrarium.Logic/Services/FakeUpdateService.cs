namespace Terrarium.Logic.Services
{
    public class FakeUpdateService : IUpdateService
    {
        private string _fakeVersion = "9.9.9 (Test)";

        public async Task<string?> CheckForUpdatesAsync()
        {
            await Task.Delay(1000);
            return _fakeVersion;
        }

        public async Task DownloadUpdatesAsync(Action<int> progress, CancellationToken token)
        {
            for (int i = 0; i <= 100; i++)
            {
                if (token.IsCancellationRequested)
                {
                    throw new OperationCanceledException();
                }

                progress(i);

                await Task.Delay(30, token);
            }
        }

        public void ApplyUpdatesAndRestart()
        {
            System.Diagnostics.Debug.WriteLine("FAKE SERVICE: ApplyUpdatesAndRestart called.");
        }
    }
}