using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Terrarium.Avalonia.ViewModels.Core;
using Terrarium.Avalonia.ViewModels.Models;
using Terrarium.Core.Enums; // Ensure you have this namespace for TaskPriority if needed
using Terrarium.Core.Interfaces;
using Terrarium.Core.Models;
using Terrarium.Logic.Services;

namespace Terrarium.Avalonia.ViewModels
{
    public class KanbanBoardViewModel : ViewModelBase
    {
        private readonly IBoardService _boardService;

        public UpdateViewModel Updater { get; } = new UpdateViewModel();
        public ICommand AddItemCommand { get; }
        public ICommand DeleteTaskCommand { get; }
        public ICommand SelectTaskCommand { get; }

        public ObservableCollection<Column> Columns { get; set; } = new();

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
            _boardService = new BoardService();
            AddItemCommand = new RelayCommand(ExecuteAddItem);
            DeleteTaskCommand = new RelayCommand(ExecuteDeleteTask, CanExecuteDeleteTask);
            SelectTaskCommand = new RelayCommand(ExecuteSelectTask);
            LoadData();
        }

        // --- FIXED MOVE TASK METHOD ---
        // 1. Added 'int index' so we can drop items in specific spots
        // 2. Combined the Logic (Service) and UI updates into ONE method
        public void MoveTask(TaskItem task, Column targetColumn, int index = -1)
        {
            var sourceColumn = Columns.FirstOrDefault(c => c.Tasks.Contains(task));

            if (sourceColumn != null)
            {
                // Case A: Reordering within the SAME column
                if (sourceColumn == targetColumn)
                {
                    var oldIndex = sourceColumn.Tasks.IndexOf(task);

                    // If index is -1 (append) or invalid, put it at the end
                    if (index == -1 || index >= sourceColumn.Tasks.Count)
                        index = sourceColumn.Tasks.Count - 1;

                    if (oldIndex != index)
                    {
                        sourceColumn.Tasks.Move(oldIndex, index);
                        // Optional: Call _boardService.ReorderTask(...) if your backend supports it
                    }
                }
                // Case B: Moving to DIFFERENT column
                else
                {
                    // 1. Update UI
                    sourceColumn.Tasks.Remove(task);

                    if (index == -1 || index > targetColumn.Tasks.Count)
                    {
                        targetColumn.Tasks.Add(task);
                    }
                    else
                    {
                        targetColumn.Tasks.Insert(index, task);
                    }

                    // 2. Update Backend (Service)
                    // Note: Your Service implementation might need to support index/insertions later
                    _boardService.RemoveTaskFromColumn(sourceColumn.Entity, task.Entity);
                    _boardService.AddTaskToColumn(targetColumn.Entity, task.Entity);
                }
            }
        }

        private void ExecuteSelectTask(object? parameter)
        {
            if (parameter is TaskItem task) SelectedTask = task;
        }

        public void CloseDetails() => SelectedTask = null;

        private void ExecuteDeleteTask(object? parameter)
        {
            if (SelectedTask is TaskItem taskToDelete)
            {
                var sourceColumn = Columns.FirstOrDefault(c => c.Tasks.Contains(taskToDelete));
                if (sourceColumn != null)
                {
                    sourceColumn.Tasks.Remove(taskToDelete);
                    SelectedTask = null;
                    _boardService.RemoveTaskFromColumn(sourceColumn.Entity, taskToDelete.Entity);
                }
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

                var newEntity = new TaskEntity
                {
                    Id = nextId.ToString(),
                    Content = "New Task",
                    Tag = "New",
                    Priority = TaskPriority.Low,
                    DueDate = DateTime.Now
                };

                var newTask = new TaskItem(newEntity);
                targetColumn.Tasks.Insert(0, newTask);
                SelectedTask = newTask;

                _boardService.AddTaskToColumn(targetColumn.Entity, newEntity);
            }
        }

        private void LoadData()
        {
            var boardData = _boardService.GetFullBoard();
            foreach (var colEntity in boardData)
            {
                var columnVm = new Column(colEntity);
                foreach (var taskEntity in colEntity.Tasks)
                {
                    columnVm.Tasks.Add(new TaskItem(taskEntity));
                }
                Columns.Add(columnVm);
            }
        }
    }
}