using Terrarium.Core.Events.Kanban;
using Terrarium.Core.Interfaces.Kanban;
using Terrarium.Core.Models.Data;

namespace Terrarium.Logic.Services.Kanban
{
    public class BackupService(IBoardService boardService, IBoardSerializer serializer, StorageOptions storageOptions) : IBackupService
    {
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private int _delayTime = 2000;
        
        public void Initialize()
        {
            if (boardService != null)
            {
                boardService.BoardChanged += OnBoardChanged;
            }
        }

        private async void OnBoardChanged(object sender, BoardChangedEventsArgs e)
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            
            _cancellationTokenSource = new CancellationTokenSource();
            
            var token = _cancellationTokenSource.Token;
            try
            {
                await Task.Delay(_delayTime, token);
                
                await Save();
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Backup error: {ex.Message}");
            }
        }

        private async Task Save()
        {
            var board = boardService.GetCachedBoard();
            var markdown = serializer.ToMarkdown(board);
            await File.WriteAllTextAsync(storageOptions.BackupFilePath, markdown);
        }
    }
}

