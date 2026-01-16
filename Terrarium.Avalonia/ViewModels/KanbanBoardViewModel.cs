using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Input.Platform;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Terrarium.Avalonia.Models.Kanban;
using Terrarium.Avalonia.ViewModels.Core;
using Terrarium.Core.Interfaces.Context;
using Terrarium.Core.Interfaces.Kanban;
using Terrarium.Core.Models.Context;

namespace Terrarium.Avalonia.ViewModels;

public partial class KanbanBoardViewModel : ViewModelBase
{
    private readonly IBoardService _boardService;
    private readonly IProjectContextService _contextService;

    [ObservableProperty] 
    private string? _currentWorkspaceId;

    [ObservableProperty]
    private string? _currentProjectId;
    
    [ObservableProperty]
    private bool _isDeleteAllConfirmationOpen;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDetailPanelOpen))]
    [NotifyCanExecuteChangedFor(nameof(DeleteTaskCommand))]
    private TaskItem? _openedTask;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeleteSelectedTasksCommand))]
    private int _selectionCount;
    
    public ObservableCollection<Column> Columns { get; set; } = new();
    public ObservableHashSet<string> SelectedTaskIds { get; } = new();

    public bool IsDetailPanelOpen => OpenedTask != null;

    public KanbanBoardViewModel(IBoardService boardService, IProjectContextService contextService)
    {
        _boardService = boardService;
        _contextService = contextService;
        
        UpdateLocalState(_contextService.CurrentContext);
        _contextService.ContextChanged += OnContextChanged;
        
        _ = LoadDataAsync();
    }
    

    private void OnContextChanged(ProjectContext context)
    {
        UpdateLocalState(context);
        _ = LoadDataAsync();
    }
    
    private void UpdateLocalState(ProjectContext? context)
    {
        if (context == null) return;
        CurrentWorkspaceId = context.WorkspaceId ?? string.Empty;
        CurrentProjectId = context.ProjectId;
    }
    
    public async Task LoadDataAsync()
    {
        var workspaceId = CurrentWorkspaceId;
        var projectId = CurrentProjectId;
        
        if (string.IsNullOrEmpty(workspaceId) || string.IsNullOrEmpty(projectId)) 
        {
            return;
        }
        
        var board = await _boardService.GetBoardAsync(workspaceId, projectId);
        
        var preparedColumns = new List<Column>();
    
        if (board?.Columns != null)
        {
            foreach (var colEntity in board.Columns)
            {
                var columnVm = new Column(colEntity);
                foreach (var taskEntity in colEntity.Tasks) 
                {
                    columnVm.Tasks.Add(new TaskItem(taskEntity));
                }
                preparedColumns.Add(columnVm);
            }
        }
        
        await Dispatcher.UIThread.InvokeAsync(() => 
        {
            Columns.Clear();
            foreach (var col in preparedColumns)
            {
                Columns.Add(col);
            }
        });
    }

    [RelayCommand]
    private async Task AddItemAsync(Column targetColumn)
    {
        if (string.IsNullOrEmpty(CurrentWorkspaceId) || string.IsNullOrEmpty(CurrentProjectId)) 
            return;
        
        var newEntity = await _boardService.CreateDefaultTaskEntity(
            targetColumn.Id, 
            CurrentWorkspaceId, 
            CurrentProjectId);
    
        await _boardService.AddTaskAsync(newEntity, targetColumn.Id, CurrentWorkspaceId, CurrentProjectId);
        
        var newTask = new TaskItem(newEntity);
        targetColumn.Tasks.Add(newTask);
        OpenedTask = newTask;
    }

    [RelayCommand(CanExecute = nameof(CanDeleteTask))]
    private async Task DeleteTaskAsync()
    {
        if (OpenedTask is not { } taskToDelete) return;

        var sourceColumn = Columns.FirstOrDefault(c => c.Tasks.Contains(taskToDelete));
        if (sourceColumn != null)
        {
            sourceColumn.Tasks.Remove(taskToDelete);
            OpenedTask = null;
            await _boardService.DeleteTaskAsync(taskToDelete.Entity);
        }
    }
    private bool CanDeleteTask() => OpenedTask != null;

    [RelayCommand]
    private async Task SmartPasteAsync(IClipboard clipboard)
    {
        if (string.IsNullOrEmpty(CurrentWorkspaceId) || string.IsNullOrEmpty(CurrentProjectId)) 
            return;
        
        var text = await clipboard.GetTextAsync();
        if (string.IsNullOrWhiteSpace(text)) return;
        
        var newTasks = await _boardService.ProcessSmartPasteAsync(text, CurrentWorkspaceId, CurrentProjectId);
        foreach (var entity in newTasks)
        {
            var uiColumn = Columns.FirstOrDefault(c => c.Id == entity.ColumnId);
            uiColumn?.Tasks.Add(new TaskItem(entity));
        }
    }

    [RelayCommand]
    private async Task ConfirmDeleteAllAsync()
    {
        IsDeleteAllConfirmationOpen = false;
        if (CurrentProjectId != null)
        {
            await _boardService.WipeBoardAsync(CurrentProjectId);
            foreach (var column in Columns) column.Tasks.Clear();
            OpenedTask = null;
        }
    }

    [RelayCommand(CanExecute = nameof(CanDeleteSelected))]
    private async Task DeleteSelectedTasksAsync()
    {
        var idsToDelete = SelectedTaskIds.ToList();
        if (OpenedTask != null && idsToDelete.Contains(OpenedTask.Id)) OpenedTask = null;
        
        await _boardService.DeleteMultipleTasksAsync(idsToDelete);
        
        foreach (var column in Columns)
        {
            var toRemove = column.Tasks.Where(t => idsToDelete.Contains(t.Id)).ToList();
            foreach (var task in toRemove) column.Tasks.Remove(task);
        }
    
        SelectedTaskIds.Clear();
        SelectionCount = 0;
    }
    private bool CanDeleteSelected() => !IsDetailPanelOpen && SelectionCount > 0;

    [RelayCommand]
    private void OpenTask(TaskItem task)
    {
        DeselectAll(); // Focus on the opened task
        task.IsSelected = true;
        SelectedTaskIds.Add(task.Id);
        OpenedTask = task;
        SelectionCount = SelectedTaskIds.Count;
    }

    public async Task MoveTaskAsync(TaskItem task, Column targetColumn, int index = -1)
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
    
    [RelayCommand] 
    public void CloseDetails() => OpenedTask = null;

    [RelayCommand]
    public void DeselectAll()
    {
        CloseDetails();
        SelectedTaskIds.Clear();
        foreach (var t in Columns.SelectMany(c => c.Tasks)) t.IsSelected = false;
        SelectionCount = 0;
    }
    
    [RelayCommand(CanExecute = nameof(CanModifyBoard))]
    private void SelectAllTasks()
    {
        SelectedTaskIds.Clear();
        foreach (var task in Columns.SelectMany(c => c.Tasks))
        {
            SelectedTaskIds.Add(task.Id);
            task.IsSelected = true;
        }
        SelectionCount = SelectedTaskIds.Count;
    }

    [RelayCommand(CanExecute = nameof(CanModifyBoard))]
    private void DeleteAllTasks() 
    {
        IsDeleteAllConfirmationOpen = true;
    }

    [RelayCommand]
    private void CancelDeleteAll() 
    {
        IsDeleteAllConfirmationOpen = false;
    }

    [RelayCommand]
    private void ToggleTaskSelection(TaskItem task)
    {
        if (IsDetailPanelOpen && OpenedTask?.Id == task.Id) return;

        task.IsSelected = !task.IsSelected;
        
        if (task.IsSelected) 
            SelectedTaskIds.Add(task.Id);
        else 
            SelectedTaskIds.Remove(task.Id);

        SelectionCount = SelectedTaskIds.Count;
    }

    private bool CanModifyBoard() => !IsDetailPanelOpen;
}