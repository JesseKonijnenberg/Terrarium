using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Terrarium.Avalonia.ViewModels.Core;
using Terrarium.Logic.Services.Update;

namespace Terrarium.Avalonia.ViewModels
{
    public class UpdateViewModel : ViewModelBase
    {
        private readonly IUpdateService _updateService;
        private CancellationTokenSource? _updateCts;
        private string _foundVersion = "";

        public ICommand UpdateCommand { get; }
        public ICommand CancelUpdateCommand { get; }
        public ICommand RestartCommand { get; }
        
        public bool ShowStartButton => IsUpdateAvailable && !IsUpdating && !IsRestartPending && !IsCancelling;
        public bool ShowProgressPanel => IsUpdating && !IsCancelling;

        public bool IsUpdateAvailable
        {
            get;
            set
            {
                field = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowStartButton));
            }
        }

        public bool IsUpdating
        {
            get;
            set
            {
                field = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowStartButton));
                OnPropertyChanged(nameof(ShowProgressPanel));
            }
        }

        public bool IsRestartPending
        {
            get;
            set
            {
                field = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowStartButton));
            }
        }

        public bool IsCancelling
        {
            get;
            set
            {
                field = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowStartButton));
                OnPropertyChanged(nameof(ShowProgressPanel));
            }
        }

        public int UpdateProgress
        {
            get;
            set
            {
                field = value;
                OnPropertyChanged();
            }
        }

        public string UpdateButtonText
        {
            get;
            set
            {
                field = value;
                OnPropertyChanged();
            }
        } = "Check for Updates";
        
        public UpdateViewModel()
        {
            _updateService = new UpdateService();

            UpdateCommand = new RelayCommand(ExecuteUpdate);
            CancelUpdateCommand = new RelayCommand(ExecuteCancelUpdate);
            RestartCommand = new RelayCommand(ExecuteRestart);

            Task.Run(CheckForUpdates);
        }

        private async Task CheckForUpdates()
        {
            string? newVersion = await _updateService.CheckForUpdatesAsync();
            if (!string.IsNullOrEmpty(newVersion))
            {
                _foundVersion = newVersion;
                IsUpdateAvailable = true;
                UpdateButtonText = $"Update to {newVersion}";
            }
        }

        private async void ExecuteUpdate(object? param)
        {
            if (!IsUpdateAvailable) return;

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
                IsCancelling = true;
                IsUpdating = false;

                UpdateButtonText = "Cancelled";

                await Task.Delay(1000);

                IsCancelling = false;
                UpdateProgress = 0;
                UpdateButtonText = $"Update to {_foundVersion}";
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

        private void ExecuteCancelUpdate(object? param) => _updateCts?.Cancel();
        private void ExecuteRestart(object? param) => _updateService.ApplyUpdatesAndRestart();
    }
}