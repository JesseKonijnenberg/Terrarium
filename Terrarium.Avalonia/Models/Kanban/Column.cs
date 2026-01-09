#region File Header & Imports

using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Terrarium.Avalonia.ViewModels.Core;
using Terrarium.Core.Models.Kanban;

#endregion

namespace Terrarium.Avalonia.Models.Kanban;

/// <summary>
/// A reactive wrapper for ColumnEntity that manages a collection of TaskItems.
/// </summary>
public partial class Column : ViewModelBase
{
    public ColumnEntity Entity { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TaskCount))]
    private string _title;

    public ObservableCollection<TaskItem> Tasks { get; } = new();

    /// <summary>
    /// Calculated property that updates automatically when the Tasks collection changes.
    /// </summary>
    public int TaskCount => Tasks.Count;

    public string Id => Entity.Id;

    public Column(ColumnEntity entity)
    {
        Entity = entity ?? throw new ArgumentNullException(nameof(entity));
        
        _title = entity.Title;
        
        Tasks.CollectionChanged += (s, e) => OnPropertyChanged(nameof(TaskCount));
    }

    #region Toolkit Generated Hooks
    /// <summary>
    /// Synchronizes UI title changes back to the underlying database entity.
    /// </summary>
    partial void OnTitleChanged(string value) => Entity.Title = value;
    #endregion
}