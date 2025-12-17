using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Terrarium.Avalonia.ViewModels.Core;
using Terrarium.Avalonia.ViewModels.Models;
using Terrarium.Core.Enums;
using Terrarium.Core.Interfaces;
using Terrarium.Core.Models;
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
            _gardenEconomyService = GardenEconomyService.Instance;
            AddItemCommand = new RelayCommand(ExecuteAddItem);
            DeleteTaskCommand = new RelayCommand(ExecuteDeleteTask, CanExecuteDeleteTask);
            SelectTaskCommand = new RelayCommand(ExecuteSelectTask);
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

        public void CloseDetails() => SelectedTask = null;

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
                    Content = "New Task",
                    Tag = "New",
                    Priority = TaskPriority.Low,
                    DueDate = DateTime.Now
                };

                var newTask = new TaskItem(newEntity);
                targetColumn.Tasks.Insert(0, newTask);
                SelectedTask = newTask;

                await _boardService.AddTaskAsync(newEntity, targetColumn.Id);
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