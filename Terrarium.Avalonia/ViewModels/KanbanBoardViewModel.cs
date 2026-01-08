using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Avalonia.Input.Platform;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Terrarium.Avalonia.Models.Kanban;
using Terrarium.Avalonia.ViewModels.Core;
using Terrarium.Core.Interfaces.Kanban;
using Terrarium.Core.Interfaces.Update;

namespace Terrarium.Avalonia.ViewModels
{
    public class KanbanBoardViewModel : ViewModelBase
    {

        private readonly IBoardService _boardService;
        
        public ICommand AddItemCommand { get; }
        public ICommand DeleteTaskCommand { get; }
        public ICommand OpenTaskCommand { get; }
        public ICommand SaveTaskCommand { get; }
        public ICommand SmartPasteCommand { get; }
        public ICommand SelectAllTasksCommand { get; }
        public ICommand DeleteAllTasksCommand { get; }
        public ICommand ConfirmDeleteAllCommand { get; }
        public ICommand CancelDeleteAllCommand { get; }
        public ICommand ToggleTaskSelectionCommand { get; }
        public ICommand DeleteSelectedTasksCommand { get; }
        public ICommand DeselectAllCommand { get; }

        public UpdateViewModel Updater { get; }
        public ObservableCollection<Column> Columns { get; set; } = new();
        public ObservableHashSet<string> SelectedTaskIds { get; } = new();
        

        public TaskItem? OpenedTask
        {
            get;
            set
            {
                if (field != null)
                {
                    SaveTask(field);
                }
                field = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsDetailPanelOpen));
            }
        }
        public bool IsDeleteAllConfirmationOpen
        {
            get;
            set { field = value; OnPropertyChanged(); }
        }
        
        public bool IsDetailPanelOpen => OpenedTask != null;

        public KanbanBoardViewModel(
            IBoardService boardService, 
            IUpdateService updateService)
        {
            _boardService = boardService;
            
            Updater = new UpdateViewModel(updateService);

            AddItemCommand = new RelayCommand(ExecuteAddItem);
            DeleteTaskCommand = new RelayCommand(ExecuteDeleteTask, CanExecuteDeleteTask);
            OpenTaskCommand = new RelayCommand(ExecuteOpenTask);
            SaveTaskCommand = new RelayCommand(ExecuteSaveTask);
            SmartPasteCommand = new RelayCommand(ExecuteSmartPaste);
            ConfirmDeleteAllCommand = new RelayCommand(ExecuteConfirmDeleteAll);
            CancelDeleteAllCommand = new RelayCommand(_ => IsDeleteAllConfirmationOpen = false);
            ToggleTaskSelectionCommand = new RelayCommand(ExecuteToggleSelection);
            DeleteAllTasksCommand = new RelayCommand(
                _ => IsDeleteAllConfirmationOpen = true,
                _ => !IsDetailPanelOpen
            );
            DeleteSelectedTasksCommand = new RelayCommand(
                ExecuteDeleteSelected, 
                _ => !IsDetailPanelOpen && SelectedTaskIds.Any()
            );
            SelectAllTasksCommand = new RelayCommand(
                ExecuteSelectAll,
                _ => !IsDetailPanelOpen
            );
            DeselectAllCommand = new RelayCommand(ExecuteDeselectAll);
            
            LoadData();
        }

        public async void MoveTask(TaskItem task, Column targetColumn, int index = -1)
        {
            var tasksToMove = task.IsSelected 
                ? Columns.SelectMany(c => c.Tasks).Where(t => t.IsSelected).ToList()
                : new List<TaskItem> { task };

            var ids = tasksToMove.Select(t => t.Id).ToList();
            
            foreach (var t in tasksToMove)
            {
                var sourceCol = Columns.FirstOrDefault(c => c.Tasks.Contains(t));
                sourceCol?.Tasks.Remove(t);
                if (index == -1 || index > targetColumn.Tasks.Count)
                    targetColumn.Tasks.Add(t);
                else
                    targetColumn.Tasks.Insert(index++, t);
            }
            
            await _boardService.MoveTasksWithEconomyAsync(ids, targetColumn.Id, targetColumn.Title, index);
        }

        private void ExecuteOpenTask(object? parameter)
        {
            if (parameter is TaskItem task) 
            {
                task.IsSelected = true;
                if (!SelectedTaskIds.Contains(task.Id))
                {
                    SelectedTaskIds.Add(task.Id);
                }

                OpenedTask = task;
            }
        }

        public void CloseDetails()
        {
            if (OpenedTask != null)
            {
                OpenedTask.IsSelected = false;
                SelectedTaskIds.Remove(OpenedTask.Id);
        
                SaveTask(OpenedTask);
            }
            OpenedTask = null;
        }

        private void ExecuteSaveTask(object? parameter)
        {
            if (OpenedTask != null)
            {
                SaveTask(OpenedTask);
            }
        }

        private async void SaveTask(TaskItem task)
        {
            await _boardService.UpdateTaskFromUiAsync(
                task.Entity, 
                task.Title, 
                task.Description, 
                task.Tag, 
                task.Priority, 
                task.Date);
        }

        private async void ExecuteDeleteTask(object? parameter)
        {
            if (OpenedTask is TaskItem taskToDelete)
            {
                var sourceColumn = Columns.FirstOrDefault(c => c.Tasks.Contains(taskToDelete));
                if (sourceColumn != null)
                {
                    sourceColumn.Tasks.Remove(taskToDelete);
                    OpenedTask = null;

                    await _boardService.DeleteTaskAsync(taskToDelete.Entity);
                }
            }
        }

        private bool CanExecuteDeleteTask(object? parameter) => OpenedTask != null;

        private async void ExecuteAddItem(object? parameter)
        {
            if (parameter is Column targetColumn)
            {
                var newEntity = _boardService.CreateDefaultTaskEntity(targetColumn.Id).Result;
                var newTask = new TaskItem(newEntity);
        
                targetColumn.Tasks.Insert(0, newTask);
                OpenedTask = newTask;
        
                await _boardService.AddTaskAsync(newEntity, targetColumn.Id);
            }
        }

        private async void LoadData()
        {
            var boardData = await _boardService.LoadBoardAsync();

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
        
        private async void ExecuteConfirmDeleteAll(object? parameter)
        {
            IsDeleteAllConfirmationOpen = false;
            
            await _boardService.WipeBoardAsync();
            
            foreach (var column in Columns)
            {
                column.Tasks.Clear();
            }
    
            OpenedTask = null;
        }
        
        private async void ExecuteSmartPaste(object? parameter)
        {
            if (parameter is not IClipboard clipboard) return;
            var text = await clipboard.GetTextAsync();
            
            var newTasks = await _boardService.ProcessSmartPasteAsync(text);

            foreach (var entity in newTasks)
            {
                var uiColumn = Columns.FirstOrDefault(c => c.Id == entity.ColumnId);
                
                if (uiColumn != null)
                {
                    var newTaskItem = new TaskItem(entity);
                    uiColumn.Tasks.Add(newTaskItem);
                }
            }
        }
        
        private void ExecuteToggleSelection(object? parameter)
        {
            if (parameter is TaskItem task)
            {
                if (IsDetailPanelOpen && OpenedTask?.Id == task.Id) return;
        
                task.IsSelected = !task.IsSelected;
        
                if (task.IsSelected)
                {
                    if (!SelectedTaskIds.Contains(task.Id))
                        SelectedTaskIds.Add(task.Id);
                }
                else
                {
                    SelectedTaskIds.Remove(task.Id);
                }
                (DeleteSelectedTasksCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }
        
        private void ExecuteSelectAll(object? parameter)
        {
            if (IsDetailPanelOpen) return;
            SelectedTaskIds.Clear();
    
            foreach (var column in Columns)
            {
                foreach (var task in column.Tasks)
                {
                    SelectedTaskIds.Add(task.Id);
                    task.IsSelected = true; 
                }
            }
            (DeleteSelectedTasksCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }
        
        private async void ExecuteDeleteSelected(object? parameter)
        {
            if (IsDetailPanelOpen) return;
            if (!SelectedTaskIds.Any()) return;

            var idsToDelete = SelectedTaskIds.ToList();
            
            if (OpenedTask != null && idsToDelete.Contains(OpenedTask.Id))
            {
                OpenedTask = null;
            }
            
            await _boardService.DeleteMultipleTasksAsync(idsToDelete);
            
            foreach (var column in Columns)
            {
                var toRemove = column.Tasks.Where(t => idsToDelete.Contains(t.Id)).ToList();
                foreach (var task in toRemove)
                {
                    column.Tasks.Remove(task);
                }
            }
            SelectedTaskIds.Clear();
            (DeleteSelectedTasksCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }
        
        private void ExecuteDeselectAll(object? parameter)
        {
            if (IsDeleteAllConfirmationOpen)
            {
                return;
            }
            if (IsDetailPanelOpen)
            {
                CloseDetails();
                return;
            }
            
            SelectedTaskIds.Clear();
            foreach (var column in Columns)
            {
                foreach (var task in column.Tasks)
                {
                    task.IsSelected = false;
                }
            }
    
            (DeleteSelectedTasksCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }
    }
}