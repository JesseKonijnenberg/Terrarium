#region File Header & Imports

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

#endregion

namespace Terrarium.Avalonia.ViewModels;

public class KanbanBoardViewModel : ViewModelBase
{
#region Fields & Services
    private readonly IBoardService _boardService;
#endregion

#region UI Collections & State
    public UpdateViewModel Updater { get; }
    public ObservableCollection<Column> Columns { get; set; } = new();
    public ObservableHashSet<string> SelectedTaskIds { get; } = new();

    public TaskItem? OpenedTask
    {
        get;
        set
        {
            if (field != null) SaveTask(field);
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
#endregion

#region Commands
    public ICommand AddItemCommand { get; private set; }
    public ICommand DeleteTaskCommand { get; private set; }
    public ICommand OpenTaskCommand { get; private set; }
    public ICommand SaveTaskCommand { get; private set; }
    public ICommand SmartPasteCommand { get; private set; }
    public ICommand SelectAllTasksCommand { get; private set; }
    public ICommand DeleteAllTasksCommand { get; private set; }
    public ICommand ConfirmDeleteAllCommand { get; private set; }
    public ICommand CancelDeleteAllCommand { get; private set; }
    public ICommand ToggleTaskSelectionCommand { get; private set; }
    public ICommand DeleteSelectedTasksCommand { get; private set; }
    public ICommand DeselectAllCommand { get; private set; }
#endregion

#region Constructor & Initialization
    public KanbanBoardViewModel(IBoardService boardService, IUpdateService updateService)
    {
        _boardService = boardService;
        Updater = new UpdateViewModel(updateService);

        InitializeCommands();
        LoadData();
    }

    private void InitializeCommands()
    {
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
            _ => !IsDetailPanelOpen);

        DeleteSelectedTasksCommand = new RelayCommand(
            ExecuteDeleteSelected, 
            _ => !IsDetailPanelOpen && SelectedTaskIds.Any());

        SelectAllTasksCommand = new RelayCommand(
            ExecuteSelectAll,
            _ => !IsDetailPanelOpen);

        DeselectAllCommand = new RelayCommand(ExecuteDeselectAll);
    }
#endregion

#region Task Operations (CRUD & Move)
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

    private async void ExecuteSaveTask(object? parameter) 
    { 
        if (OpenedTask != null) SaveTask(OpenedTask); 
    }

    private async void SaveTask(TaskItem task) => 
        await _boardService.UpdateTaskFromUiAsync(task.Entity, task.Title, task.Description, task.Tag, task.Priority, task.Date);

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

    private async void ExecuteDeleteSelected(object? parameter)
    {
        if (IsDetailPanelOpen || !SelectedTaskIds.Any()) return;

        var idsToDelete = SelectedTaskIds.ToList();
        if (OpenedTask != null && idsToDelete.Contains(OpenedTask.Id)) OpenedTask = null;
        
        await _boardService.DeleteMultipleTasksAsync(idsToDelete);
        
        foreach (var column in Columns)
        {
            var toRemove = column.Tasks.Where(t => idsToDelete.Contains(t.Id)).ToList();
            foreach (var task in toRemove) column.Tasks.Remove(task);
        }
        SelectedTaskIds.Clear();
        RefreshCommandStates();
    }

    private async void ExecuteConfirmDeleteAll(object? parameter)
    {
        IsDeleteAllConfirmationOpen = false;
        await _boardService.WipeBoardAsync();
        foreach (var column in Columns) column.Tasks.Clear();
        OpenedTask = null;
    }
#endregion

#region Selection & UI Navigation
    private void ExecuteOpenTask(object? parameter)
    {
        if (parameter is TaskItem task) 
        {
            task.IsSelected = true;
            if (!SelectedTaskIds.Contains(task.Id)) SelectedTaskIds.Add(task.Id);
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

    private void ExecuteToggleSelection(object? parameter)
    {
        if (parameter is not TaskItem task || (IsDetailPanelOpen && OpenedTask?.Id == task.Id)) return;

        task.IsSelected = !task.IsSelected;
        if (task.IsSelected) SelectedTaskIds.Add(task.Id);
        else SelectedTaskIds.Remove(task.Id);

        RefreshCommandStates();
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
        RefreshCommandStates();
    }

    private void ExecuteDeselectAll(object? parameter)
    {
        if (IsDeleteAllConfirmationOpen) return;
        if (IsDetailPanelOpen) { CloseDetails(); return; }
        
        SelectedTaskIds.Clear();
        foreach (var column in Columns)
            foreach (var task in column.Tasks) task.IsSelected = false;

        RefreshCommandStates();
    }
#endregion

#region Data Loading & External Input
    private async void LoadData()
    {
        var boardData = await _boardService.LoadBoardAsync();
        Columns.Clear();
        foreach (var colEntity in boardData)
        {
            var columnVm = new Column(colEntity);
            foreach (var taskEntity in colEntity.Tasks) columnVm.Tasks.Add(new TaskItem(taskEntity));
            Columns.Add(columnVm);
        }
    }

    private async void ExecuteSmartPaste(object? parameter)
    {
        if (parameter is not IClipboard clipboard) return;
        var text = await clipboard.GetTextAsync();
        var newTasks = await _boardService.ProcessSmartPasteAsync(text);

        foreach (var entity in newTasks)
        {
            var uiColumn = Columns.FirstOrDefault(c => c.Id == entity.ColumnId);
            if (uiColumn != null) uiColumn.Tasks.Add(new TaskItem(entity));
        }
    }
#endregion

#region Helpers
    private void RefreshCommandStates() => ((RelayCommand)DeleteSelectedTasksCommand).RaiseCanExecuteChanged();
#endregion
}