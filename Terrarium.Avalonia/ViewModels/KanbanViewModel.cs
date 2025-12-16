using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Terrarium.Avalonia.ViewModels.Core;
using Terrarium.Avalonia.ViewModels.Models;
using Terrarium.Logic.Services;

namespace Terrarium.Avalonia.ViewModels
{
    public class KanbanBoardViewModel : ViewModelBase
    {
        // --- CHILD VIEWMODEL ---
        public UpdateViewModel Updater { get; } = new UpdateViewModel();

        // --- COMMANDS ---
        public ICommand AddItemCommand { get; }
        public ICommand DeleteTaskCommand { get; }
        public ICommand SelectTaskCommand { get; } // <--- NEW

        // --- DATA ---
        public ObservableCollection<Column> Columns { get; set; } = new();

        // --- SELECTION ---
        private TaskItem? _selectedTask;
        public TaskItem? SelectedTask
        {
            get => _selectedTask;
            set
            {
                _selectedTask = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsDetailPanelOpen));
            }
        }
        public bool IsDetailPanelOpen => SelectedTask != null;

        public KanbanBoardViewModel()
        {
            LoadData();
            AddItemCommand = new RelayCommand(ExecuteAddItem);
            DeleteTaskCommand = new RelayCommand(ExecuteDeleteTask, CanExecuteDeleteTask);
            SelectTaskCommand = new RelayCommand(ExecuteSelectTask); // <--- NEW
        }

        private void ExecuteSelectTask(object? parameter)
        {
            if (parameter is TaskItem task)
            {
                SelectedTask = task;
            }
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
                    if (allTasks.Any())
                    {
                        nextId = allTasks.Max(t => int.TryParse(t.Id, out int id) ? id : 0) + 1;
                    }
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