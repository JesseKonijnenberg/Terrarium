using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Input.Platform;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Terrarium.Avalonia.Models.Kanban;
using Terrarium.Avalonia.ViewModels.Core;
using Terrarium.Core.Interfaces.Context;
using Terrarium.Core.Interfaces.Kanban;
using Terrarium.Core.Messages;
using Terrarium.Core.Models.Context;

namespace Terrarium.Avalonia.ViewModels;

public partial class KanbanBoardViewModel : ViewModelBase, IRecipient<BoardUpdatedMessage>
{
    private readonly IBoardService _boardService;
    private readonly IProjectContextService _contextService;
    
    private bool _isLocalInteractionInProgress;
    
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private string? _statusMessage;
    [ObservableProperty] private string? _currentWorkspaceId;
    [ObservableProperty] private string? _currentProjectId;
    [ObservableProperty] private bool _isDeleteAllConfirmationOpen;

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
        
        WeakReferenceMessenger.Default.RegisterAll(this);
        
        _ = LoadDataAsync();
    }
    
    public void Receive(BoardUpdatedMessage message)
    {
        if (_isLocalInteractionInProgress) return;

        if (string.IsNullOrEmpty(message.ProjectId) || message.ProjectId == CurrentProjectId)
        {
            _ = MergeDataAsync();
        }
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
        IsLoading = true;
        StatusMessage = "Loading Board...";
        
        await Dispatcher.UIThread.InvokeAsync(() => Columns.Clear());

        try
        {
            var workspaceId = CurrentWorkspaceId;
            var projectId = CurrentProjectId;
            
            if (string.IsNullOrEmpty(workspaceId) || string.IsNullOrEmpty(projectId))
            {
                StatusMessage = "Please select a Project from the sidebar to view the board.";
                return;
            }

            var board = await _boardService.GetBoardAsync(workspaceId, projectId);
            if (board == null)
            {
                StatusMessage = "No Kanban board found for this project.";
                return;
            }
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
                foreach (var col in preparedColumns) Columns.Add(col);
            });
            
            StatusMessage = null; 
        }
        catch (Exception ex)
        {
            StatusMessage = $"Unable to load board.\nError: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    public async Task MergeDataAsync()
    {
        if (_isLocalInteractionInProgress) return;

        try
        {
            var workspaceId = CurrentWorkspaceId;
            var projectId = CurrentProjectId;
            if (string.IsNullOrEmpty(projectId)) return;

            var freshBoard = await _boardService.GetBoardAsync(workspaceId, projectId);
            if (freshBoard == null) return;

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                foreach (var freshColumn in freshBoard.Columns)
                {
                    var uiColumn = Columns.FirstOrDefault(c => c.Id == freshColumn.Id);
                    if (uiColumn == null) continue; 
                    
                    var freshIds = freshColumn.Tasks.Select(t => t.Id).ToHashSet();
                    var toRemove = uiColumn.Tasks.Where(t => !freshIds.Contains(t.Id)).ToList();
                    foreach (var t in toRemove) uiColumn.Tasks.Remove(t);
                    
                    foreach (var freshTask in freshColumn.Tasks)
                    {
                        var uiTask = uiColumn.Tasks.FirstOrDefault(t => t.Id == freshTask.Id);
                        if (uiTask != null)
                        {
                            uiTask.Entity.Title = freshTask.Title;
                            uiTask.Entity.Description = freshTask.Description;
                            uiTask.Entity.Tag = freshTask.Tag;
                            uiTask.Entity.Priority = freshTask.Priority;
                            uiTask.Entity.DueDate = freshTask.DueDate;
                            
                            uiTask.Entity.Order = freshTask.Order;
                            uiTask.Entity.ColumnId = freshTask.ColumnId;
                            
                            uiTask.RefreshFromEntity();
                        }
                        else
                        {
                            uiColumn.Tasks.Add(new TaskItem(freshTask));
                        }
                    }
                    
                    var sorted = uiColumn.Tasks.OrderBy(t => t.Entity.Order).ToList();
                    for (int i = 0; i < sorted.Count; i++)
                    {
                        var t = sorted[i];
                        if (uiColumn.Tasks.IndexOf(t) != i)
                        {
                            uiColumn.Tasks.Move(uiColumn.Tasks.IndexOf(t), i);
                        }
                    }
                }
            });
        }
        catch (Exception ex)
        {
            StatusMessage = $"Sync error: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task AddItemAsync(Column targetColumn)
    {
        if (string.IsNullOrEmpty(CurrentWorkspaceId) || string.IsNullOrEmpty(CurrentProjectId)) 
            return;
        
        try 
        {
            _isLocalInteractionInProgress = true; // Block Updates

            var newEntity = await _boardService.CreateDefaultTaskEntity(
                targetColumn.Id, 
                CurrentWorkspaceId, 
                CurrentProjectId);

            // Optimistic UI Add
            var newTask = new TaskItem(newEntity);
            targetColumn.Tasks.Add(newTask);
            OpenedTask = newTask;
        
            await _boardService.AddTaskAsync(newEntity, targetColumn.Id, CurrentWorkspaceId, CurrentProjectId);
        }
        finally
        {
            _isLocalInteractionInProgress = false; // Unblock
        }
    }

    [RelayCommand(CanExecute = nameof(CanDeleteTask))]
    private async Task DeleteTaskAsync()
    {
        if (OpenedTask is not { } taskToDelete) return;

        try
        {
            _isLocalInteractionInProgress = true;

            var sourceColumn = Columns.FirstOrDefault(c => c.Tasks.Contains(taskToDelete));
            if (sourceColumn != null)
            {
                sourceColumn.Tasks.Remove(taskToDelete);
                OpenedTask = null;
                await _boardService.DeleteTaskAsync(taskToDelete.Entity);
            }
        }
        finally
        {
            _isLocalInteractionInProgress = false;
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
    }

    [RelayCommand]
    private async Task ConfirmDeleteAllAsync()
    {
        IsDeleteAllConfirmationOpen = false;
        if (CurrentProjectId != null)
        {
            try 
            {
                _isLocalInteractionInProgress = true;
                
                foreach (var column in Columns) column.Tasks.Clear();
                OpenedTask = null;
                
                await _boardService.WipeBoardAsync(CurrentProjectId);
            }
            finally
            {
                _isLocalInteractionInProgress = false;
            }
        }
    }

    [RelayCommand(CanExecute = nameof(CanDeleteSelected))]
    private async Task DeleteSelectedTasksAsync()
    {
        var idsToDelete = SelectedTaskIds.ToList();
        if (OpenedTask != null && idsToDelete.Contains(OpenedTask.Id)) OpenedTask = null;
        
        try
        {
            _isLocalInteractionInProgress = true;

            foreach (var column in Columns)
            {
                var toRemove = column.Tasks.Where(t => idsToDelete.Contains(t.Id)).ToList();
                foreach (var task in toRemove) column.Tasks.Remove(task);
            }
            
            await _boardService.DeleteMultipleTasksAsync(idsToDelete);
            
            SelectedTaskIds.Clear();
            SelectionCount = 0;
        }
        finally
        {
            _isLocalInteractionInProgress = false;
        }
    }
    private bool CanDeleteSelected() => !IsDetailPanelOpen && SelectionCount > 0;

    [RelayCommand]
    private void OpenTask(TaskItem task)
    {
        DeselectAll();
        task.IsSelected = true;
        SelectedTaskIds.Add(task.Id);
        OpenedTask = task;
        SelectionCount = SelectedTaskIds.Count;
    }
    
    [RelayCommand]
    private async Task SaveTaskAsync()
    {
        if (OpenedTask == null) return;
    
        try 
        {
            _isLocalInteractionInProgress = true;
            await _boardService.UpdateTaskAsync(OpenedTask.Entity);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to save: {ex.Message}";
        }
        finally
        {
            _isLocalInteractionInProgress = false;
        }
    }

    public async Task MoveTaskAsync(TaskItem task, Column targetColumn, int index = -1)
    {
        // Snapshot items to move
        var tasksToMove = task.IsSelected 
            ? Columns.SelectMany(c => c.Tasks).Where(t => t.IsSelected).ToList()
            : new List<TaskItem> { task };
        var ids = tasksToMove.Select(t => t.Id).ToList();

        // Calculate Adjustment (Handle items removed from above target)
        var itemsAndIndices = tasksToMove.Select(t => new { 
            Task = t, 
            Col = Columns.FirstOrDefault(c => c.Tasks.Contains(t)),
            Idx = Columns.FirstOrDefault(c => c.Tasks.Contains(t))?.Tasks.IndexOf(t) ?? -1
        }).ToList();

        // Purge (Collapse list) - This stabilizes indices
        foreach (var item in itemsAndIndices) item.Col?.Tasks.Remove(item.Task);

        // Calculate Clean Insertion Index
        int cleanIndex = index;
        if (cleanIndex == -1 || cleanIndex > targetColumn.Tasks.Count) cleanIndex = targetColumn.Tasks.Count;
        else 
        {
            int adjustment = itemsAndIndices.Count(x => x.Col == targetColumn && x.Idx != -1 && x.Idx < index);
            cleanIndex -= adjustment;
        }
        
        // Clamp
        if (cleanIndex < 0) cleanIndex = 0;
        if (cleanIndex > targetColumn.Tasks.Count) cleanIndex = targetColumn.Tasks.Count;

        int finalServerIndex = cleanIndex;

        // Place (Insert) & Update Local Data (True Optimistic UI)
        foreach (var t in tasksToMove)
        {
            targetColumn.Tasks.Insert(cleanIndex++, t);
            t.Entity.ColumnId = targetColumn.Id;
        }
        
        try
        {
            _isLocalInteractionInProgress = true;
            
            await _boardService.MoveTasksWithEconomyAsync(ids, targetColumn.Id, targetColumn.Title, finalServerIndex);
        }
        finally
        {
            _isLocalInteractionInProgress = false;
        }
    }

    [RelayCommand] 
    public async Task CloseDetailsAsync()
    {
        await SaveTaskAsync();
        OpenedTask = null;
    }

    [RelayCommand]
    public void DeselectAll()
    {
        OpenedTask = null; 
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