using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Terrarium.Avalonia.ViewModels.Core;
using Terrarium.Core.Interfaces.Update;

namespace Terrarium.Avalonia.ViewModels;

public partial class UpdateViewModel : ViewModelBase
{
    private readonly IUpdateService _updateService;
    private CancellationTokenSource? _updateCts;
    private string _foundVersion = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowStartButton))]
    private bool _isUpdateAvailable;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowStartButton))]
    [NotifyPropertyChangedFor(nameof(ShowProgressPanel))]
    private bool _isUpdating;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowStartButton))]
    private bool _isRestartPending;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowStartButton))]
    [NotifyPropertyChangedFor(nameof(ShowProgressPanel))]
    private bool _isCancelling;

    [ObservableProperty]
    private int _updateProgress;

    [ObservableProperty]
    private string _updateButtonText = "Check for Updates";

    public bool ShowStartButton => IsUpdateAvailable && !IsUpdating && !IsRestartPending && !IsCancelling;
    public bool ShowProgressPanel => IsUpdating && !IsCancelling;

    public UpdateViewModel(IUpdateService updateService)
    {
        _updateService = updateService;
        _ = CheckForUpdatesAsync();
    }

    private async Task CheckForUpdatesAsync()
    {
        string? newVersion = await _updateService.CheckForUpdatesAsync();
        if (!string.IsNullOrEmpty(newVersion))
        {
            _foundVersion = newVersion;
            IsUpdateAvailable = true;
            UpdateButtonText = $"Update to {newVersion}";
        }
    }

    [RelayCommand(CanExecute = nameof(CanUpdate))]
    private async Task UpdateAsync()
    {
        IsUpdating = true;
        UpdateButtonText = "Downloading...";
        _updateCts = new CancellationTokenSource();

        try
        {
            await _updateService.DownloadUpdatesAsync((progress) =>
            {
                UpdateProgress = progress;
                UpdateButtonText = $"Downloading {progress}%";
            }, _updateCts.Token);

            IsUpdating = false;
            IsRestartPending = true;
            UpdateButtonText = "Restart Required";
        }
        catch (OperationCanceledException)
        {
            await HandleCancellationAsync();
        }
        catch (Exception ex)
        {
            IsUpdating = false;
            UpdateButtonText = "Failed";
            Debug.WriteLine($"Update Failed: {ex}");
        }
        finally
        {
            _updateCts?.Dispose();
            _updateCts = null;
        }
    }

    private bool CanUpdate() => IsUpdateAvailable && !IsUpdating;

    [RelayCommand]
    private void CancelUpdate() => _updateCts?.Cancel();

    [RelayCommand]
    private void Restart() => _updateService.ApplyUpdatesAndRestart();

    private async Task HandleCancellationAsync()
    {
        IsCancelling = true;
        IsUpdating = false;
        UpdateButtonText = "Cancelled";

        await Task.Delay(1000);

        IsCancelling = false;
        UpdateProgress = 0;
        UpdateButtonText = $"Update to {_foundVersion}";
    }
}