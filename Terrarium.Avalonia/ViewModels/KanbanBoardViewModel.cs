using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Terrarium.Avalonia.ViewModels.Core;
using Terrarium.Avalonia.ViewModels.Models;
using Terrarium.Core.Enums.Kanban;
using Terrarium.Core.Interfaces.Garden;
using Terrarium.Core.Interfaces.Kanban;
using Terrarium.Core.Models.Kanban;
using Terrarium.Logic.Services;

namespace Terrarium.Avalonia.ViewModels
{
    public class KanbanBoardViewModel : ViewModelBase
    {

        private readonly IBoardService _boardService;
        private readonly IGardenEconomyService _gardenEconomyService;

        public UpdateViewModel Updater { get; } = new UpdateViewModel();
        public ICommand AddItemCommand { get; }
        public ICommand DeleteTaskCommand { get; }
        public ICommand SelectTaskCommand { get; }
        public ICommand SaveTaskCommand { get; }

        public ObservableCollection<Column> Columns { get; set; } = new();

        private TaskItem? _selectedTask;
        public TaskItem? SelectedTask
        {
            get => _selectedTask;
            set
            {
                if (_selectedTask != null)
                {
                    SaveTask(_selectedTask);
                }
                _selectedTask = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsDetailPanelOpen));
            }
        }
        public bool IsDetailPanelOpen => SelectedTask != null;

        public KanbanBoardViewModel(IBoardService boardService, IGardenEconomyService gardenEconomyService)
        {
            _boardService = boardService;
            _gardenEconomyService = gardenEconomyService;

            AddItemCommand = new RelayCommand(ExecuteAddItem);
            DeleteTaskCommand = new RelayCommand(ExecuteDeleteTask, CanExecuteDeleteTask);
            SelectTaskCommand = new RelayCommand(ExecuteSelectTask);
            SaveTaskCommand = new RelayCommand(ExecuteSaveTask);
            LoadData();
        }

        public async void MoveTask(TaskItem task, Column targetColumn, int index = -1)
        {
            var sourceColumn = Columns.FirstOrDefault(c => c.Tasks.Contains(task));

            if (sourceColumn != null)
            {
                sourceColumn.Tasks.Remove(task);

                if (index == -1 || index > targetColumn.Tasks.Count)
                {
                    targetColumn.Tasks.Add(task);
                }
                else
                {
                    targetColumn.Tasks.Insert(index, task);
                }
                if (targetColumn.Title == "Complete" || targetColumn.Title == "Done")
                {
                    _gardenEconomyService.EarnWater(20);
                }

                await _boardService.MoveTaskAsync(task.Entity, targetColumn.Id, index);
            }
        }

        private void ExecuteSelectTask(object? parameter)
        {
            if (parameter is TaskItem task) SelectedTask = task;
        }

        public void CloseDetails()
        {
            if (SelectedTask != null)
            {
                SaveTask(SelectedTask);
            }
            SelectedTask = null;
        }

        private void ExecuteSaveTask(object? parameter)
        {
            if (SelectedTask != null)
            {
                SaveTask(SelectedTask);
            }
        }

        private async void SaveTask(TaskItem task)
        {
            task.Entity.Title = task.Title;
            task.Entity.Tag = task.Tag;
            task.Entity.Description = task.Description;

            if (Enum.TryParse(typeof(TaskPriority), task.Priority, true, out var result))
            {
                task.Entity.Priority = (TaskPriority)result;
            }
            else
            {
                task.Entity.Priority = TaskPriority.Low; // Default fallback
            }
            if (DateTime.TryParse(task.Date, out var dateResult))
            {
                task.Entity.DueDate = dateResult;
            }

            await _boardService.UpdateTaskAsync(task.Entity);
        }

        private async void ExecuteDeleteTask(object? parameter)
        {
            if (SelectedTask is TaskItem taskToDelete)
            {
                var sourceColumn = Columns.FirstOrDefault(c => c.Tasks.Contains(taskToDelete));
                if (sourceColumn != null)
                {
                    sourceColumn.Tasks.Remove(taskToDelete);
                    SelectedTask = null;

                    await _boardService.DeleteTaskAsync(taskToDelete.Entity);
                }
            }
        }

        private bool CanExecuteDeleteTask(object? parameter) => SelectedTask != null;

        private async void ExecuteAddItem(object? parameter)
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
                    Title = "New Task",
                    Tag = "New",
                    Description = "",
                    Priority = TaskPriority.Low,
                    DueDate = DateTime.Now
                };

                var newTask = new TaskItem(newEntity);
                targetColumn.Tasks.Insert(0, newTask);
                SelectedTask = newTask;

                await _boardService.AddTaskAsync(newEntity, targetColumn.Id);
            }
        }

        private async void LoadData()
        {
            var boardData = await ((BoardService)_boardService).LoadBoardAsync();

            Columns.Clear();
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