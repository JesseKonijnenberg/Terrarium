using Terrarium.Core.Events.Kanban;
using Terrarium.Core.Interfaces.Kanban;
using Terrarium.Core.Models.Data;

namespace Terrarium.Logic.Services.Kanban;

public class BackupService(
    IBoardService boardService, 
    IBoardSerializer serializer, 
    StorageOptions storageOptions) : IBackupService
{
    private CancellationTokenSource _cancellationTokenSource = new();
    
    /// <summary> The delay in milliseconds to wait before triggering a backup. </summary>
    private readonly int _delayTime = 2000;
    
    /// <inheritdoc />
    public void Initialize()
    {
        if (boardService != null)
        {
            boardService.BoardChanged += OnBoardChanged;
        }
    }
    
    /// <summary>
    /// Resets the backup timer every time a change is detected.
    /// </summary>
    private async void OnBoardChanged(object? sender, BoardChangedEventsArgs e)
    {
        // Cancel the previous pending backup attempt
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
        
        _cancellationTokenSource = new CancellationTokenSource();
        var token = _cancellationTokenSource.Token;

        try
        {
            // Wait for user inactivity
            await Task.Delay(_delayTime, token);
            
            await Save();
        }
        catch (OperationCanceledException)
        {
            // Expected when a new change triggers a reset
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Backup error: {ex.Message}");
        }
    }

    /// <summary>
    /// Serializes the current board state and writes it to the designated backup file.
    /// </summary>
    private async Task Save()
    {
        var board = boardService.GetCachedBoard();
        var markdown = serializer.ToMarkdown(board);
        await File.WriteAllTextAsync(storageOptions.BackupFilePath, markdown);
    }
}