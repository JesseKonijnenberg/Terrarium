using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Terrarium.Avalonia.ViewModels.Core;
using Terrarium.Core.Models.Kanban;

namespace Terrarium.Avalonia.Models.Kanban;

public partial class Column : ViewModelBase
{
    public ColumnEntity Entity { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TaskCount))]
    private string _title;

    public ObservableCollection<TaskItem> Tasks { get; } = new();

    public int TaskCount => Tasks.Count;
    public string Id => Entity.Id;

    public Column(ColumnEntity entity)
    {
        Entity = entity ?? throw new ArgumentNullException(nameof(entity));
        _title = entity.Title;
        
        Tasks.CollectionChanged += (s, e) => OnPropertyChanged(nameof(TaskCount));
    }

    partial void OnTitleChanged(string value) => Entity.Title = value;
}