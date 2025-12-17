using System;
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

        private bool _isUpdateAvailable;
        public bool IsUpdateAvailable
        {
            get => _isUpdateAvailable;
            set
            {
                _isUpdateAvailable = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowStartButton));
            }
        }

        private bool _isUpdating;
        public bool IsUpdating
        {
            get => _isUpdating;
            set
            {
                _isUpdating = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowStartButton));
                OnPropertyChanged(nameof(ShowProgressPanel));
            }
        }

        private bool _isRestartPending;
        public bool IsRestartPending
        {
            get => _isRestartPending;
            set
            {
                _isRestartPending = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowStartButton));
            }
        }

        private bool _isCancelling;
        public bool IsCancelling
        {
            get => _isCancelling;
            set
            {
                _isCancelling = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowStartButton));
                OnPropertyChanged(nameof(ShowProgressPanel));
            }
        }

        private int _updateProgress;
        public int UpdateProgress
        {
            get => _updateProgress;
            set { _updateProgress = value; OnPropertyChanged(); }
        }

        private string _updateButtonText = "Check for Updates";
        public string UpdateButtonText { get => _updateButtonText; set { _updateButtonText = value; OnPropertyChanged(); } }


        public bool ShowStartButton => IsUpdateAvailable && !IsUpdating && !IsRestartPending && !IsCancelling;

        public bool ShowProgressPanel => IsUpdating && !IsCancelling;

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
                System.Diagnostics.Debug.WriteLine($"Update Failed: {ex}");
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