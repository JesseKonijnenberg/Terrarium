using Avalonia.Media;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Terrarium.Logic.Services;

namespace Terrarium.Avalonia.ViewModels
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Func<object?, bool>? _canExecute;
        public event EventHandler? CanExecuteChanged;
        public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }
        public bool CanExecute(object? parameter) => _canExecute == null || _canExecute(parameter);
        public void Execute(object? parameter) => _execute(parameter);
    }

    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public static class ObservableCollectionExtensions
    {
        public static void Move<T>(this ObservableCollection<T> collection, int oldIndex, int newIndex)
        {
            if (oldIndex < 0 || newIndex < 0 || oldIndex >= collection.Count || newIndex >= collection.Count) return;
            if (oldIndex == newIndex) return;
            var item = collection[oldIndex];
            collection.RemoveAt(oldIndex);
            collection.Insert(newIndex, item);
        }
    }

    public class TaskItem : ViewModelBase
    {
        public string Id { get; set; } = "";
        public string Content { get; set; } = "";
        private string _tag = "";
        public string Tag { get => _tag; set => _tag = value.ToUpper(); }
        public string Priority { get; set; } = "";
        public string Date { get; set; } = "";
        public IBrush TagBgColor => GetTagBrush(Tag, 0.3);
        public IBrush TagTextColor => GetTagBrush(Tag, 1.0);
        public IBrush TagBorderColor => GetTagBrush(Tag, 0.5);
        public bool IsHighPriority => Priority == "High";

        private IBrush GetTagBrush(string tag, double opacity)
        {
            var colorStr = tag.ToUpper() switch { "DESIGN" => "#a65d57", "DEV" => "#4a5c6a", "MARKETING" => "#cca43b", "PRODUCT" => "#5e6c5b", _ => "#5e6c5b" };
            var color = Color.Parse(colorStr);
            var finalColor = new Color((byte)(255 * opacity), color.R, color.G, color.B);
            return new SolidColorBrush(finalColor);
        }
    }

    public class Column : ViewModelBase
    {
        public string Id { get; set; } = "";
        public string Title { get; set; } = "";
        public ObservableCollection<TaskItem> Tasks { get; set; } = new();
        public int TaskCount => Tasks.Count;
        public void AddTask(TaskItem task) { if (!Tasks.Contains(task)) { Tasks.Add(task); OnPropertyChanged(nameof(TaskCount)); } }
        public void RemoveTask(TaskItem task) { if (Tasks.Contains(task)) { Tasks.Remove(task); OnPropertyChanged(nameof(TaskCount)); } }
    }

    public class KanbanBoardViewModel : ViewModelBase
    {
        private readonly UpdateService _updateService;
        private CancellationTokenSource? _updateCts;

        // FIX 1: Store the found version string here so we can use it after cancelling
        private string _foundVersion = "v9.9.9 (Test)";

        public ICommand AddItemCommand { get; }
        public ICommand DeleteTaskCommand { get; }
        public ICommand UpdateCommand { get; }
        public ICommand CancelUpdateCommand { get; }

        public ObservableCollection<Column> Columns { get; set; } = new();

        private TaskItem? _selectedTask;
        public TaskItem? SelectedTask
        {
            get => _selectedTask;
            set { _selectedTask = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsDetailPanelOpen)); }
        }
        public bool IsDetailPanelOpen => SelectedTask != null;

        private bool _isUpdateAvailable;
        public bool IsUpdateAvailable { get => _isUpdateAvailable; set { _isUpdateAvailable = value; OnPropertyChanged(); } }

        private string _updateButtonText = "Check for Updates";
        public string UpdateButtonText { get => _updateButtonText; set { _updateButtonText = value; OnPropertyChanged(); } }

        private bool _isUpdating;
        public bool IsUpdating { get => _isUpdating; set { _isUpdating = value; OnPropertyChanged(); } }

        private int _updateProgress;
        public int UpdateProgress { get => _updateProgress; set { _updateProgress = value; OnPropertyChanged(); } }

        public KanbanBoardViewModel()
        {
            _updateService = new UpdateService();
            LoadData();

            AddItemCommand = new RelayCommand(ExecuteAddItem);
            DeleteTaskCommand = new RelayCommand(ExecuteDeleteTask, CanExecuteDeleteTask);
            UpdateCommand = new RelayCommand(ExecuteFakeUpdate); // Kept your fake update
            CancelUpdateCommand = new RelayCommand(ExecuteCancelUpdate);

            // Default Test State
            IsUpdateAvailable = true;
            UpdateButtonText = $"Update to {_foundVersion}";
        }

        public void CloseDetails() => SelectedTask = null;

        public void MoveTask(TaskItem task, Column targetColumn)
        {
            var sourceColumn = Columns.FirstOrDefault(c => c.Tasks.Contains(task));
            if (sourceColumn != null && sourceColumn != targetColumn)
            {
                sourceColumn.RemoveTask(task);
                targetColumn.AddTask(task);
            }
        }

        private async Task CheckForUpdates()
        {
            string? newVersion = await _updateService.CheckForUpdatesAsync();
            if (!string.IsNullOrEmpty(newVersion))
            {
                _foundVersion = newVersion; // Store it
                IsUpdateAvailable = true;
                UpdateButtonText = $"Update to {newVersion}";
            }
        }

        // FIX 2: Updated Fake Update to respect cancellation
        private async void ExecuteFakeUpdate(object? param)
        {
            IsUpdating = true;
            _updateCts = new CancellationTokenSource();

            try
            {
                for (int i = 0; i <= 100; i++)
                {
                    // Check if user clicked cancel
                    if (_updateCts.Token.IsCancellationRequested)
                    {
                        throw new OperationCanceledException();
                    }

                    UpdateProgress = i;
                    UpdateButtonText = $"Downloading {i}%";
                    await Task.Delay(50, _updateCts.Token); // Pass token to delay
                }

                // Success
                IsUpdating = false;
                IsUpdateAvailable = false;
                UpdateButtonText = "Done";
            }
            catch (OperationCanceledException)
            {
                UpdateButtonText = "Cancelled";
                IsUpdating = false;
                UpdateProgress = 0;
                await Task.Delay(1000);
                UpdateButtonText = $"Update to {_foundVersion}";
            }
            catch
            {
                IsUpdating = false;
                UpdateButtonText = "Failed";
            }
        }

        private void ExecuteCancelUpdate(object? param)
        {
            _updateCts?.Cancel();
        }

        private void ExecuteDeleteTask(object? parameter)
        {
            if (SelectedTask is TaskItem taskToDelete)
            {
                var sourceColumn = Columns.FirstOrDefault(c => c.Tasks.Contains(taskToDelete));
                if (sourceColumn != null) { sourceColumn.RemoveTask(taskToDelete); SelectedTask = null; }
            }
        }

        private bool CanExecuteDeleteTask(object? parameter) => SelectedTask != null;

        private void ExecuteAddItem(object? parameter)
        {
            if (parameter is Column targetColumn)
            {
                int nextId = 1;
                if (Columns.Any(c => c.Tasks.Any()))
                {
                    var allTasks = Columns.SelectMany(c => c.Tasks);
                    if (allTasks.Any()) nextId = allTasks.Max(t => int.TryParse(t.Id, out int id) ? id : 0) + 1;
                }
                var newTask = new TaskItem { Id = nextId.ToString(), Content = "New Task", Tag = "New", Priority = "Low", Date = "Today" };
                targetColumn.Tasks.Insert(0, newTask);
                SelectedTask = newTask;
            }
        }

        private void LoadData()
        {
            var col1 = new Column { Id = "col-1", Title = "Backlog" };
            col1.Tasks.Add(new TaskItem { Id = "1", Content = "Design System Audit", Tag = "Design", Priority = "High", Date = "Tomorrow" });
            col1.Tasks.Add(new TaskItem { Id = "2", Content = "Q3 Marketing Assets", Tag = "Marketing", Priority = "Medium", Date = "Oct 24" });
            col1.Tasks.Add(new TaskItem { Id = "3", Content = "Update dependencies", Tag = "Dev", Priority = "Low", Date = "Oct 28" });
            var col2 = new Column { Id = "col-2", Title = "In Progress" };
            col2.Tasks.Add(new TaskItem { Id = "4", Content = "Dark Mode Implementation", Tag = "Dev", Priority = "High", Date = "Today" });
            var col3 = new Column { Id = "col-3", Title = "Review" };
            var col4 = new Column { Id = "col-4", Title = "Complete" };
            col4.Tasks.Add(new TaskItem { Id = "5", Content = "Competitor Analysis", Tag = "Product", Priority = "Medium", Date = "Oct 10" });
            Columns.Add(col1); Columns.Add(col2); Columns.Add(col3); Columns.Add(col4);
        }
    }
}